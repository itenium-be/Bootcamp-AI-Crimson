import { createFileRoute } from '@tanstack/react-router';
import { LessonContentEditor } from '@/pages/LessonContentEditor';

export const Route = createFileRoute('/_authenticated/manager/lessons/$lessonId')({
  component: LessonContentEditor,
});
