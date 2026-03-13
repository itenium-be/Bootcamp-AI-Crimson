import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchMyModules } from '@/api/client';

function ProgressBar({ percent }: { percent: number }) {
  return (
    <div className="w-full bg-muted rounded-full h-2">
      <div className="bg-primary h-2 rounded-full transition-all" style={{ width: `${percent}%` }} />
    </div>
  );
}

export function LearnerModules() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { data: modules = [], isLoading } = useQuery({
    queryKey: ['learner', 'modules'],
    queryFn: fetchMyModules,
  });

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('learnerModules.title')}</h1>
        <p className="text-muted-foreground mt-1">{t('learnerModules.subtitle')}</p>
      </div>

      {modules.length === 0 ? (
        <div className="rounded-md border p-8 text-center text-muted-foreground">{t('learnerModules.noModules')}</div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {modules.map((module) => (
            <div
              key={module.id}
              className="rounded-md border p-4 space-y-3 cursor-pointer hover:bg-muted/50"
              onClick={() => navigate({ to: '/modules/$id', params: { id: String(module.id) } })}
            >
              <div className="flex items-start justify-between gap-2">
                <h3 className="font-semibold leading-tight">{module.name}</h3>
                <span className="shrink-0 text-sm font-medium tabular-nums">{module.completionPercent}%</span>
              </div>
              {module.description && <p className="text-sm text-muted-foreground line-clamp-2">{module.description}</p>}
              <ProgressBar percent={module.completionPercent} />
              <p className="text-xs text-muted-foreground">
                {t('learnerModules.courseCount', { count: module.courses.length })}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
