import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchTeamMembers } from '@/api/client';
import { useTeamStore } from '@/stores';

export function TeamMembers() {
  const { t } = useTranslation();
  const { selectedTeam, teams, setSelectedTeam, setMode } = useTeamStore();

  const activeTeamId = selectedTeam?.id ?? teams[0]?.id ?? null;

  const { data: members, isLoading } = useQuery({
    queryKey: ['team-members', activeTeamId],
    queryFn: () => fetchTeamMembers(activeTeamId ?? 0),
    enabled: activeTeamId !== null,
  });

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('teamMembers.title')}</h1>

        {teams.length > 1 && (
          <div className="flex gap-2">
            {teams.map((team) => (
              <button
                key={team.id}
                onClick={() => {
                  setMode('manager');
                  setSelectedTeam(team);
                }}
                className={`rounded px-3 py-1.5 text-sm font-medium border ${
                  activeTeamId === team.id
                    ? 'bg-primary text-primary-foreground border-primary'
                    : 'hover:bg-muted border-input'
                }`}
              >
                {team.name}
              </button>
            ))}
          </div>
        )}
      </div>

      {activeTeamId === null ? (
        <p className="text-muted-foreground">{t('teamMembers.noTeam')}</p>
      ) : (
        <div className="rounded-md border">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="p-3 text-left font-medium">{t('teamMembers.name')}</th>
                <th className="p-3 text-left font-medium">{t('teamMembers.email')}</th>
                <th className="p-3 text-left font-medium">{t('teamMembers.lastActive')}</th>
              </tr>
            </thead>
            <tbody>
              {members?.map((member) => (
                <tr key={member.email} className="border-b">
                  <td className="p-3 font-medium">{member.name}</td>
                  <td className="p-3 text-muted-foreground">{member.email}</td>
                  <td className="p-3 text-muted-foreground">{member.lastActive ?? '-'}</td>
                </tr>
              ))}
              {members?.length === 0 && (
                <tr>
                  <td colSpan={3} className="p-3 text-center text-muted-foreground">
                    {t('teamMembers.noMembers')}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
