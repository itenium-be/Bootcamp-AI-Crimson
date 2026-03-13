import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams, Link } from '@tanstack/react-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { fetchCourse, createCourse, updateCourse } from '@/api/client';
import type { CourseFormData } from '@/api/client';

export function ManagerCourseForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const params = useParams({ strict: false });
  const courseId = params.id && params.id !== 'new' ? Number(params.id) : null;
  const isEdit = courseId !== null;

  const { data: existing, isLoading } = useQuery({
    queryKey: ['course', courseId],
    queryFn: () => fetchCourse(courseId ?? 0),
    enabled: isEdit,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CourseFormData>({
    defaultValues: { name: '', description: null, category: null, level: null, estimatedDuration: null },
  });

  useEffect(() => {
    if (existing) {
      reset({
        name: existing.name,
        description: existing.description,
        category: existing.category,
        level: existing.level,
        estimatedDuration: existing.estimatedDuration,
      });
    }
  }, [existing, reset]);

  const createMutation = useMutation({
    mutationFn: createCourse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      void navigate({ to: '/manager/courses' });
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: CourseFormData) => updateCourse(courseId ?? 0, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      void navigate({ to: '/manager/courses' });
    },
  });

  function onSubmit(data: CourseFormData) {
    const payload: CourseFormData = {
      ...data,
      estimatedDuration: data.estimatedDuration ? Number(data.estimatedDuration) : null,
    };
    if (isEdit) {
      updateMutation.mutate(payload);
    } else {
      createMutation.mutate(payload);
    }
  }

  if (isEdit && isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">
          {isEdit ? t('managerCourses.editCourse') : t('managerCourses.newCourse')}
        </h1>
        {isEdit && (
          <Link
            to="/manager/courses/$id/lessons"
            params={{ id: String(courseId) }}
            className="text-sm text-primary hover:underline"
          >
            {t('lessons.manage')}
          </Link>
        )}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('courses.name')} *</label>
          <input
            {...register('name', { required: t('managerCourses.nameRequired') })}
            className="w-full border rounded px-3 py-2 text-sm"
          />
          {errors.name && <p className="text-xs text-destructive mt-1">{errors.name.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('courses.description')}</label>
          <textarea {...register('description')} rows={4} className="w-full border rounded px-3 py-2 text-sm" />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('courses.category')}</label>
            <input {...register('category')} className="w-full border rounded px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('courses.level')}</label>
            <select {...register('level')} className="w-full border rounded px-3 py-2 text-sm">
              <option value="">—</option>
              <option value="Beginner">Beginner</option>
              <option value="Intermediate">Intermediate</option>
              <option value="Advanced">Advanced</option>
            </select>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">
            {t('managerCourses.duration')} ({t('managerCourses.durationUnit')})
          </label>
          <input
            type="number"
            min={1}
            {...register('estimatedDuration')}
            className="w-full border rounded px-3 py-2 text-sm"
          />
        </div>

        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={isPending}
            className="rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            {isPending ? t('common.loading') : t('common.save')}
          </button>
          <button
            type="button"
            onClick={() => void navigate({ to: '/manager/courses' })}
            className="rounded border px-4 py-2 text-sm font-medium hover:bg-muted"
          >
            {t('common.cancel')}
          </button>
        </div>
      </form>
    </div>
  );
}
