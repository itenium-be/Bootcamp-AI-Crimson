import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { fetchCourses, fetchLessons, fetchLessonProgressSummary, resetLessonProgress } from '@/api/client';
import type { Lesson } from '@/api/client';

export function ManagerLessonProgress() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null);

  const { data: courses } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const { data: lessons = [], isLoading: lessonsLoading } = useQuery({
    queryKey: ['lessons', selectedCourseId],
    queryFn: () => fetchLessons(selectedCourseId ?? 0),
    enabled: selectedCourseId != null,
  });

  function handleReset(lesson: Lesson) {
    if (window.confirm(t('lessonProgress.confirmReset', { title: lesson.title }))) {
      resetMutation.mutate(lesson.id);
    }
  }

  const resetMutation = useMutation({
    mutationFn: (lessonId: number) => resetLessonProgress(lessonId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lessonProgress'] });
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('lessonProgress.title')}</h1>
        <p className="text-muted-foreground">{t('lessonProgress.subtitle')}</p>
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('courses.name')}</label>
        <select
          value={selectedCourseId ?? ''}
          onChange={(e) => setSelectedCourseId(e.target.value ? Number(e.target.value) : null)}
          className="border rounded px-3 py-2 text-sm w-full max-w-xs"
        >
          <option value="">{t('lessonProgress.selectCourse')}</option>
          {courses?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
      </div>

      {selectedCourseId &&
        (lessonsLoading ? (
          <div>{t('common.loading')}</div>
        ) : lessons.length === 0 ? (
          <p className="text-muted-foreground">{t('lessonProgress.noLessons')}</p>
        ) : (
          <div className="rounded-md border overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="p-3 text-left font-medium">{t('lessonProgress.lesson')}</th>
                  <th className="p-3 text-right font-medium">{t('lessonProgress.completedCount')}</th>
                  <th className="p-3 text-left font-medium"></th>
                  <th className="p-3 text-center font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {lessons.map((lesson) => (
                  <LessonProgressRow key={lesson.id} lesson={lesson} onReset={handleReset} />
                ))}
              </tbody>
            </table>
          </div>
        ))}
    </div>
  );
}

function LessonProgressRow({ lesson, onReset }: { lesson: Lesson; onReset: (l: Lesson) => void }) {
  const { t } = useTranslation();

  const { data: summary } = useQuery({
    queryKey: ['lessonProgress', lesson.id],
    queryFn: () => fetchLessonProgressSummary(lesson.id),
  });

  const count = summary?.completedCount ?? 0;

  return (
    <tr className="border-b">
      <td className="p-3 font-medium">{lesson.title}</td>
      <td className="p-3 text-right">{count}</td>
      <td className="p-3">
        {count > 0 && (
          <span className="text-xs px-2 py-0.5 bg-yellow-100 text-yellow-800 rounded">
            {t('lessonProgress.warning', { count })}
          </span>
        )}
      </td>
      <td className="p-3 text-center">
        {count > 0 && (
          <button onClick={() => onReset(lesson)} className="text-xs text-destructive underline hover:no-underline">
            {t('lessonProgress.reset')}
          </button>
        )}
      </td>
    </tr>
  );
}
