import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from '@tanstack/react-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  fetchCourseLessons,
  createLesson,
  updateLesson,
  deleteLesson,
  reorderLessons,
  type LessonItem,
  type CreateLessonData,
} from '@/api/client';

export type { LessonItem };

export function sortLessons(lessons: LessonItem[]): LessonItem[] {
  return [...lessons].sort((a, b) => a.sortOrder - b.sortOrder);
}

export function filterLessonsByTitle(lessons: LessonItem[], search: string): LessonItem[] {
  if (!search) return lessons;
  const lower = search.toLowerCase();
  return lessons.filter((l) => l.title.toLowerCase().includes(lower));
}

export function getNextSortOrder(lessons: LessonItem[]): number {
  if (lessons.length === 0) return 1;
  return Math.max(...lessons.map((l) => l.sortOrder)) + 1;
}

interface LessonFormData {
  title: string;
  estimatedDuration: string;
}

interface Props {
  courseId: number;
}

export function CourseLessonsManager({ courseId }: Props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [editingId, setEditingId] = useState<number | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data: lessons = [], isLoading } = useQuery({
    queryKey: ['manager-lessons', courseId],
    queryFn: () => fetchCourseLessons(courseId),
  });

  const sorted = sortLessons(lessons);

  const addForm = useForm<LessonFormData>({
    defaultValues: { title: '', estimatedDuration: '' },
  });

  const editForm = useForm<LessonFormData>({
    defaultValues: { title: '', estimatedDuration: '' },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateLessonData) => createLesson(courseId, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['manager-lessons', courseId] });
      setShowAddForm(false);
      addForm.reset();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: CreateLessonData }) => updateLesson(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['manager-lessons', courseId] });
      setEditingId(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteLesson(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['manager-lessons', courseId] });
      setDeleteError(null);
    },
    onError: (error: { response?: { status?: number } }) => {
      if (error?.response?.status === 409) {
        setDeleteError(t('lessons.deleteBlocked'));
      }
    },
  });

  const reorderMutation = useMutation({
    mutationFn: (orderedIds: number[]) => reorderLessons(courseId, orderedIds),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['manager-lessons', courseId] });
    },
  });

  function handleAddSubmit(data: LessonFormData) {
    createMutation.mutate({
      title: data.title,
      estimatedDuration: data.estimatedDuration ? Number(data.estimatedDuration) : null,
      sortOrder: getNextSortOrder(lessons),
    });
  }

  function startEdit(lesson: LessonItem) {
    setEditingId(lesson.id);
    editForm.reset({
      title: lesson.title,
      estimatedDuration: lesson.estimatedDuration != null ? String(lesson.estimatedDuration) : '',
    });
  }

  function handleEditSubmit(data: LessonFormData) {
    if (editingId == null) return;
    const lesson = lessons.find((l) => l.id === editingId);
    updateMutation.mutate({
      id: editingId,
      data: {
        title: data.title,
        estimatedDuration: data.estimatedDuration ? Number(data.estimatedDuration) : null,
        sortOrder: lesson?.sortOrder ?? 1,
      },
    });
  }

  function handleMoveUp(lesson: LessonItem) {
    const idx = sorted.findIndex((l) => l.id === lesson.id);
    if (idx <= 0) return;
    const newOrder = sorted.map((l) => l.id);
    newOrder.splice(idx, 1);
    newOrder.splice(idx - 1, 0, lesson.id);
    reorderMutation.mutate(newOrder);
  }

  function handleMoveDown(lesson: LessonItem) {
    const idx = sorted.findIndex((l) => l.id === lesson.id);
    if (idx < 0 || idx >= sorted.length - 1) return;
    const newOrder = sorted.map((l) => l.id);
    newOrder.splice(idx, 1);
    newOrder.splice(idx + 1, 0, lesson.id);
    reorderMutation.mutate(newOrder);
  }

  function handleDelete(lesson: LessonItem) {
    if (!window.confirm(t('lessons.confirmDelete', { title: lesson.title }))) return;
    setDeleteError(null);
    deleteMutation.mutate(lesson.id);
  }

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('lessons.manage')}</h1>
        <button
          type="button"
          onClick={() => void navigate({ to: '/manager/courses' })}
          className="text-sm text-muted-foreground hover:underline"
        >
          {t('lessons.backToCourse')}
        </button>
      </div>

      {deleteError && (
        <div className="rounded border border-destructive bg-destructive/10 px-4 py-2 text-sm text-destructive">
          {deleteError}
        </div>
      )}

      <div className="rounded-md border divide-y">
        {sorted.length === 0 && <div className="p-4 text-center text-muted-foreground">{t('lessons.noLessons')}</div>}
        {sorted.map((lesson, idx) => (
          <div key={lesson.id} className="p-3">
            {editingId === lesson.id ? (
              <form onSubmit={editForm.handleSubmit(handleEditSubmit)} className="flex items-center gap-2">
                <input
                  {...editForm.register('title', { required: t('lessons.nameRequired') })}
                  className="flex-1 border rounded px-2 py-1 text-sm"
                  placeholder={t('lessons.lessonTitle')}
                />
                {editForm.formState.errors.title && (
                  <span className="text-xs text-destructive">{editForm.formState.errors.title.message}</span>
                )}
                <input
                  type="number"
                  min={1}
                  {...editForm.register('estimatedDuration')}
                  className="w-24 border rounded px-2 py-1 text-sm"
                  placeholder={t('lessons.estimatedDuration')}
                />
                <button
                  type="submit"
                  disabled={updateMutation.isPending}
                  className="rounded bg-primary px-3 py-1 text-xs text-primary-foreground hover:bg-primary/90"
                >
                  {t('common.save')}
                </button>
                <button
                  type="button"
                  onClick={() => setEditingId(null)}
                  className="rounded border px-3 py-1 text-xs hover:bg-muted"
                >
                  {t('common.cancel')}
                </button>
              </form>
            ) : (
              <div className="flex items-center gap-3">
                <span className="text-muted-foreground text-xs w-6 text-right">{lesson.sortOrder}</span>
                <span className="flex-1 text-sm">{lesson.title}</span>
                {lesson.estimatedDuration != null && (
                  <span className="text-xs text-muted-foreground">{lesson.estimatedDuration} min</span>
                )}
                <div className="flex gap-1">
                  <button
                    type="button"
                    onClick={() => handleMoveUp(lesson)}
                    disabled={idx === 0 || reorderMutation.isPending}
                    title={t('lessons.moveUp')}
                    className="px-1.5 py-0.5 text-xs rounded border hover:bg-muted disabled:opacity-30"
                  >
                    ↑
                  </button>
                  <button
                    type="button"
                    onClick={() => handleMoveDown(lesson)}
                    disabled={idx === sorted.length - 1 || reorderMutation.isPending}
                    title={t('lessons.moveDown')}
                    className="px-1.5 py-0.5 text-xs rounded border hover:bg-muted disabled:opacity-30"
                  >
                    ↓
                  </button>
                  <button
                    type="button"
                    onClick={() => startEdit(lesson)}
                    className="px-2 py-0.5 text-xs rounded border hover:bg-muted"
                  >
                    {t('common.edit')}
                  </button>
                  <button
                    type="button"
                    onClick={() => handleDelete(lesson)}
                    disabled={deleteMutation.isPending}
                    className="px-2 py-0.5 text-xs rounded border text-destructive hover:bg-destructive/10"
                  >
                    {t('common.delete')}
                  </button>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>

      {showAddForm ? (
        <form onSubmit={addForm.handleSubmit(handleAddSubmit)} className="rounded-md border p-4 space-y-3">
          <h2 className="font-semibold text-sm">{t('lessons.addLesson')}</h2>
          <div>
            <label className="block text-xs font-medium mb-1">{t('lessons.lessonTitle')} *</label>
            <input
              {...addForm.register('title', { required: t('lessons.nameRequired') })}
              className="w-full border rounded px-3 py-2 text-sm"
            />
            {addForm.formState.errors.title && (
              <p className="text-xs text-destructive mt-1">{addForm.formState.errors.title.message}</p>
            )}
          </div>
          <div>
            <label className="block text-xs font-medium mb-1">{t('lessons.estimatedDuration')}</label>
            <input
              type="number"
              min={1}
              {...addForm.register('estimatedDuration')}
              className="w-full border rounded px-3 py-2 text-sm"
            />
          </div>
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
            >
              {createMutation.isPending ? t('common.loading') : t('lessons.addLesson')}
            </button>
            <button
              type="button"
              onClick={() => {
                setShowAddForm(false);
                addForm.reset();
              }}
              className="rounded border px-4 py-2 text-sm font-medium hover:bg-muted"
            >
              {t('common.cancel')}
            </button>
          </div>
        </form>
      ) : (
        <button
          type="button"
          onClick={() => setShowAddForm(true)}
          className="rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          {t('lessons.addLesson')}
        </button>
      )}
    </div>
  );
}
