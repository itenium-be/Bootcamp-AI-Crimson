import { useState } from 'react';
import { useParams, Link } from '@tanstack/react-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import {
  fetchModules,
  fetchCourses,
  updateModule,
  addCourseToModule,
  removeCourseFromModule,
  reorderModuleCourses,
} from '@/api/client';
import type { ModuleCourse } from '@/api/client';

export function ManagerModuleDetail() {
  const { t } = useTranslation();
  const { id } = useParams({ from: '/_authenticated/manager/modules/$id' });
  const moduleId = Number(id);
  const queryClient = useQueryClient();

  const [editing, setEditing] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [goal, setGoal] = useState('');
  const [selectedCourseId, setSelectedCourseId] = useState('');

  const { data: modules, isLoading } = useQuery({
    queryKey: ['modules'],
    queryFn: fetchModules,
  });

  const { data: allCourses } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const module = modules?.find((m) => m.id === moduleId);

  const updateMutation = useMutation({
    mutationFn: () => updateModule(moduleId, { name, description: description || null, goal: goal || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['modules'] });
      setEditing(false);
    },
  });

  const addCourseMutation = useMutation({
    mutationFn: (courseId: number) => addCourseToModule(moduleId, courseId, (module?.courses.length ?? 0) + 1),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['modules'] });
      setSelectedCourseId('');
    },
  });

  const removeMutation = useMutation({
    mutationFn: (courseId: number) => removeCourseFromModule(moduleId, courseId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['modules'] }),
  });

  const reorderMutation = useMutation({
    mutationFn: (orderedIds: number[]) => reorderModuleCourses(moduleId, orderedIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['modules'] }),
  });

  function startEdit() {
    setName(module?.name ?? '');
    setDescription(module?.description ?? '');
    setGoal(module?.goal ?? '');
    setEditing(true);
  }

  function handleSave(e: React.FormEvent) {
    e.preventDefault();
    updateMutation.mutate();
  }

  function handleAddCourse() {
    if (!selectedCourseId) return;
    addCourseMutation.mutate(Number(selectedCourseId));
  }

  function handleRemove(c: ModuleCourse) {
    if (window.confirm(t('modules.confirmRemoveCourse', { name: c.courseName }))) {
      removeMutation.mutate(c.courseId);
    }
  }

  function moveUp(courses: ModuleCourse[], index: number) {
    if (index === 0) return;
    const reordered = [...courses];
    [reordered[index - 1], reordered[index]] = [reordered[index], reordered[index - 1]];
    reorderMutation.mutate(reordered.map((c) => c.courseId));
  }

  function moveDown(courses: ModuleCourse[], index: number) {
    if (index === courses.length - 1) return;
    const reordered = [...courses];
    [reordered[index], reordered[index + 1]] = [reordered[index + 1], reordered[index]];
    reorderMutation.mutate(reordered.map((c) => c.courseId));
  }

  if (isLoading) return <div>{t('common.loading')}</div>;
  if (!module) return <div>{t('common.error')}</div>;

  const assignedCourseIds = new Set(module.courses.map((c) => c.courseId));
  const availableCourses = allCourses?.filter((c) => !assignedCourseIds.has(c.id)) ?? [];
  const sortedCourses = [...module.courses].sort((a, b) => a.order - b.order);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link to="/manager/modules" className="text-sm text-muted-foreground hover:underline">
          ← {t('modules.backToList')}
        </Link>
        <h1 className="text-3xl font-bold">{module.name}</h1>
      </div>

      {editing ? (
        <form onSubmit={handleSave} className="rounded-md border p-4 space-y-3">
          <div>
            <label className="block text-sm font-medium mb-1">{t('modules.name')} *</label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="border rounded px-3 py-2 w-full text-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('modules.description')}</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="border rounded px-3 py-2 w-full text-sm"
              rows={2}
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('modules.goal')}</label>
            <input
              value={goal}
              onChange={(e) => setGoal(e.target.value)}
              className="border rounded px-3 py-2 w-full text-sm"
            />
          </div>
          <div className="flex gap-2">
            <button
              type="submit"
              className="rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:bg-primary/90"
            >
              {t('modules.save')}
            </button>
            <button
              type="button"
              onClick={() => setEditing(false)}
              className="rounded border px-3 py-1.5 text-sm hover:bg-muted"
            >
              {t('modules.cancel')}
            </button>
          </div>
        </form>
      ) : (
        <div className="rounded-md border p-4 space-y-2">
          {module.description && <p className="text-muted-foreground text-sm">{module.description}</p>}
          {module.goal && (
            <p className="text-sm">
              <strong>{t('modules.goal')}:</strong> {module.goal}
            </p>
          )}
          <button onClick={startEdit} className="text-xs underline hover:no-underline mt-2">
            {t('modules.editModule')}
          </button>
        </div>
      )}

      {/* Add course */}
      <div className="flex items-center gap-3">
        <select
          value={selectedCourseId}
          onChange={(e) => setSelectedCourseId(e.target.value)}
          className="border rounded px-3 py-2 text-sm flex-1"
        >
          <option value="">{t('modules.selectCourse')}</option>
          {availableCourses.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
        <button
          onClick={handleAddCourse}
          disabled={!selectedCourseId}
          className="rounded bg-primary px-3 py-2 text-sm text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          {t('modules.addCourse')}
        </button>
      </div>

      {/* Course list */}
      <div className="rounded-md border">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium w-12">#</th>
              <th className="p-3 text-left font-medium">{t('courses.name')}</th>
              <th className="p-3 text-left font-medium">Order</th>
              <th className="p-3 text-left font-medium">{t('courses.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {sortedCourses.map((c, i) => (
              <tr key={c.courseId} className="border-b">
                <td className="p-3 text-muted-foreground">{i + 1}</td>
                <td className="p-3 font-medium">{c.courseName}</td>
                <td className="p-3">
                  <div className="flex gap-1">
                    <button
                      onClick={() => moveUp(sortedCourses, i)}
                      disabled={i === 0}
                      className="px-2 py-0.5 border rounded text-xs disabled:opacity-30 hover:bg-muted"
                    >
                      ↑
                    </button>
                    <button
                      onClick={() => moveDown(sortedCourses, i)}
                      disabled={i === sortedCourses.length - 1}
                      className="px-2 py-0.5 border rounded text-xs disabled:opacity-30 hover:bg-muted"
                    >
                      ↓
                    </button>
                  </div>
                </td>
                <td className="p-3">
                  <button
                    onClick={() => handleRemove(c)}
                    className="text-xs text-destructive underline hover:no-underline"
                  >
                    {t('modules.removeCourse')}
                  </button>
                </td>
              </tr>
            ))}
            {sortedCourses.length === 0 && (
              <tr>
                <td colSpan={4} className="p-3 text-center text-muted-foreground">
                  {t('modules.noCourses')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
