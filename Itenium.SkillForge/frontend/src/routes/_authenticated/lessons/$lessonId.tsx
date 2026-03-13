import { createFileRoute } from '@tanstack/react-router';
import { LessonView } from '@/pages/LessonView';

export const Route = createFileRoute('/_authenticated/lessons/$lessonId')({
  component: LessonView,
});
