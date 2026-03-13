import { useState } from 'react';
import { useParams } from '@tanstack/react-router';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { fetchQuizAnalytics, fetchQuizLearnerAnalytics } from '@/api/client';
import { useTeamStore } from '@/stores';

export function QuizAnalytics() {
  const { t } = useTranslation();
  const { id } = useParams({ from: '/_authenticated/manager/quizzes/$id/analytics' });
  const quizId = Number(id);
  const { selectedTeam } = useTeamStore();

  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const { data: analytics, isLoading: analyticsLoading } = useQuery({
    queryKey: ['quizAnalytics', quizId, dateFrom, dateTo],
    queryFn: () =>
      fetchQuizAnalytics(quizId, {
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
      }),
  });

  const { data: learners, isLoading: learnersLoading } = useQuery({
    queryKey: ['quizLearnerAnalytics', quizId, selectedTeam?.id],
    queryFn: () => fetchQuizLearnerAnalytics(quizId, selectedTeam?.id),
  });

  const maxDistributionCount = Math.max(1, ...(analytics?.scoreDistribution.map((b) => b.count) ?? []));

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('quizAnalytics.title')}</h1>
        <p className="text-muted-foreground">{t('quizAnalytics.subtitle')}</p>
      </div>

      {/* Date range filter */}
      <div className="flex gap-4 items-end">
        <div>
          <label className="block text-sm font-medium mb-1">{t('quizAnalytics.dateFrom')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="border rounded px-2 py-1 text-sm"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">{t('quizAnalytics.dateTo')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="border rounded px-2 py-1 text-sm"
          />
        </div>
      </div>

      {analyticsLoading ? (
        <div>{t('common.loading')}</div>
      ) : (
        <>
          {/* Summary stats */}
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <StatCard label={t('quizAnalytics.averageScore')} value={`${analytics?.averageScore.toFixed(1) ?? 0}%`} />
            <StatCard label={t('quizAnalytics.passRate')} value={`${analytics?.passRate.toFixed(1) ?? 0}%`} />
            <StatCard label={t('quizAnalytics.totalAttempts')} value={String(analytics?.totalAttempts ?? 0)} />
            <StatCard label={t('quizAnalytics.uniqueLearners')} value={String(analytics?.uniqueLearners ?? 0)} />
          </div>

          {/* Score distribution */}
          <div className="rounded-md border p-4 space-y-2">
            <h2 className="text-lg font-semibold">{t('quizAnalytics.scoreDistribution')}</h2>
            <div className="space-y-1">
              {analytics?.scoreDistribution.map((bucket) => (
                <div key={bucket.range} className="flex items-center gap-2 text-sm">
                  <span className="w-16 text-right text-muted-foreground">{bucket.range}</span>
                  <div className="flex-1 bg-muted rounded overflow-hidden h-5">
                    <div
                      className="bg-primary h-full rounded"
                      style={{ width: `${(bucket.count / maxDistributionCount) * 100}%` }}
                    />
                  </div>
                  <span className="w-8 text-right">{bucket.count}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Question stats - most missed */}
          <div className="rounded-md border p-4 space-y-2">
            <h2 className="text-lg font-semibold">{t('quizAnalytics.questionStats')}</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="p-2 text-left font-medium">{t('quizAnalytics.question')}</th>
                    <th className="p-2 text-right font-medium">{t('quizAnalytics.correctRate')}</th>
                  </tr>
                </thead>
                <tbody>
                  {analytics?.questionStats.map((qs) => (
                    <tr key={qs.questionId} className="border-b">
                      <td className="p-2">{qs.questionText}</td>
                      <td className="p-2 text-right">
                        <span className={qs.correctRate < 50 ? 'text-destructive font-medium' : 'text-green-600'}>
                          {qs.correctRate.toFixed(1)}%
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}

      {/* Per-learner breakdown */}
      <div className="rounded-md border p-4 space-y-2">
        <h2 className="text-lg font-semibold">
          {t('quizAnalytics.learnerBreakdown')}
          {selectedTeam && (
            <span className="ml-2 text-sm font-normal text-muted-foreground">({selectedTeam.name})</span>
          )}
        </h2>
        {learnersLoading ? (
          <div>{t('common.loading')}</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="p-2 text-left font-medium">{t('quizAnalytics.learner')}</th>
                  <th className="p-2 text-right font-medium">{t('quizAnalytics.latestScore')}</th>
                  <th className="p-2 text-center font-medium">{t('quizAnalytics.result')}</th>
                  <th className="p-2 text-right font-medium">{t('quizAnalytics.completedAt')}</th>
                </tr>
              </thead>
              <tbody>
                {learners?.map((learner) => (
                  <tr key={learner.userId} className="border-b">
                    <td className="p-2">{learner.userName}</td>
                    <td className="p-2 text-right">{learner.latestScore.toFixed(1)}%</td>
                    <td className="p-2 text-center">
                      <span
                        className={`px-2 py-0.5 rounded text-xs font-medium ${learner.isPassed ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}
                      >
                        {learner.isPassed ? t('quizAnalytics.passed') : t('quizAnalytics.failed')}
                      </span>
                    </td>
                    <td className="p-2 text-right text-muted-foreground">
                      {new Date(learner.completedAt).toLocaleDateString()}
                    </td>
                  </tr>
                ))}
                {learners?.length === 0 && (
                  <tr>
                    <td colSpan={4} className="p-3 text-center text-muted-foreground">
                      {t('quizAnalytics.noLearners')}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border p-4">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
    </div>
  );
}
