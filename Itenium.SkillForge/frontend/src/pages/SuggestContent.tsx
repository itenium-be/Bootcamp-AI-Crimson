import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { fetchMyContentSuggestions, submitContentSuggestion } from '@/api/client';
import type { SubmitContentSuggestionRequest } from '@/api/client';

export function SuggestContent() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [url, setUrl] = useState('');
  const [topic, setTopic] = useState('');

  const { data: suggestions = [] } = useQuery({
    queryKey: ['my-content-suggestions'],
    queryFn: fetchMyContentSuggestions,
  });

  const submitMutation = useMutation({
    mutationFn: (req: SubmitContentSuggestionRequest) => submitContentSuggestion(req),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['my-content-suggestions'] });
      setTitle('');
      setDescription('');
      setUrl('');
      setTopic('');
      toast.success(t('suggestContent.successMessage'));
    },
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    submitMutation.mutate({
      title,
      description,
      url: url || undefined,
      topic: topic || undefined,
    });
  }

  const statusColors: Record<string, string> = {
    Pending: 'bg-yellow-100 text-yellow-800',
    Approved: 'bg-green-100 text-green-800',
    Rejected: 'bg-red-100 text-red-800',
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">{t('suggestContent.title')}</h1>
        <p className="text-muted-foreground">{t('suggestContent.subtitle')}</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div>
          <label className="block text-sm font-medium mb-1">{t('suggestContent.formTitle')}</label>
          <input
            type="text"
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="border rounded px-3 py-2 text-sm w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('suggestContent.formDescription')}</label>
          <textarea
            required
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={4}
            className="border rounded px-3 py-2 text-sm w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('suggestContent.formUrl')}</label>
          <input
            type="url"
            value={url}
            onChange={(e) => setUrl(e.target.value)}
            className="border rounded px-3 py-2 text-sm w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('suggestContent.formTopic')}</label>
          <input
            type="text"
            value={topic}
            onChange={(e) => setTopic(e.target.value)}
            className="border rounded px-3 py-2 text-sm w-full"
          />
        </div>

        <button
          type="submit"
          disabled={submitMutation.isPending}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          {submitMutation.isPending ? t('suggestContent.submitting') : t('suggestContent.submit')}
        </button>
      </form>

      <div>
        <h2 className="text-xl font-semibold mb-4">{t('suggestContent.mySuggestions')}</h2>
        {suggestions.length === 0 ? (
          <p className="text-muted-foreground">{t('suggestContent.noSuggestions')}</p>
        ) : (
          <div className="rounded-md border overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="p-3 text-left font-medium">{t('suggestContent.formTitle')}</th>
                  <th className="p-3 text-left font-medium">{t('suggestContent.formTopic')}</th>
                  <th className="p-3 text-left font-medium">Status</th>
                  <th className="p-3 text-left font-medium">{t('suggestContent.reviewNote')}</th>
                </tr>
              </thead>
              <tbody>
                {suggestions.map((s) => (
                  <tr key={s.id} className="border-b">
                    <td className="p-3 font-medium">{s.title}</td>
                    <td className="p-3 text-muted-foreground">{s.topic ?? '—'}</td>
                    <td className="p-3">
                      <span className={`text-xs px-2 py-0.5 rounded font-medium ${statusColors[s.status] ?? ''}`}>
                        {t(`suggestContent.status.${s.status}`)}
                      </span>
                    </td>
                    <td className="p-3 text-muted-foreground">{s.reviewNote ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
