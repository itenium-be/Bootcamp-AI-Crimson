import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from '@tanstack/react-router';
import {
  fetchLessons,
  setLessonStatus,
  trackLastVisited,
  fetchResumeLesson,
  getMyCourseFeedback,
  submitCourseFeedback,
  updateCourseFeedback,
  type Lesson,
  type LessonStatus,
} from '@/api/client';
import { FeedbackForm } from '@/components/FeedbackForm';

const STATUS_CYCLE: Record<LessonStatus, LessonStatus> = {
  new: 'done',
  done: 'later',
  later: 'new',
};

const STATUS_LABEL: Record<LessonStatus, string> = {
  new: '○',
  done: '✓',
  later: '◷',
};

export function CourseLessons() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams({ from: '/_authenticated/courses/$id' });
  const courseId = Number(id);
  const queryClient = useQueryClient();

  const { data: lessons = [], isLoading } = useQuery({
    queryKey: ['lessons', courseId],
    queryFn: () => fetchLessons(courseId),
  });

  const { data: resume } = useQuery({
    queryKey: ['resume', courseId],
    queryFn: () => fetchResumeLesson(courseId),
  });

  const mutation = useMutation({
    mutationFn: ({ lessonId, status }: { lessonId: number; status: LessonStatus }) => setLessonStatus(lessonId, status),
    onMutate: async ({ lessonId, status }) => {
      await queryClient.cancelQueries({ queryKey: ['lessons', courseId] });
      const previous = queryClient.getQueryData<Lesson[]>(['lessons', courseId]);
      queryClient.setQueryData<Lesson[]>(['lessons', courseId], (old = []) =>
        old.map((l) => (l.id === lessonId ? { ...l, status } : l)),
      );
      return { previous };
    },
    onError: (_err, _vars, context) => {
      if (context?.previous) {
        queryClient.setQueryData(['lessons', courseId], context.previous);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resume', courseId] });
    },
  });

  function toggleStatus(lesson: Lesson) {
    const next = STATUS_CYCLE[lesson.status];
    mutation.mutate({ lessonId: lesson.id, status: next });
  }

  function handleLessonClick(lessonId: number) {
    void trackLastVisited(courseId, lessonId);
  }

  function handleResume() {
    if (resume?.lessonId) {
      navigate({ to: '/lessons/$lessonId', params: { lessonId: String(resume.lessonId) } });
    }
  }

  const doneCount = lessons.filter((l) => l.status === 'done').length;
  const progress = lessons.length > 0 ? Math.round((doneCount / lessons.length) * 100) : 0;

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">{t('lessons.title')}</h1>
          {lessons.length > 0 && (
            <p className="text-muted-foreground mt-1">
              {t('lessons.progress', { done: doneCount, total: lessons.length, pct: progress })}
            </p>
          )}
        </div>
        {resume &&
          (resume.isComplete ? (
            <div className="flex flex-col items-end gap-1">
              <span className="text-sm text-green-700 font-medium">{t('myLearning.courseComplete')}</span>
              {resume.lessonId && (
                <button onClick={handleResume} className="text-sm rounded border px-3 py-1.5 hover:bg-muted">
                  {t('myLearning.revisit')}
                </button>
              )}
            </div>
          ) : resume.lessonId ? (
            <button
              onClick={handleResume}
              className="shrink-0 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              {t('lessons.resume')}
            </button>
          ) : null)}
      </div>

      {lessons.length > 0 && (
        <div className="h-2 rounded-full bg-muted overflow-hidden">
          <div className="h-full bg-primary transition-all" style={{ width: `${progress}%` }} />
        </div>
      )}

      <div className="rounded-md border divide-y">
        {lessons.map((lesson) => (
          <div key={lesson.id} className="flex items-center gap-3 p-3">
            <button
              onClick={() => toggleStatus(lesson)}
              title={t(`lessons.status.${lesson.status}`)}
              className={`text-xl w-8 h-8 flex items-center justify-center rounded-full transition-colors
                ${lesson.status === 'done' ? 'text-green-600 bg-green-50 hover:bg-green-100' : ''}
                ${lesson.status === 'later' ? 'text-amber-600 bg-amber-50 hover:bg-amber-100' : ''}
                ${lesson.status === 'new' ? 'text-muted-foreground hover:bg-muted' : ''}
              `}
            >
              {STATUS_LABEL[lesson.status]}
            </button>
            <Link
              to="/lessons/$lessonId"
              params={{ lessonId: String(lesson.id) }}
              onClick={() => handleLessonClick(lesson.id)}
              className={`hover:underline ${lesson.status === 'done' ? 'line-through text-muted-foreground' : ''}`}
            >
              {lesson.title}
            </Link>
          </div>
        ))}
        {lessons.length === 0 && <div className="p-4 text-center text-muted-foreground">{t('lessons.noLessons')}</div>}
      </div>

      {resume?.isComplete && (
        <>
          <hr className="border-muted" />
          <div className="space-y-2">
            <p className="text-sm font-medium">{t('feedback.courseCompletePrompt')}</p>
            <FeedbackForm
              queryKey={['course-feedback-me', courseId]}
              fetchFn={() => getMyCourseFeedback(courseId)}
              submitFn={(rating, comment) => submitCourseFeedback(courseId, rating, comment)}
              updateFn={(rating, comment) => updateCourseFeedback(courseId, rating, comment)}
            />
          </div>
        </>
      )}
    </div>
  );
}
