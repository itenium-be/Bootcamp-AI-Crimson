import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchReportSummary, fetchCourseUsage } from '@/api/client';

export function CourseUsageReport() {
  const { t } = useTranslation();
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const { data: summary, isLoading: summaryLoading } = useQuery({
    queryKey: ['report-summary'],
    queryFn: fetchReportSummary,
  });

  const usageParams = {
    ...(dateFrom && { from: dateFrom }),
    ...(dateTo && { to: dateTo }),
  };

  const { data: courseUsage = [], isLoading: usageLoading } = useQuery({
    queryKey: ['course-usage', usageParams],
    queryFn: () => fetchCourseUsage(usageParams),
  });

  const mostPopular = [...courseUsage].sort((a, b) => b.totalEnrollments - a.totalEnrollments).slice(0, 5);
  const lowestCompletion = [...courseUsage]
    .filter((c) => c.totalEnrollments > 0)
    .sort((a, b) => a.completionRate - b.completionRate)
    .slice(0, 5);

  return (
    <div className="space-y-8">
      <h1 className="text-3xl font-bold">{t('reports.usage.title')}</h1>

      {/* Summary KPIs */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="rounded-lg border p-4">
          <p className="text-sm text-muted-foreground">{t('reports.usage.activeLearners')}</p>
          <p className="text-3xl font-bold">{summaryLoading ? '…' : (summary?.activeLearners ?? 0)}</p>
        </div>
        <div className="rounded-lg border p-4">
          <p className="text-sm text-muted-foreground">{t('reports.usage.completionsThisMonth')}</p>
          <p className="text-3xl font-bold">{summaryLoading ? '…' : (summary?.completionsThisMonth ?? 0)}</p>
        </div>
        <div className="rounded-lg border p-4">
          <p className="text-sm text-muted-foreground">{t('reports.usage.totalEnrollments')}</p>
          <p className="text-3xl font-bold">{summaryLoading ? '…' : (summary?.totalEnrollments ?? 0)}</p>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3">
        <div className="flex flex-col gap-1">
          <label className="text-xs text-muted-foreground">{t('reports.usage.from')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="rounded border px-2 py-1 text-sm"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs text-muted-foreground">{t('reports.usage.to')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="rounded border px-2 py-1 text-sm"
          />
        </div>
        {(dateFrom || dateTo) && (
          <button
            onClick={() => { setDateFrom(''); setDateTo(''); }}
            className="self-end text-sm text-muted-foreground hover:text-foreground"
          >
            {t('courses.clearFilters')}
          </button>
        )}
      </div>

      {/* Per-course table */}
      <div>
        <h2 className="text-xl font-semibold mb-3">{t('reports.usage.perCourse')}</h2>
        {usageLoading ? (
          <p className="text-muted-foreground">{t('common.loading')}</p>
        ) : courseUsage.length === 0 ? (
          <p className="text-muted-foreground">{t('common.noResults')}</p>
        ) : (
          <div className="rounded-md border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-2 text-left font-medium">{t('courses.name')}</th>
                  <th className="px-4 py-2 text-right font-medium">{t('reports.usage.totalEnrollments')}</th>
                  <th className="px-4 py-2 text-right font-medium">{t('reports.usage.completions')}</th>
                  <th className="px-4 py-2 text-right font-medium">{t('reports.usage.completionRate')}</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {courseUsage.map((c) => (
                  <tr key={c.courseId} className="hover:bg-muted/30">
                    <td className="px-4 py-2">{c.courseName}</td>
                    <td className="px-4 py-2 text-right">{c.totalEnrollments}</td>
                    <td className="px-4 py-2 text-right">{c.completions}</td>
                    <td className="px-4 py-2 text-right">
                      <span
                        className={
                          c.completionRate >= 75
                            ? 'text-green-600'
                            : c.completionRate >= 40
                              ? 'text-amber-600'
                              : 'text-red-600'
                        }
                      >
                        {c.completionRate.toFixed(1)}%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Most popular + Lowest completion */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div>
          <h2 className="text-xl font-semibold mb-3">{t('reports.usage.mostPopular')}</h2>
          <ul className="space-y-2">
            {mostPopular.map((c, i) => (
              <li key={c.courseId} className="flex justify-between rounded border px-3 py-2 text-sm">
                <span>
                  {i + 1}. {c.courseName}
                </span>
                <span className="text-muted-foreground">{c.totalEnrollments} enrolled</span>
              </li>
            ))}
            {mostPopular.length === 0 && <li className="text-muted-foreground text-sm">{t('common.noResults')}</li>}
          </ul>
        </div>

        <div>
          <h2 className="text-xl font-semibold mb-3">{t('reports.usage.lowestCompletion')}</h2>
          <ul className="space-y-2">
            {lowestCompletion.map((c) => (
              <li key={c.courseId} className="flex justify-between rounded border px-3 py-2 text-sm">
                <span>{c.courseName}</span>
                <span className="text-red-600">{c.completionRate.toFixed(1)}%</span>
              </li>
            ))}
            {lowestCompletion.length === 0 && <li className="text-muted-foreground text-sm">{t('common.noResults')}</li>}
          </ul>
        </div>
      </div>
    </div>
  );
}
