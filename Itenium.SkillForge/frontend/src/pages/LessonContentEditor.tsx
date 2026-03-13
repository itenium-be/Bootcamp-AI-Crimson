import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams } from '@tanstack/react-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchContentBlocks,
  createContentBlock,
  updateContentBlock,
  deleteContentBlock,
  reorderContentBlocks,
  type ContentBlock,
  type ContentBlockType,
} from '@/api/client';

const BLOCK_TYPES: ContentBlockType[] = ['text', 'image', 'video', 'pdf', 'link', 'youtube'];

function parseContent(content: string): Record<string, string> {
  try {
    return JSON.parse(content) as Record<string, string>;
  } catch {
    return {};
  }
}

function buildContent(fields: Record<string, string>): string {
  return JSON.stringify(fields);
}

interface BlockFormProps {
  type: ContentBlockType;
  initial?: Record<string, string>;
  onSave: (fields: Record<string, string>) => void;
  onCancel: () => void;
  isPending: boolean;
}

function BlockForm({ type, initial = {}, onSave, onCancel, isPending }: BlockFormProps) {
  const { t } = useTranslation();
  const [fields, setFields] = useState<Record<string, string>>(initial);

  function set(key: string, value: string) {
    setFields((prev) => ({ ...prev, [key]: value }));
  }

  return (
    <div className="space-y-2 p-3 border rounded bg-muted/20">
      {type === 'text' && (
        <div>
          <label className="block text-xs font-medium mb-1">{t('lessonContent.markdown')}</label>
          <textarea
            rows={6}
            value={fields.markdown ?? ''}
            onChange={(e) => set('markdown', e.target.value)}
            className="w-full border rounded px-2 py-1 text-sm font-mono"
            placeholder="# Heading&#10;&#10;Write your content here..."
          />
        </div>
      )}
      {(type === 'image' || type === 'video' || type === 'pdf' || type === 'youtube') && (
        <div>
          <label className="block text-xs font-medium mb-1">URL</label>
          <input
            type="url"
            value={fields.url ?? ''}
            onChange={(e) => set('url', e.target.value)}
            className="w-full border rounded px-2 py-1 text-sm"
            placeholder={type === 'youtube' ? 'https://youtube.com/watch?v=...' : 'https://...'}
          />
        </div>
      )}
      {type === 'link' && (
        <>
          <div>
            <label className="block text-xs font-medium mb-1">URL</label>
            <input
              type="url"
              value={fields.url ?? ''}
              onChange={(e) => set('url', e.target.value)}
              className="w-full border rounded px-2 py-1 text-sm"
              placeholder="https://..."
            />
          </div>
          <div>
            <label className="block text-xs font-medium mb-1">{t('lessonContent.label')}</label>
            <input
              type="text"
              value={fields.label ?? ''}
              onChange={(e) => set('label', e.target.value)}
              className="w-full border rounded px-2 py-1 text-sm"
              placeholder={t('lessonContent.linkLabel')}
            />
          </div>
        </>
      )}
      <div className="flex gap-2 pt-1">
        <button
          onClick={() => onSave(fields)}
          disabled={isPending}
          className="rounded bg-primary px-3 py-1 text-xs font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          {t('common.save')}
        </button>
        <button onClick={onCancel} className="rounded border px-3 py-1 text-xs font-medium hover:bg-muted">
          {t('common.cancel')}
        </button>
      </div>
    </div>
  );
}

function BlockPreview({ block }: { block: ContentBlock }) {
  const fields = parseContent(block.content);

  switch (block.type) {
    case 'text':
      return <p className="text-sm text-muted-foreground line-clamp-2">{fields.markdown ?? ''}</p>;
    case 'image':
      return <p className="text-sm text-blue-600 truncate">🖼 {fields.url}</p>;
    case 'video':
      return <p className="text-sm text-blue-600 truncate">🎬 {fields.url}</p>;
    case 'pdf':
      return <p className="text-sm text-blue-600 truncate">📄 {fields.url}</p>;
    case 'link':
      return <p className="text-sm text-blue-600 truncate">🔗 {fields.label || fields.url}</p>;
    case 'youtube':
      return <p className="text-sm text-red-600 truncate">▶ {fields.url}</p>;
    default:
      return <p className="text-sm text-muted-foreground">{block.content}</p>;
  }
}

export function LessonContentEditor() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const params = useParams({ strict: false });
  const lessonId = Number(params.lessonId);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [addingType, setAddingType] = useState<ContentBlockType | null>(null);

  const { data: blocks = [], isLoading } = useQuery({
    queryKey: ['content-blocks', lessonId],
    queryFn: () => fetchContentBlocks(lessonId),
  });

  const createMutation = useMutation({
    mutationFn: ({ type, fields }: { type: ContentBlockType; fields: Record<string, string> }) =>
      createContentBlock(lessonId, {
        type,
        content: buildContent(fields),
        order: blocks.length + 1,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['content-blocks', lessonId] });
      setAddingType(null);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ block, fields }: { block: ContentBlock; fields: Record<string, string> }) =>
      updateContentBlock(lessonId, block.id, {
        type: block.type,
        content: buildContent(fields),
        order: block.order,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['content-blocks', lessonId] });
      setEditingId(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (blockId: number) => deleteContentBlock(lessonId, blockId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['content-blocks', lessonId] }),
  });

  const reorderMutation = useMutation({
    mutationFn: (orderedIds: number[]) => reorderContentBlocks(lessonId, orderedIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['content-blocks', lessonId] }),
  });

  function moveBlock(index: number, direction: -1 | 1) {
    const newBlocks = [...blocks];
    const target = index + direction;
    if (target < 0 || target >= newBlocks.length) return;
    [newBlocks[index], newBlocks[target]] = [newBlocks[target], newBlocks[index]];
    reorderMutation.mutate(newBlocks.map((b) => b.id));
  }

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <button onClick={() => window.history.back()} className="text-sm text-muted-foreground hover:text-foreground">
          ← {t('lessonContent.back')}
        </button>
      </div>

      <h1 className="text-3xl font-bold">{t('lessonContent.title')}</h1>

      {/* Existing blocks */}
      <div className="space-y-2">
        {blocks.map((block, index) => (
          <div key={block.id} className="rounded-md border p-3 space-y-2">
            {editingId === block.id ? (
              <BlockForm
                type={block.type}
                initial={parseContent(block.content)}
                onSave={(fields) => updateMutation.mutate({ block, fields })}
                onCancel={() => setEditingId(null)}
                isPending={updateMutation.isPending}
              />
            ) : (
              <div className="flex items-start gap-3">
                <div className="flex flex-col gap-1">
                  <button
                    onClick={() => moveBlock(index, -1)}
                    disabled={index === 0 || reorderMutation.isPending}
                    className="text-xs text-muted-foreground hover:text-foreground disabled:opacity-30"
                  >
                    ▲
                  </button>
                  <button
                    onClick={() => moveBlock(index, 1)}
                    disabled={index === blocks.length - 1 || reorderMutation.isPending}
                    className="text-xs text-muted-foreground hover:text-foreground disabled:opacity-30"
                  >
                    ▼
                  </button>
                </div>
                <div className="flex-1 min-w-0">
                  <span className="text-xs font-medium uppercase text-muted-foreground">{block.type}</span>
                  <BlockPreview block={block} />
                </div>
                <div className="flex gap-2 shrink-0">
                  <button onClick={() => setEditingId(block.id)} className="text-xs underline hover:no-underline">
                    {t('common.edit')}
                  </button>
                  <button
                    onClick={() => deleteMutation.mutate(block.id)}
                    disabled={deleteMutation.isPending}
                    className="text-xs text-destructive underline hover:no-underline"
                  >
                    {t('common.delete')}
                  </button>
                </div>
              </div>
            )}
          </div>
        ))}
        {blocks.length === 0 && <p className="text-sm text-muted-foreground">{t('lessonContent.noBlocks')}</p>}
      </div>

      {/* Add block */}
      {addingType ? (
        <div className="rounded-md border p-4 space-y-3">
          <h2 className="font-semibold text-sm">
            {t('lessonContent.addBlock')}: {addingType}
          </h2>
          <BlockForm
            type={addingType}
            onSave={(fields) => addingType && createMutation.mutate({ type: addingType, fields })}
            onCancel={() => setAddingType(null)}
            isPending={createMutation.isPending}
          />
        </div>
      ) : (
        <div className="rounded-md border p-4 space-y-3">
          <h2 className="font-semibold text-sm">{t('lessonContent.addBlock')}</h2>
          <div className="flex flex-wrap gap-2">
            {BLOCK_TYPES.map((type) => (
              <button
                key={type}
                onClick={() => setAddingType(type)}
                className="rounded border px-3 py-1.5 text-xs font-medium hover:bg-muted capitalize"
              >
                + {type}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
