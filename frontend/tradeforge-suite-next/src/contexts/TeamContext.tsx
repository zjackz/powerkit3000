'use client';

import { createContext, useContext, useMemo, useState } from 'react';
import { DEFAULT_TEAM_KEY, TEAM_PROFILES, type TeamProfile } from '@/constants/teams';

interface TeamContextValue {
  team: TeamProfile;
  teams: TeamProfile[];
  setTeamKey: (key: string) => void;
}

const TeamContext = createContext<TeamContextValue | undefined>(undefined);

export const TeamProvider = ({ children }: { children: React.ReactNode }) => {
  const [teamKey, setTeamKey] = useState<string>(DEFAULT_TEAM_KEY);

  const value = useMemo(() => {
    const current = TEAM_PROFILES.find((profile) => profile.key === teamKey) ?? TEAM_PROFILES[0];
    return {
      team: current,
      teams: TEAM_PROFILES,
      setTeamKey,
    } satisfies TeamContextValue;
  }, [teamKey]);

  return <TeamContext.Provider value={value}>{children}</TeamContext.Provider>;
};

export const useTeamContext = () => {
  const context = useContext(TeamContext);
  if (!context) {
    throw new Error('useTeamContext must be used within a TeamProvider');
  }
  return context;
};
