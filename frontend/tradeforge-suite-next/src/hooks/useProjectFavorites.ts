import { useCallback, useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Project, ProjectFavoriteRecord } from '@/types/project';
import {
  clearFavorites as clearFavoritesApi,
  deleteFavorite,
  fetchFavorites,
  saveFavorite,
} from '@/services/projectsService';

const CLIENT_ID_STORAGE_KEY = 'pk3000:client-id';
const FAVORITES_QUERY_KEY = 'favorites';

const ensureClientId = (): string => {
  if (typeof window === 'undefined') {
    return '';
  }

  const existing = window.localStorage.getItem(CLIENT_ID_STORAGE_KEY);
  if (existing && existing.trim().length > 0) {
    return existing;
  }

  const generated =
    typeof window.crypto?.randomUUID === 'function'
      ? window.crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`;

  window.localStorage.setItem(CLIENT_ID_STORAGE_KEY, generated);
  return generated;
};

const buildQueryKey = (clientId: string) => [FAVORITES_QUERY_KEY, clientId];

export const useProjectFavorites = () => {
  const [clientId, setClientId] = useState('');
  useEffect(() => {
    setClientId(ensureClientId());
  }, []);
  const hasClientId = useMemo(() => clientId.trim().length > 0, [clientId]);
  const queryClient = useQueryClient();

  const favoritesQuery = useQuery<ProjectFavoriteRecord[]>({
    queryKey: buildQueryKey(clientId),
    queryFn: () => fetchFavorites(clientId),
    enabled: hasClientId,
  });

  const favorites = useMemo(() => favoritesQuery.data ?? [], [favoritesQuery.data]);

  const updateCache = useCallback(
    (updater: (items: ProjectFavoriteRecord[]) => ProjectFavoriteRecord[]) => {
      queryClient.setQueryData<ProjectFavoriteRecord[]>(buildQueryKey(clientId), (items) => updater(items ?? []));
    },
    [clientId, queryClient],
  );

  const addMutation = useMutation({
    mutationFn: ({ projectId, note }: { projectId: number; note?: string }) =>
      saveFavorite({ clientId, projectId, note }),
    onSuccess: (record) => {
      updateCache((items) => {
        const without = items.filter((item) => item.project.id !== record.project.id);
        return [record, ...without];
      });
    },
  });

  const removeMutation = useMutation({
    mutationFn: (projectId: number) => deleteFavorite(clientId, projectId),
    onSuccess: (_, projectId) => {
      updateCache((items) => items.filter((item) => item.project.id !== projectId));
    },
  });

  const updateNoteMutation = useMutation({
    mutationFn: ({ projectId, note }: { projectId: number; note?: string }) =>
      saveFavorite({ clientId, projectId, note }),
    onSuccess: (record) => {
      updateCache((items) => {
        const without = items.filter((item) => item.project.id !== record.project.id);
        return [record, ...without];
      });
    },
  });

  const clearMutation = useMutation({
    mutationFn: () => clearFavoritesApi(clientId),
    onSuccess: () => {
      updateCache(() => []);
    },
  });

  const isFavorite = useCallback(
    (projectId: number) => favorites.some((item) => item.project.id === projectId),
    [favorites],
  );

  const addFavorite = useCallback(
    (project: Project, note?: string) => addMutation.mutateAsync({ projectId: project.id, note }),
    [addMutation],
  );

  const removeFavorite = useCallback(
    (projectId: number) => removeMutation.mutateAsync(projectId),
    [removeMutation],
  );

  const updateFavoriteNote = useCallback(
    (projectId: number, note?: string) => updateNoteMutation.mutateAsync({ projectId, note }),
    [updateNoteMutation],
  );

  const clearFavorites = useCallback(
    () => clearMutation.mutateAsync(),
    [clearMutation],
  );

  return {
    clientId,
    favorites,
    isLoading: favoritesQuery.isLoading || favoritesQuery.isFetching,
    addFavorite,
    isFavorite,
    removeFavorite,
    updateFavoriteNote,
    clearFavorites,
  };
};
