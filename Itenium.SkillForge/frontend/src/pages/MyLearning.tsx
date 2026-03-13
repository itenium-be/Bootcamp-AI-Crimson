import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchMyEnrollments, fetchResumeLesson } from '@/api/client';

export interface Enrollment {
  id: number;
  courseId: number;
  courseName: string;
  courseCategory: string | null;
  courseLevel: string | null;
  enrolledAt: string;
  status: string;
  completedAt: string | null;
  moduleName: string | null;
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

function ResumeButton({ courseId }: { courseId: number }) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { data: resume } = useQuery({
    queryKey: ['resume', courseId],
    queryFn: () => fetchResumeLesson(courseId),
  });

  function handleResume(e: React.MouseEvent) {
    e.stopPropagation();
    if (resume?.isComplete) {
      if (resume.lessonId) {
        navigate({ to: '/lessons/$lessonId', params: { lessonId: String(resume.lessonId) } });
      }
    } else if (resume?.lessonId) {
      navigate({ to: '/lessons/$lessonId', params: { lessonId: String(resume.lessonId) } });
    }
  }

  if (!resume) return null;

  if (resume.isComplete) {
    return (
      <div className="flex items-center gap-2 mt-1">
        <span className="text-xs text-green-700 font-medium">{t('myLearning.courseComplete')}</span>
        {resume.lessonId && (
          <button onClick={handleResume} className="text-xs text-primary underline hover:no-underline">
            {t('myLearning.revisit')}
          </button>
        )}
      </div>
    );
  }

  return (
    <button
      onClick={handleResume}
      className="mt-1 text-xs rounded bg-primary px-2 py-1 text-primary-foreground hover:opacity-90"
    >
      {t('myLearning.resume')}
    </button>
  );
}

type Tab = 'Active' | 'Completed';

export function MyLearning() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>('Active');
  const [search, setSearch] = useState('');

  const { data: enrollments = [], isLoading } = useQuery({
    queryKey: ['enrollments', 'me'],
    queryFn: fetchMyEnrollments,
  });

  const filtered = useMemo(() => {
    const statusFiltered = enrollments.filter((e) => e.status === tab);
    if (!search.trim()) return statusFiltered;
    const s = search.toLowerCase().trim();
    return statusFiltered.filter(
      (e) => e.courseName.toLowerCase().includes(s) || (e.courseCategory ?? '').toLowerCase().includes(s),
    );
  }, [enrollments, tab, search]);

  const inProgressCount = enrollments.filter((e) => e.status === 'Active').length;
  const completedCount = enrollments.filter((e) => e.status === 'Completed').length;

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('myLearning.title')}</h1>
        <p className="text-muted-foreground mt-1">{t('myLearning.subtitle')}</p>
      </div>

      {/* Tab navigation */}
      <div className="flex gap-1 border-b">
        <button
          onClick={() => setTab('Active')}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            tab === 'Active'
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground'
          }`}
        >
          {t('myLearning.tabInProgress')}
          <span className="ml-2 rounded-full bg-muted px-1.5 py-0.5 text-xs">{inProgressCount}</span>
        </button>
        <button
          onClick={() => setTab('Completed')}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            tab === 'Completed'
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground'
          }`}
        >
          {t('myLearning.tabCompleted')}
          <span className="ml-2 rounded-full bg-muted px-1.5 py-0.5 text-xs">{completedCount}</span>
        </button>
      </div>

      {/* Search */}
      <div className="flex flex-wrap items-center gap-3">
        <input
          type="text"
          placeholder={t('myLearning.searchPlaceholder')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring w-72"
        />
        {search && (
          <button
            onClick={() => setSearch('')}
            className="h-9 rounded-md border px-3 text-sm text-muted-foreground hover:bg-muted"
          >
            {t('courses.clearFilters')}
          </button>
        )}
        <span className="text-sm text-muted-foreground ml-auto">
          {filtered.length} / {tab === 'Active' ? inProgressCount : completedCount}
        </span>
      </div>

      {filtered.length === 0 ? (
        <div className="rounded-md border p-8 text-center text-muted-foreground">
          {search
            ? t('common.noResults')
            : tab === 'Completed'
              ? t('myLearning.noCompleted')
              : t('myLearning.noEnrollments')}
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {filtered.map((enrollment) => (
            <div
              key={enrollment.id}
              className="rounded-md border p-4 space-y-2 cursor-pointer hover:bg-muted/50"
              onClick={() => navigate({ to: '/courses/$id', params: { id: String(enrollment.courseId) } })}
            >
              <div className="flex items-start justify-between gap-2">
                <h3 className="font-semibold leading-tight">{enrollment.courseName}</h3>
                {tab === 'Completed' ? (
                  <span className="shrink-0 rounded-full px-2 py-0.5 text-xs font-medium bg-green-100 text-green-800 flex items-center gap-1">
                    🎓 {t('myLearning.certificate')}
                  </span>
                ) : (
                  <span className="shrink-0 rounded-full px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-800">
                    {t('myLearning.statusActive')}
                  </span>
                )}
              </div>
              <div className="flex gap-2 text-xs text-muted-foreground">
                {enrollment.courseCategory && <span>{enrollment.courseCategory}</span>}
                {enrollment.courseLevel && <span>· {enrollment.courseLevel}</span>}
              </div>
              {enrollment.moduleName && (
                <p className="text-xs text-muted-foreground">
                  {t('myLearning.module')}: {enrollment.moduleName}
                </p>
              )}
              <p className="text-xs text-muted-foreground">
                {t('myLearning.enrolledOn')} {new Date(enrollment.enrolledAt).toLocaleDateString()}
              </p>
              {tab === 'Completed' && enrollment.completedAt && (
                <p className="text-xs text-green-700 font-medium">
                  {t('myLearning.completedOn')} {new Date(enrollment.completedAt).toLocaleDateString()}
                </p>
              )}
              <ResumeButton courseId={enrollment.courseId} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
