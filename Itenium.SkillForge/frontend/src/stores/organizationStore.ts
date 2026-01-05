import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface Organization {
  id: number;
  code: string;
  name: string;
}

type Mode = 'central' | 'local';

interface OrganizationState {
  mode: Mode;
  selectedOrganization: Organization | null;
  organizations: Organization[];
  isCentral: boolean;
  setMode: (mode: Mode) => void;
  setSelectedOrganization: (organization: Organization | null) => void;
  setOrganizations: (organizations: Organization[], isCentral: boolean) => void;
  reset: () => void;
}

export const useOrganizationStore = create<OrganizationState>()(
  persist(
    (set, get) => ({
      mode: 'central',
      selectedOrganization: null,
      organizations: [],
      isCentral: false,

      setMode: (mode: Mode) => {
        const { isCentral } = get();
        // Non-central users cannot switch to central mode
        if (mode === 'central' && !isCentral) {
          return;
        }
        set({ mode });
      },

      setSelectedOrganization: (organization: Organization | null) => {
        set({ selectedOrganization: organization });
      },

      setOrganizations: (organizations: Organization[], isCentral: boolean) => {
        const currentState = get();

        // If user is not central, automatically switch to local mode
        if (!isCentral) {
          const selectedOrganization = currentState.selectedOrganization
            && organizations.some(o => o.id === currentState.selectedOrganization?.id)
            ? currentState.selectedOrganization
            : organizations[0] || null;

          set({
            organizations,
            isCentral,
            mode: 'local',
            selectedOrganization,
          });
        } else {
          set({
            organizations,
            isCentral,
          });
        }
      },

      reset: () => {
        set({
          mode: 'central',
          selectedOrganization: null,
          organizations: [],
          isCentral: false,
        });
      },
    }),
    {
      name: 'organization-storage',
      partialize: (state) => ({
        mode: state.mode,
        selectedOrganization: state.selectedOrganization,
      }),
    }
  )
);
