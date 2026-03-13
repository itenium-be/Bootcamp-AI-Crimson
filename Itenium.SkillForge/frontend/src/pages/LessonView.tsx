import { useTranslation } from 'react-i18next';
import { useParams } from '@tanstack/react-router';
import { useQuery } from '@tanstack/react-query';
import { fetchContentBlocks, type ContentBlock } from '@/api/client';

function parseContent(content: string): Record<string, string> {
  try {
    return JSON.parse(content) as Record<string, string>;
  } catch {
    return {};
  }
}

function getYouTubeEmbedUrl(url: string): string | null {
  try {
    const u = new URL(url);
    const videoId = u.searchParams.get('v') ?? u.pathname.split('/').pop();
    return videoId ? `https://www.youtube.com/embed/${videoId}` : null;
  } catch {
    return null;
  }
}

function ContentBlockView({ block }: { block: ContentBlock }) {
  const fields = parseContent(block.content);

  switch (block.type) {
    case 'text':
      return (
        <div className="prose prose-sm max-w-none">
          <pre className="whitespace-pre-wrap font-sans text-sm">{fields.markdown}</pre>
        </div>
      );
    case 'image':
      return (
        <div>
          <img src={fields.url} alt="" className="max-w-full rounded-md" />
        </div>
      );
    case 'video':
      return (
        <div>
          <video src={fields.url} controls className="w-full rounded-md" />
        </div>
      );
    case 'pdf':
      return (
        <div className="border rounded-md overflow-hidden">
          <iframe src={fields.url} className="w-full h-[600px]" title="PDF" />
        </div>
      );
    case 'link':
      return (
        <a
          href={fields.url}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 rounded border px-3 py-2 text-sm text-primary hover:bg-muted"
        >
          🔗 {fields.label || fields.url}
        </a>
      );
    case 'youtube': {
      const embedUrl = getYouTubeEmbedUrl(fields.url ?? '');
      return embedUrl ? (
        <div className="aspect-video">
          <iframe
            src={embedUrl}
            className="w-full h-full rounded-md"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
            title="YouTube video"
          />
        </div>
      ) : (
        <a href={fields.url} target="_blank" rel="noopener noreferrer" className="text-sm text-primary underline">
          {fields.url}
        </a>
      );
    }
    default:
      return null;
  }
}

export function LessonView() {
  const { t } = useTranslation();

  const params = useParams({ strict: false });
  const lessonId = Number(params.lessonId);

  const { data: blocks = [], isLoading } = useQuery({
    queryKey: ['content-blocks', lessonId],
    queryFn: () => fetchContentBlocks(lessonId),
  });

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6 max-w-3xl">
      <button onClick={() => window.history.back()} className="text-sm text-muted-foreground hover:text-foreground">
        ← {t('lessonContent.backToLessons')}
      </button>

      {blocks.length === 0 ? (
        <p className="text-muted-foreground text-sm">{t('lessonContent.noBlocks')}</p>
      ) : (
        <div className="space-y-6">
          {blocks.map((block) => (
            <div key={block.id}>
              <ContentBlockView block={block} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
