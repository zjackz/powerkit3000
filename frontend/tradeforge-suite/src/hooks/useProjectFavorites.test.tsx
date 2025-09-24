import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import type { Project, ProjectFavoriteRecord } from '@/types/project';
import { useProjectFavorites } from './useProjectFavorites';

vi.mock('@/services/projectsService', () => ({
  fetchFavorites: vi.fn(),
  saveFavorite: vi.fn(),
  deleteFavorite: vi.fn(),
  clearFavorites: vi.fn(),
}));

import {
  fetchFavorites,
  saveFavorite,
  deleteFavorite,
  clearFavorites,
} from '@/services/projectsService';

const mockedFetchFavorites = vi.mocked(fetchFavorites);
const mockedSaveFavorite = vi.mocked(saveFavorite);
const mockedDeleteFavorite = vi.mocked(deleteFavorite);
const mockedClearFavorites = vi.mocked(clearFavorites);

const CLIENT_ID = 'client-test';

const createQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

const buildProject = (overrides: Partial<Project> = {}): Project => ({
  id: 1,
  name: 'Sample Project',
  blurb: null,
  blurbCn: null,
  categoryId: null,
  categoryName: 'Design',
  country: 'US',
  state: 'successful',
  goal: 1000,
  pledged: 2500,
  percentFunded: 250,
  fundingVelocity: 120,
  backersCount: 150,
  currency: 'USD',
  launchedAt: new Date('2024-01-01T00:00:00Z').toISOString(),
  deadline: new Date('2024-02-01T00:00:00Z').toISOString(),
  creatorName: 'Creator',
  locationName: 'New York',
  ...overrides,
});

const buildFavorite = (overrides: Partial<ProjectFavoriteRecord> = {}): ProjectFavoriteRecord => ({
  id: 10,
  clientId: 'client-test',
  project: buildProject(),
  note: '高热度项目',
  savedAt: new Date('2024-03-01T00:00:00Z').toISOString(),
  ...overrides,
});

const createWrapper = (client: QueryClient) =>
  function Wrapper({ children }: React.PropsWithChildren) {
    return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
  };

describe('useProjectFavorites', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    window.localStorage.clear();
    window.localStorage.setItem('powerkit3000:client-id', CLIENT_ID);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('fetches favorites from API on mount', async () => {
    const queryClient = createQueryClient();
    const wrapper = createWrapper(queryClient);
    mockedFetchFavorites.mockResolvedValueOnce([buildFavorite()]);

    const { result } = renderHook(() => useProjectFavorites(), { wrapper });

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.favorites).toHaveLength(1);
    expect(mockedFetchFavorites).toHaveBeenCalledWith(CLIENT_ID);
  });

  it('adds favorite via API and updates cache', async () => {
    const queryClient = createQueryClient();
    const wrapper = createWrapper(queryClient);
    mockedFetchFavorites.mockResolvedValueOnce([]);

    const project = buildProject({ id: 123 });
    const favoriteRecord = buildFavorite({
      id: 50,
      project,
      note: '测试收藏',
    });
    mockedSaveFavorite.mockResolvedValue(favoriteRecord);

    const { result } = renderHook(() => useProjectFavorites(), { wrapper });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    await act(async () => {
      await result.current.addFavorite(project, '测试收藏');
    });

    expect(mockedSaveFavorite).toHaveBeenCalledWith({
      clientId: CLIENT_ID,
      projectId: 123,
      note: '测试收藏',
    });
    expect(result.current.favorites).toHaveLength(1);
    expect(result.current.favorites[0].project.id).toBe(123);
  });

  it('removes favorite via API and updates cache', async () => {
    const queryClient = createQueryClient();
    const wrapper = createWrapper(queryClient);
    const existingFavorite = buildFavorite({ project: buildProject({ id: 777 }) });
    mockedFetchFavorites.mockResolvedValueOnce([existingFavorite]);
    mockedDeleteFavorite.mockResolvedValue();

    const { result } = renderHook(() => useProjectFavorites(), { wrapper });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    await act(async () => {
      await result.current.removeFavorite(777);
    });

    expect(mockedDeleteFavorite).toHaveBeenCalledWith(CLIENT_ID, 777);
    expect(result.current.favorites).toHaveLength(0);
  });

  it('clears favorites via API', async () => {
    const queryClient = createQueryClient();
    const wrapper = createWrapper(queryClient);
    mockedFetchFavorites.mockResolvedValueOnce([buildFavorite()]);
    mockedClearFavorites.mockResolvedValue();

    const { result } = renderHook(() => useProjectFavorites(), { wrapper });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    await act(async () => {
      await result.current.clearFavorites();
    });

    expect(mockedClearFavorites).toHaveBeenCalledWith(CLIENT_ID);
    expect(result.current.favorites).toHaveLength(0);
  });
});
