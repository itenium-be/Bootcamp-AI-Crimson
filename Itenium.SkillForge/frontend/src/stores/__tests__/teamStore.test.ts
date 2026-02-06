import { useTeamStore, type Team } from '../teamStore';

const teamA: Team = { id: 1, name: 'Team Alpha' };
const teamB: Team = { id: 2, name: 'Team Beta' };
const teamC: Team = { id: 3, name: 'Team Charlie' };

function resetStore() {
  useTeamStore.setState({
    mode: 'backoffice',
    selectedTeam: null,
    teams: [],
  });
  localStorage.clear();
}

beforeEach(() => {
  resetStore();
});

describe('useTeamStore', () => {
  describe('setMode', () => {
    it('switches mode', () => {
      useTeamStore.getState().setMode('manager');
      expect(useTeamStore.getState().mode).toBe('manager');

      useTeamStore.getState().setMode('backoffice');
      expect(useTeamStore.getState().mode).toBe('backoffice');
    });
  });

  describe('setSelectedTeam', () => {
    it('sets the selected team', () => {
      useTeamStore.getState().setSelectedTeam(teamA);
      expect(useTeamStore.getState().selectedTeam).toEqual(teamA);
    });

    it('clears the selected team with null', () => {
      useTeamStore.getState().setSelectedTeam(teamA);
      useTeamStore.getState().setSelectedTeam(null);
      expect(useTeamStore.getState().selectedTeam).toBeNull();
    });
  });

  describe('setTeams (backoffice user)', () => {
    it('sets teams without changing mode or selectedTeam', () => {
      useTeamStore.getState().setTeams([teamA, teamB], true);

      const state = useTeamStore.getState();
      expect(state.teams).toEqual([teamA, teamB]);
      expect(state.mode).toBe('backoffice');
      expect(state.selectedTeam).toBeNull();
    });
  });

  describe('setTeams (non-backoffice user)', () => {
    it('switches to manager mode and selects the first team', () => {
      useTeamStore.getState().setTeams([teamA, teamB], false);

      const state = useTeamStore.getState();
      expect(state.mode).toBe('manager');
      expect(state.selectedTeam).toEqual(teamA);
      expect(state.teams).toEqual([teamA, teamB]);
    });

    it('keeps the previously selected team if it still exists in the list', () => {
      useTeamStore.setState({ selectedTeam: teamB });

      useTeamStore.getState().setTeams([teamA, teamB, teamC], false);

      expect(useTeamStore.getState().selectedTeam).toEqual(teamB);
    });

    it('falls back to first team when previously selected team is no longer in the list', () => {
      useTeamStore.setState({ selectedTeam: teamC });

      useTeamStore.getState().setTeams([teamA, teamB], false);

      expect(useTeamStore.getState().selectedTeam).toEqual(teamA);
    });

    it('sets selectedTeam to null when teams list is empty', () => {
      useTeamStore.getState().setTeams([], false);

      const state = useTeamStore.getState();
      expect(state.mode).toBe('manager');
      expect(state.selectedTeam).toBeNull();
    });
  });

  describe('reset', () => {
    it('resets to initial state', () => {
      useTeamStore.setState({
        mode: 'manager',
        selectedTeam: teamA,
        teams: [teamA, teamB],
      });

      useTeamStore.getState().reset();

      const state = useTeamStore.getState();
      expect(state.mode).toBe('backoffice');
      expect(state.selectedTeam).toBeNull();
      expect(state.teams).toEqual([]);
    });
  });
});
