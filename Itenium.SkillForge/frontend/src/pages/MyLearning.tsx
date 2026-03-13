import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchMyEnrollments } from '@/api/client';

export interface Enrollment {
  id: number;
  courseId: number;
  courseName: string;
  courseCategory: string | null;
  courseLevel: string | null;
  enrolledAt: string;
  status: string;
}

export interface EnrollmentFilters {
  search: string;
  status: string;
}

export function filterEnrollments(enrollments: Enrollment[], filters: EnrollmentFilters): Enrollment[] {
  const search = filters.search.toLowerCase().trim();
  return enrollments.filter((e) => {
    if (search) {
      const inName = e.courseName.toLowerCase().includes(search);
      const inCategory = (e.courseCategory ?? '').toLowerCase().includes(search);
      if (!inName && !inCategory) return false;
    }
    if (filters.status && e.status !== filters.status) return false;
    return true;
  });
}

export function MyLearning() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [filters, setFilters] = useState<EnrollmentFilters>({ search: '', status: '' });

  const { data: enrollments = [], isLoading } = useQuery({
    queryKey: ['enrollments', 'me'],
    queryFn: fetchMyEnrollments,
  });

  const filtered = useMemo(() => filterEnrollments(enrollments, filters), [enrollments, filters]);
  const hasActiveFilters = filters.search !== '' || filters.status !== '';

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('myLearning.title')}</h1>
        <p className="text-muted-foreground mt-1">{t('myLearning.subtitle')}</p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <input
          type="text"
          placeholder={t('myLearning.searchPlaceholder')}
          value={filters.search}
          onChange={(e) => setFilters((f) => ({ ...f, search: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring w-72"
        />
        <select
          value={filters.status}
          onChange={(e) => setFilters((f) => ({ ...f, status: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('myLearning.allStatuses')}</option>
          <option value="Active">{t('myLearning.statusActive')}</option>
          <option value="Completed">{t('myLearning.statusCompleted')}</option>
        </select>
        {hasActiveFilters && (
          <button
            onClick={() => setFilters({ search: '', status: '' })}
            className="h-9 rounded-md border px-3 text-sm text-muted-foreground hover:bg-muted"
          >
            {t('courses.clearFilters')}
          </button>
        )}
        <span className="text-sm text-muted-foreground ml-auto">
          {filtered.length} / {enrollments.length}
        </span>
      </div>

      {filtered.length === 0 ? (
        <div className="rounded-md border p-8 text-center text-muted-foreground">
          {hasActiveFilters ? t('common.noResults') : t('myLearning.noEnrollments')}
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {filtered.map((enrollment) => (
            <div
              key={enrollment.id}
              className="rounded-md border p-4 space-y-2 cursor-pointer hover:bg-muted/50"
              onClick={() => navigate({ to: '/manager/courses/$id', params: { id: String(enrollment.courseId) } })}
            >
              <div className="flex items-start justify-between gap-2">
                <h3 className="font-semibold leading-tight">{enrollment.courseName}</h3>
                <span
                  className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
                    enrollment.status === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'
                  }`}
                >
                  {t(`myLearning.status${enrollment.status}`)}
                </span>
              </div>
              <div className="flex gap-2 text-xs text-muted-foreground">
                {enrollment.courseCategory && <span>{enrollment.courseCategory}</span>}
                {enrollment.courseLevel && <span>· {enrollment.courseLevel}</span>}
              </div>
              <p className="text-xs text-muted-foreground">
                {t('myLearning.enrolledOn')} {new Date(enrollment.enrolledAt).toLocaleDateString()}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
