import { createFileRoute } from '@tanstack/react-router';
import { QuizAnalytics } from '@/pages/QuizAnalytics';

export const Route = createFileRoute('/_authenticated/manager/quizzes/$id/analytics')({
  component: QuizAnalytics,
});
