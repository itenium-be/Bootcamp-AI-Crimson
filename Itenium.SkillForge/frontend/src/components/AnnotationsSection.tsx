import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchAnnotations, createAnnotation, updateAnnotation, deleteAnnotation, type Annotation } from '@/api/client';

function StarRating({ value, onChange, readonly }: { value: number; onChange?: (v: number) => void; readonly?: boolean }) {
  const [hovered, setHovered] = useState(0);
  return (
    <div className="flex gap-0.5">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => !readonly && onChange?.(star === value ? 0 : star)}
          onMouseEnter={() => !readonly && setHovered(star)}
          onMouseLeave={() => !readonly && setHovered(0)}
          disabled={readonly}
          className={`text-lg transition-colors disabled:cursor-default ${
            star <= (hovered || value) ? 'text-yellow-400' : 'text-muted-foreground/30'
          }`}
        >
          ★
        </button>
      ))}
    </div>
  );
}

function AnnotationItem({ annotation, lessonId }: { annotation: Annotation; lessonId: number }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [content, setContent] = useState(annotation.content);
  const [rating, setRating] = useState(annotation.rating ?? 0);

  const updateMutation = useMutation({
    mutationFn: () => updateAnnotation(annotation.id, content, rating > 0 ? rating : undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['annotations', lessonId] });
      setEditing(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteAnnotation(annotation.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['annotations', lessonId] });
    },
  });

  if (editing) {
    return (
      <div className="rounded-md border p-3 space-y-2 bg-muted/20">
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          rows={3}
          className="w-full rounded border px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
        />
        <StarRating value={rating} onChange={setRating} />
        <div className="flex gap-2">
          <button
            onClick={() => updateMutation.mutate()}
            disabled={!content.trim() || updateMutation.isPending}
            className="rounded bg-primary px-3 py-1 text-xs text-primary-foreground hover:opacity-90 disabled:opacity-50"
          >
            {t('common.save')}
          </button>
          <button onClick={() => setEditing(false)} className="rounded border px-3 py-1 text-xs hover:bg-muted">
            {t('common.cancel')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-md border p-3 space-y-1">
      <div className="flex items-center justify-between gap-2">
        <span className="text-xs font-medium text-muted-foreground">{annotation.displayName}</span>
        <span className="text-xs text-muted-foreground">{new Date(annotation.createdAt).toLocaleDateString()}</span>
      </div>
      {annotation.rating != null && <StarRating value={annotation.rating} readonly />}
      <p className="text-sm whitespace-pre-wrap">{annotation.content}</p>
      {annotation.isOwn && (
        <div className="flex gap-3 pt-1">
          <button
            onClick={() => {
              setContent(annotation.content);
              setRating(annotation.rating ?? 0);
              setEditing(true);
            }}
            className="text-xs text-primary underline hover:no-underline"
          >
            {t('common.edit')}
          </button>
          <button
            onClick={() => {
              if (window.confirm(t('annotations.confirmDelete'))) deleteMutation.mutate();
            }}
            className="text-xs text-destructive underline hover:no-underline"
          >
            {t('common.delete')}
          </button>
        </div>
      )}
    </div>
  );
}

export function AnnotationsSection({ lessonId }: { lessonId: number }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [content, setContent] = useState('');
  const [rating, setRating] = useState(0);
  const [showForm, setShowForm] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['annotations', lessonId],
    queryFn: () => fetchAnnotations(lessonId),
  });

  const createMutation = useMutation({
    mutationFn: () => createAnnotation(lessonId, content, rating > 0 ? rating : undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['annotations', lessonId] });
      setContent('');
      setRating(0);
      setShowForm(false);
    },
  });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold">{t('annotations.title')}</h2>
        {!showForm && (
          <button
            onClick={() => setShowForm(true)}
            className="text-sm rounded border px-3 py-1 hover:bg-muted"
          >
            {t('annotations.addNote')}
          </button>
        )}
      </div>

      {showForm && (
        <div className="rounded-md border p-3 space-y-2 bg-muted/10">
          <p className="text-xs text-muted-foreground">{t('annotations.shareExperience')}</p>
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder={t('annotations.contentPlaceholder')}
            rows={3}
            className="w-full rounded border px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
          />
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted-foreground">{t('annotations.ratingOptional')}</span>
            <StarRating value={rating} onChange={setRating} />
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => createMutation.mutate()}
              disabled={!content.trim() || createMutation.isPending}
              className="rounded bg-primary px-3 py-1 text-xs text-primary-foreground hover:opacity-90 disabled:opacity-50"
            >
              {createMutation.isPending ? t('common.loading') : t('annotations.post')}
            </button>
            <button onClick={() => setShowForm(false)} className="rounded border px-3 py-1 text-xs hover:bg-muted">
              {t('common.cancel')}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{t('common.loading')}</p>
      ) : data?.items.length === 0 ? (
        <p className="text-sm text-muted-foreground">{t('annotations.noAnnotations')}</p>
      ) : (
        <div className="space-y-3">
          {data?.items.map((a) => (
            <AnnotationItem key={a.id} annotation={a} lessonId={lessonId} />
          ))}
          {data && data.totalCount > data.items.length && (
            <p className="text-xs text-muted-foreground text-center">
              {t('annotations.showingOf', { shown: data.items.length, total: data.totalCount })}
            </p>
          )}
        </div>
      )}
    </div>
  );
}
