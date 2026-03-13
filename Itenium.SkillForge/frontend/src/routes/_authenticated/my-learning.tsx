import { createFileRoute } from '@tanstack/react-router';
import { MyLearning } from '@/pages/MyLearning';

export const Route = createFileRoute('/_authenticated/my-learning')({
  component: MyLearning,
});
