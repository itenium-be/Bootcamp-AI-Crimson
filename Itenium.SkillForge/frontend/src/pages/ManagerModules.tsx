import { useState } from 'react';
import { Link } from '@tanstack/react-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { fetchModules, createModule, deleteModule } from '@/api/client';
import type { Module } from '@/api/client';

export function ManagerModules() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [goal, setGoal] = useState('');
  const [error, setError] = useState('');

  const { data: modules, isLoading } = useQuery({
    queryKey: ['modules'],
    queryFn: fetchModules,
  });

  const createMutation = useMutation({
    mutationFn: () => createModule({ name, description: description || null, goal: goal || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['modules'] });
      setShowForm(false);
      setName('');
      setDescription('');
      setGoal('');
      setError('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteModule(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['modules'] }),
  });

  function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) {
      setError(t('modules.nameRequired'));
      return;
    }
    createMutation.mutate();
  }

  function handleDelete(m: Module) {
    if (window.confirm(t('modules.confirmDelete', { name: m.name }))) {
      deleteMutation.mutate(m.id);
    }
  }

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('modules.title')}</h1>
        <button
          onClick={() => setShowForm(true)}
          className="rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          {t('modules.newModule')}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="rounded-md border p-4 space-y-3">
          <h2 className="text-lg font-semibold">{t('modules.newModule')}</h2>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div>
            <label className="block text-sm font-medium mb-1">{t('modules.name')} *</label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="border rounded px-3 py-2 w-full text-sm"
              placeholder={t('modules.name')}
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
            <button type="submit" className="rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:bg-primary/90">
              {t('modules.save')}
            </button>
            <button type="button" onClick={() => setShowForm(false)} className="rounded border px-3 py-1.5 text-sm hover:bg-muted">
              {t('modules.cancel')}
            </button>
          </div>
        </form>
      )}

      <div className="rounded-md border">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('modules.name')}</th>
              <th className="p-3 text-left font-medium">{t('modules.goal')}</th>
              <th className="p-3 text-left font-medium">{t('modules.courses')}</th>
              <th className="p-3 text-left font-medium">{t('courses.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {modules?.map((m) => (
              <tr key={m.id} className="border-b">
                <td className="p-3 font-medium">{m.name}</td>
                <td className="p-3 text-muted-foreground">{m.goal ?? '—'}</td>
                <td className="p-3 text-muted-foreground">{t('modules.courseCount', { count: m.courses.length })}</td>
                <td className="p-3">
                  <div className="flex gap-2">
                    <Link
                      to="/manager/modules/$id"
                      params={{ id: String(m.id) }}
                      className="text-xs underline hover:no-underline"
                    >
                      {t('common.edit')}
                    </Link>
                    <button
                      onClick={() => handleDelete(m)}
                      className="text-xs text-destructive underline hover:no-underline"
                    >
                      {t('common.delete')}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {modules?.length === 0 && (
              <tr>
                <td colSpan={4} className="p-3 text-center text-muted-foreground">
                  {t('modules.noModules')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
