import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface Team {
  id: number;
  name: string;
}

type Mode = 'backoffice' | 'manager';

interface TeamState {
  mode: Mode;
  selectedTeam: Team | null;
  teams: Team[];
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

      setMode: (mode: Mode) => set({ mode }),

      setSelectedTeam: (team: Team | null) => set({ selectedTeam: team }),

      setTeams: (teams: Team[], isBackOffice: boolean) => {
        const currentState = get();

        // If user is not backoffice, automatically switch to local mode
        if (!isBackOffice) {
          const selectedTeam =
            currentState.selectedTeam && teams.some((t) => t.id === currentState.selectedTeam?.id)
              ? currentState.selectedTeam
              : teams[0] || null;

          set({
            teams,
            mode: 'manager',
            selectedTeam,
          });
        } else {
          set({ teams });
        }
      },

      reset: () => {
        set({
          mode: 'backoffice',
          selectedTeam: null,
          teams: [],
        });
      },
    }),
    {
      name: 'team-storage',
      partialize: (state) => ({
        mode: state.mode,
        selectedTeam: state.selectedTeam,
      }),
    },
  ),
);
