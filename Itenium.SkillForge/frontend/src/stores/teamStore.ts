import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface Team {
  id: number;
  code: string;
  name: string;
}

type Mode = 'backoffice' | 'local';

interface TeamState {
  mode: Mode;
  selectedTeam: Team | null;
  teams: Team[];
  isBackOffice: boolean;
  setMode: (mode: Mode) => void;
  setSelectedTeam: (team: Team | null) => void;
  setTeams: (teams: Team[], isBackOffice: boolean) => void;
  reset: () => void;
}

export const useTeamStore = create<TeamState>()(
  persist(
    (set, get) => ({
      mode: 'backoffice',
      selectedTeam: null,
      teams: [],
      isBackOffice: false,

      setMode: (mode: Mode) => {
        const { isBackOffice } = get();
        // Non-backoffice users cannot switch to backoffice mode
        if (mode === 'backoffice' && !isBackOffice) {
          return;
        }
        set({ mode });
      },

      setSelectedTeam: (team: Team | null) => {
        set({ selectedTeam: team });
      },

      setTeams: (teams: Team[], isBackOffice: boolean) => {
        const currentState = get();

        // If user is not backoffice, automatically switch to local mode
        if (!isBackOffice) {
          const selectedTeam = currentState.selectedTeam
            && teams.some(t => t.id === currentState.selectedTeam?.id)
            ? currentState.selectedTeam
            : teams[0] || null;

          set({
            teams,
            isBackOffice,
            mode: 'local',
            selectedTeam,
          });
        } else {
          set({
            teams,
            isBackOffice,
          });
        }
      },

      reset: () => {
        set({
          mode: 'backoffice',
          selectedTeam: null,
          teams: [],
          isBackOffice: false,
        });
      },
    }),
    {
      name: 'team-storage',
      partialize: (state) => ({
        mode: state.mode,
        selectedTeam: state.selectedTeam,
      }),
    }
  )
);
