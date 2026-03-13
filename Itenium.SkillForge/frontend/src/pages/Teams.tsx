import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchUserTeams } from '@/api/client';

export function Teams() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { data: teams = [], isLoading } = useQuery({
    queryKey: ['teams'],
    queryFn: fetchUserTeams,
  });

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('teams.title')}</h1>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('teams.name')}</th>
              <th className="p-3 text-left font-medium">{t('teams.members')}</th>
            </tr>
          </thead>
          <tbody>
            {teams.map((team) => (
              <tr
                key={team.id}
                className="border-b cursor-pointer hover:bg-muted/50"
                onClick={() => navigate({ to: '/admin/teams/$id', params: { id: String(team.id) } })}
              >
                <td className="p-3 font-medium">{team.name}</td>
                <td className="p-3 text-muted-foreground text-sm">{t('teams.viewMembers')}</td>
              </tr>
            ))}
            {teams.length === 0 && (
              <tr>
                <td colSpan={2} className="p-3 text-center text-muted-foreground">
                  {t('teams.noTeams')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
