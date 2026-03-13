import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useTeamStore } from '@/stores';
import { fetchContentSuggestions, approveContentSuggestion, rejectContentSuggestion } from '@/api/client';
import type { ContentSuggestion } from '@/api/client';

export function ManagerSuggestions() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const selectedTeam = useTeamStore((s) => s.selectedTeam);
  const teamId = selectedTeam?.id ?? 0;
  const [statusFilter, setStatusFilter] = useState<string>('pending');
  const [reviewNote, setReviewNote] = useState<Record<number, string>>({});

  const { data: suggestions, isLoading } = useQuery({
    queryKey: ['contentSuggestions', teamId, statusFilter],
    queryFn: () => fetchContentSuggestions(teamId, statusFilter || undefined),
    enabled: teamId > 0,
  });

  const approveMutation = useMutation({
    mutationFn: ({ id, note }: { id: number; note?: string }) => approveContentSuggestion(id, note),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['contentSuggestions'] }),
  });

  const rejectMutation = useMutation({
    mutationFn: ({ id, note }: { id: number; note?: string }) => rejectContentSuggestion(id, note),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['contentSuggestions'] }),
  });

  if (!selectedTeam) {
    return <div className="text-muted-foreground">{t('suggestions.noTeam')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('suggestions.title')}</h1>
          <p className="text-muted-foreground mt-1">{t('suggestions.subtitle', { team: selectedTeam.name })}</p>
        </div>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="border rounded px-3 py-1.5 text-sm"
        >
          <option value="">{t('suggestions.allStatuses')}</option>
          <option value="pending">{t('suggestions.statusPending')}</option>
          <option value="approved">{t('suggestions.statusApproved')}</option>
          <option value="rejected">{t('suggestions.statusRejected')}</option>
        </select>
      </div>

      {isLoading ? (
        <div>{t('common.loading')}</div>
      ) : !suggestions || suggestions.length === 0 ? (
        <div className="text-muted-foreground">{t('suggestions.noSuggestions')}</div>
      ) : (
        <div className="space-y-4">
          {suggestions.map((s: ContentSuggestion) => (
            <div key={s.id} className="rounded-md border p-4 space-y-3">
              <div className="flex items-start justify-between gap-4">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <h2 className="font-semibold">{s.title}</h2>
                    <span
                      className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                        s.status === 'pending'
                          ? 'bg-yellow-100 text-yellow-800'
                          : s.status === 'approved'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {t(`suggestions.status${s.status.charAt(0).toUpperCase() + s.status.slice(1)}`)}
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground">
                    {t('suggestions.submittedBy', { name: s.submitterName ?? s.submittedBy })}
                    {' · '}
                    {new Date(s.submittedAt).toLocaleDateString()}
                  </p>
                  {s.description && <p className="text-sm">{s.description}</p>}
                  {s.url && (
                    <a
                      href={s.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-sm text-blue-600 hover:underline break-all"
                    >
                      {s.url}
                    </a>
                  )}
                  {s.topic && (
                    <p className="text-xs text-muted-foreground">
                      {t('suggestions.topic')}: {s.topic}
                    </p>
                  )}
                </div>
              </div>

              {s.status === 'pending' && (
                <div className="flex items-center gap-3 pt-2 border-t">
                  <input
                    type="text"
                    placeholder={t('suggestions.notePlaceholder')}
                    value={reviewNote[s.id] ?? ''}
                    onChange={(e) => setReviewNote((prev) => ({ ...prev, [s.id]: e.target.value }))}
                    className="flex-1 border rounded px-3 py-1.5 text-sm"
                  />
                  <button
                    onClick={() => approveMutation.mutate({ id: s.id, note: reviewNote[s.id] })}
                    disabled={approveMutation.isPending}
                    className="px-3 py-1.5 text-sm font-medium rounded bg-green-600 text-white hover:bg-green-700 disabled:opacity-50"
                  >
                    {t('suggestions.approve')}
                  </button>
                  <button
                    onClick={() => rejectMutation.mutate({ id: s.id, note: reviewNote[s.id] })}
                    disabled={rejectMutation.isPending}
                    className="px-3 py-1.5 text-sm font-medium rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
                  >
                    {t('suggestions.reject')}
                  </button>
                </div>
              )}

              {s.status !== 'pending' && s.reviewNote && (
                <div className="pt-2 border-t text-sm text-muted-foreground">
                  {t('suggestions.reviewNote')}: {s.reviewNote}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
