import { createFileRoute } from '@tanstack/react-router';
import { ManagerLessonProgress } from '@/pages/ManagerLessonProgress';

export const Route = createFileRoute('/_authenticated/manager/lesson-progress')({
  component: ManagerLessonProgress,
});
