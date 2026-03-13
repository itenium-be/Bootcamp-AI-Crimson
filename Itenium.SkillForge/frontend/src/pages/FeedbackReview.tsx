import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { getFeedbackSummary, getCourseFeedback, flagFeedback, dismissFeedback } from '@/api/client';
import type { CourseFeedbackRanking, FeedbackEntry } from '@/api/client';

export function FeedbackReview() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [selectedCourse, setSelectedCourse] = useState<CourseFeedbackRanking | null>(null);
  const [minRating, setMinRating] = useState<number | undefined>(undefined);

  const { data: ranking, isLoading: rankingLoading } = useQuery({
    queryKey: ['feedbackSummary'],
    queryFn: getFeedbackSummary,
  });

  const { data: detail, isLoading: detailLoading } = useQuery({
    queryKey: ['courseFeedback', selectedCourse?.courseId, minRating],
    queryFn: () => getCourseFeedback(selectedCourse!.courseId, minRating),
    enabled: selectedCourse != null,
  });

  const flagMutation = useMutation({
    mutationFn: (id: number) => flagFeedback(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courseFeedback'] }),
  });

  const dismissMutation = useMutation({
    mutationFn: (id: number) => dismissFeedback(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courseFeedback'] }),
  });

  if (selectedCourse) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <button
            onClick={() => setSelectedCourse(null)}
            className="text-sm text-muted-foreground hover:underline"
          >
            ← {t('feedback.backToSummary')}
          </button>
          <h1 className="text-3xl font-bold">{t('feedback.entries', { name: selectedCourse.courseName })}</h1>
        </div>

        <div className="flex items-center gap-3">
          <label className="text-sm font-medium">{t('feedback.filterByRating')}:</label>
          <select
            value={minRating ?? ''}
            onChange={(e) => setMinRating(e.target.value ? Number(e.target.value) : undefined)}
            className="border rounded px-2 py-1 text-sm"
          >
            <option value="">{t('feedback.allRatings')}</option>
            {[1, 2, 3, 4, 5].map((r) => (
              <option key={r} value={r}>{r}+</option>
            ))}
          </select>
        </div>

        {detailLoading ? (
          <div>{t('common.loading')}</div>
        ) : (
          <>
            {detail && detail.entries.length > 0 && (
              <div className="rounded-md border p-4">
                <p className="text-sm text-muted-foreground">
                  {t('feedback.averageRating')}: <strong>{detail.averageRating.toFixed(1)}</strong>
                </p>
              </div>
            )}

            <div className="rounded-md border overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="p-2 text-left font-medium">{t('feedback.rating')}</th>
                    <th className="p-2 text-left font-medium">{t('feedback.comment')}</th>
                    <th className="p-2 text-left font-medium">{t('feedback.submittedAt')}</th>
                    <th className="p-2 text-center font-medium">{t('feedback.flagged')}</th>
                    <th className="p-2 text-center font-medium"></th>
                  </tr>
                </thead>
                <tbody>
                  {detail?.entries.map((entry: FeedbackEntry) => (
                    <tr key={entry.id} className="border-b">
                      <td className="p-2">
                        <RatingStars rating={entry.rating} />
                      </td>
                      <td className="p-2 max-w-xs truncate">{entry.comment ?? '—'}</td>
                      <td className="p-2 text-muted-foreground whitespace-nowrap">
                        {new Date(entry.submittedAt).toLocaleDateString()}
                      </td>
                      <td className="p-2 text-center">
                        {entry.isFlagged && (
                          <span className="px-2 py-0.5 rounded text-xs font-medium bg-yellow-100 text-yellow-800">
                            {t('feedback.flagged')}
                          </span>
                        )}
                      </td>
                      <td className="p-2 text-center">
                        <div className="flex gap-2 justify-center">
                          {!entry.isFlagged && (
                            <button
                              onClick={() => flagMutation.mutate(entry.id)}
                              className="text-xs border rounded px-2 py-1 hover:bg-muted"
                            >
                              {t('feedback.flag')}
                            </button>
                          )}
                          {entry.isFlagged && (
                            <button
                              onClick={() => dismissMutation.mutate(entry.id)}
                              className="text-xs border rounded px-2 py-1 hover:bg-muted"
                            >
                              {t('feedback.dismiss')}
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                  {detail?.entries.length === 0 && (
                    <tr>
                      <td colSpan={5} className="p-3 text-center text-muted-foreground">
                        {t('feedback.noFeedback')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('feedback.title')}</h1>
        <p className="text-muted-foreground">{t('feedback.subtitle')}</p>
      </div>

      {rankingLoading ? (
        <div>{t('common.loading')}</div>
      ) : (
        <div className="rounded-md border overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="p-2 text-left font-medium">{t('feedback.course')}</th>
                <th className="p-2 text-right font-medium">{t('feedback.averageRating')}</th>
                <th className="p-2 text-right font-medium">{t('feedback.count')}</th>
                <th className="p-2"></th>
              </tr>
            </thead>
            <tbody>
              {ranking?.map((item) => (
                <tr key={item.courseId} className="border-b hover:bg-muted/30 cursor-pointer" onClick={() => setSelectedCourse(item)}>
                  <td className="p-2 font-medium">{item.courseName}</td>
                  <td className="p-2 text-right">
                    <RatingStars rating={Math.round(item.averageRating)} />
                    <span className="ml-1 text-muted-foreground">({item.averageRating.toFixed(1)})</span>
                  </td>
                  <td className="p-2 text-right text-muted-foreground">{item.count}</td>
                  <td className="p-2 text-right">
                    <span className="text-primary text-xs hover:underline">View →</span>
                  </td>
                </tr>
              ))}
              {ranking?.length === 0 && (
                <tr>
                  <td colSpan={4} className="p-3 text-center text-muted-foreground">
                    {t('feedback.noFeedback')}
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

function RatingStars({ rating }: { rating: number }) {
  return (
    <span className="text-yellow-500">
      {'★'.repeat(Math.min(5, Math.max(0, rating)))}{'☆'.repeat(Math.max(0, 5 - rating))}
    </span>
  );
}
