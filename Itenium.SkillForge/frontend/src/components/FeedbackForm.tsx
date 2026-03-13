import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { FeedbackEntry } from '@/api/client';

interface FeedbackFormProps {
  queryKey: unknown[];
  fetchFn: () => Promise<FeedbackEntry | null>;
  submitFn: (rating: number, comment?: string) => Promise<FeedbackEntry>;
  updateFn: (rating: number, comment?: string) => Promise<void>;
}

function StarRating({ value, onChange }: { value: number; onChange: (v: number) => void }) {
  const [hovered, setHovered] = useState(0);
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onChange(star)}
          onMouseEnter={() => setHovered(star)}
          onMouseLeave={() => setHovered(0)}
          className={`text-2xl transition-colors ${
            star <= (hovered || value) ? 'text-yellow-400' : 'text-muted-foreground/30'
          }`}
        >
          ★
        </button>
      ))}
    </div>
  );
}

export function FeedbackForm({ queryKey, fetchFn, submitFn, updateFn }: FeedbackFormProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [editing, setEditing] = useState(false);

  const { data: existing, isLoading } = useQuery({
    queryKey,
    queryFn: fetchFn,
  });

  const mutation = useMutation({
    mutationFn: async () => {
      if (existing && !editing) return;
      if (existing) {
        await updateFn(rating, comment || undefined);
      } else {
        await submitFn(rating, comment || undefined);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    },
  });

  if (isLoading) return null;

  if (existing && !editing) {
    return (
      <div className="rounded-md border p-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium">{t('feedback.yourFeedback')}</span>
          <button
            onClick={() => {
              setRating(existing.rating);
              setComment(existing.comment ?? '');
              setEditing(true);
            }}
            className="text-xs text-primary underline hover:no-underline"
          >
            {t('common.edit')}
          </button>
        </div>
        <div className="flex gap-0.5">
          {[1, 2, 3, 4, 5].map((s) => (
            <span key={s} className={`text-xl ${s <= existing.rating ? 'text-yellow-400' : 'text-muted-foreground/30'}`}>★</span>
          ))}
        </div>
        {existing.comment && <p className="text-sm text-muted-foreground">{existing.comment}</p>}
      </div>
    );
  }

  return (
    <div className="rounded-md border p-4 space-y-3">
      <p className="text-sm font-medium">{existing ? t('feedback.editFeedback') : t('feedback.leaveFeedback')}</p>
      <StarRating value={rating} onChange={setRating} />
      <textarea
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        placeholder={t('feedback.commentPlaceholder')}
        rows={3}
        className="w-full rounded-md border px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
      />
      <div className="flex gap-2">
        <button
          onClick={() => mutation.mutate()}
          disabled={rating === 0 || mutation.isPending}
          className="rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:opacity-90 disabled:opacity-50"
        >
          {mutation.isPending ? t('common.loading') : t('common.save')}
        </button>
        {editing && (
          <button
            onClick={() => setEditing(false)}
            className="rounded border px-3 py-1.5 text-sm hover:bg-muted"
          >
            {t('common.cancel')}
          </button>
        )}
      </div>
    </div>
  );
}
