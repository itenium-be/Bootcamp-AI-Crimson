import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchTeamMembers, fetchAvailableLearners, addTeamMember, removeTeamMember } from '@/api/client';
import type { User } from '@/api/client';

interface TeamDetailProps {
  teamId: number;
  teamName?: string;
}

export function TeamDetail({ teamId, teamName }: TeamDetailProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchLearner, setSearchLearner] = useState('');
  const [confirmRemove, setConfirmRemove] = useState<User | null>(null);

  const { data: members = [], isLoading: loadingMembers } = useQuery({
    queryKey: ['team-members', teamId],
    queryFn: () => fetchTeamMembers(teamId),
  });

  const { data: availableLearners = [] } = useQuery({
    queryKey: ['team-available-learners', teamId],
    queryFn: () => fetchAvailableLearners(teamId),
  });

  const memberIds = useMemo(() => new Set(members.map((m) => m.id)), [members]);

  const filteredLearners = useMemo(() => {
    const q = searchLearner.toLowerCase().trim();
    return availableLearners.filter(
      (l) => !memberIds.has(l.id) && (l.name.toLowerCase().includes(q) || l.email.toLowerCase().includes(q)),
    );
  }, [availableLearners, memberIds, searchLearner]);

  const addMutation = useMutation({
    mutationFn: (userId: string) => addTeamMember(teamId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['team-members', teamId] });
      setSearchLearner('');
    },
  });

  const removeMutation = useMutation({
    mutationFn: (userId: string) => removeTeamMember(teamId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['team-members', teamId] });
      setConfirmRemove(null);
    },
  });

  if (loadingMembers) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate({ to: '/admin/teams' })}
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← {t('teamDetail.backToTeams')}
        </button>
      </div>

      <div>
        <h1 className="text-3xl font-bold">{teamName ?? t('teamDetail.title')}</h1>
      </div>

      {/* Add member section */}
      <div className="rounded-md border p-4 space-y-3">
        <h2 className="font-semibold">{t('teamDetail.addMember')}</h2>
        <div className="flex gap-2">
          <div className="relative flex-1">
            <input
              type="text"
              placeholder={t('teamDetail.searchLearner')}
              value={searchLearner}
              onChange={(e) => setSearchLearner(e.target.value)}
              className="h-9 w-full rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
            {searchLearner && filteredLearners.length > 0 && (
              <div className="absolute z-10 mt-1 w-full rounded-md border bg-background shadow-md">
                {filteredLearners.slice(0, 8).map((learner) => (
                  <button
                    key={learner.id}
                    onClick={() => addMutation.mutate(learner.id)}
                    disabled={addMutation.isPending}
                    className="w-full text-left px-3 py-2 text-sm hover:bg-muted flex justify-between items-center"
                  >
                    <span className="font-medium">{learner.name || learner.email}</span>
                    <span className="text-muted-foreground text-xs">{learner.email}</span>
                  </button>
                ))}
              </div>
            )}
            {searchLearner && filteredLearners.length === 0 && (
              <div className="absolute z-10 mt-1 w-full rounded-md border bg-background shadow-md px-3 py-2 text-sm text-muted-foreground">
                {t('common.noResults')}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Members list */}
      <div className="rounded-md border">
        <div className="p-3 border-b bg-muted/50 font-medium text-sm">
          {t('teamDetail.members')} ({members.length})
        </div>
        {members.length === 0 ? (
          <div className="p-6 text-center text-muted-foreground text-sm">{t('teamDetail.noMembers')}</div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b bg-muted/20">
                <th className="p-3 text-left font-medium text-sm">{t('users.name')}</th>
                <th className="p-3 text-left font-medium text-sm">{t('users.email')}</th>
                <th className="p-3 text-left font-medium text-sm">{t('users.status')}</th>
                <th className="p-3"></th>
              </tr>
            </thead>
            <tbody>
              {members.map((member) => (
                <tr key={member.id} className="border-b last:border-0">
                  <td className="p-3 font-medium">{member.name || '-'}</td>
                  <td className="p-3 text-muted-foreground">{member.email}</td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                        member.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {member.isActive ? t('common.active') : t('users.inactive')}
                    </span>
                  </td>
                  <td className="p-3 text-right">
                    <button
                      onClick={() => setConfirmRemove(member)}
                      className="rounded-md border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50"
                    >
                      {t('teamDetail.remove')}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Remove confirmation modal */}
      {confirmRemove && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-background rounded-md border p-6 max-w-sm w-full space-y-4">
            <p className="text-sm">
              {t('teamDetail.confirmRemove', { name: confirmRemove.name || confirmRemove.email })}
            </p>
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => setConfirmRemove(null)}
                className="h-9 rounded-md border px-3 text-sm text-muted-foreground hover:bg-muted"
              >
                {t('common.cancel')}
              </button>
              <button
                onClick={() => removeMutation.mutate(confirmRemove.id)}
                disabled={removeMutation.isPending}
                className="h-9 rounded-md bg-red-600 text-white px-3 text-sm font-medium hover:bg-red-700 disabled:opacity-50"
              >
                {t('teamDetail.remove')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
