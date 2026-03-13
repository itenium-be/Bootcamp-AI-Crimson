import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from '@tanstack/react-router';
import { fetchModuleProgress } from '@/api/client';

function ProgressBar({ percent }: { percent: number }) {
  return (
    <div className="w-full bg-muted rounded-full h-2">
      <div className="bg-primary h-2 rounded-full transition-all" style={{ width: `${percent}%` }} />
    </div>
  );
}

export function LearnerModuleDetail() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams({ strict: false }) as { id: string };

  const { data: module, isLoading } = useQuery({
    queryKey: ['learner', 'module', id],
    queryFn: () => fetchModuleProgress(Number(id)),
    enabled: !!id,
  });

  if (isLoading || !module) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <button
          className="text-sm text-muted-foreground hover:text-foreground"
          onClick={() => navigate({ to: '/modules' })}
        >
          ← {t('learnerModules.backToModules')}
        </button>
      </div>

      <div className="space-y-2">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold">{module.name}</h1>
            {module.description && <p className="text-muted-foreground mt-1">{module.description}</p>}
          </div>
          <span className="text-2xl font-bold tabular-nums shrink-0">{module.completionPercent}%</span>
        </div>
        <ProgressBar percent={module.completionPercent} />
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold">{t('learnerModules.courses')}</h2>
        {module.courses.length === 0 ? (
          <div className="rounded-md border p-6 text-center text-muted-foreground">{t('learnerModules.noCourses')}</div>
        ) : (
          <div className="space-y-3">
            {module.courses.map((course) => (
              <div key={course.courseId} className="rounded-md border p-4 space-y-2">
                <div className="flex items-start justify-between gap-2">
                  <div className="flex items-center gap-2 flex-wrap">
                    <h3 className="font-semibold">{course.courseName}</h3>
                    {course.isMandatory && (
                      <span className="rounded-full bg-orange-100 px-2 py-0.5 text-xs font-medium text-orange-800">
                        {t('learnerModules.mandatory')}
                      </span>
                    )}
                  </div>
                  <span className="shrink-0 text-sm font-medium tabular-nums">{course.completionPercent}%</span>
                </div>
                <ProgressBar percent={course.completionPercent} />
                <p className="text-xs text-muted-foreground">
                  {t('learnerModules.lessonProgress', { done: course.completedLessons, total: course.totalLessons })}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
