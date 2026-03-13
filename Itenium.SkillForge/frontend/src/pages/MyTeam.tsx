import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchTeamMembers } from '@/api/client';
import { useTeamStore } from '@/stores';

export function MyTeam() {
  const { t } = useTranslation();
  const { teams, selectedTeam, setSelectedTeam } = useTeamStore();
  const [activeTeam, setActiveTeam] = useState(selectedTeam ?? teams[0] ?? null);

  const { data: members = [], isLoading } = useQuery({
    queryKey: ['team-members', activeTeam?.id],
    queryFn: () => fetchTeamMembers(activeTeam?.id ?? 0),
    enabled: activeTeam != null,
  });

  const handleSelectTeam = (team: (typeof teams)[0]) => {
    setActiveTeam(team);
    setSelectedTeam(team);
  };

  return (
    <div className="space-y-6 max-w-3xl">
      <h1 className="text-3xl font-bold">{t('myTeam.title')}</h1>

      {teams.length > 1 && (
        <div className="flex gap-2 border-b">
          {teams.map((team) => (
            <button
              key={team.id}
              onClick={() => handleSelectTeam(team)}
              className={`px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors ${
                activeTeam?.id === team.id
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              {team.name}
            </button>
          ))}
        </div>
      )}

      {isLoading ? (
        <div>{t('common.loading')}</div>
      ) : (
        <div className="rounded-md border">
          <div className="p-3 border-b bg-muted/50 font-medium text-sm">
            {activeTeam?.name} ({members.length})
          </div>
          {members.length === 0 ? (
            <div className="p-6 text-center text-muted-foreground text-sm">{t('myTeam.noMembers')}</div>
          ) : (
            <table className="w-full">
              <thead>
                <tr className="border-b bg-muted/20">
                  <th className="p-3 text-left font-medium text-sm">{t('users.name')}</th>
                  <th className="p-3 text-left font-medium text-sm">{t('users.email')}</th>
                  <th className="p-3 text-left font-medium text-sm">{t('myTeam.lastActive')}</th>
                </tr>
              </thead>
              <tbody>
                {members.map((member) => (
                  <tr key={member.id} className="border-b last:border-0">
                    <td className="p-3 font-medium">{member.name || '-'}</td>
                    <td className="p-3 text-muted-foreground">{member.email}</td>
                    <td className="p-3 text-muted-foreground text-sm">
                      {member.lastActiveAt ? new Date(member.lastActiveAt).toLocaleDateString() : t('myTeam.never')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
