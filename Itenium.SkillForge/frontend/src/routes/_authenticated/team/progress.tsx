import { createFileRoute } from '@tanstack/react-router';
import { TeamProgress } from '@/pages/TeamProgress';

export const Route = createFileRoute('/_authenticated/team/progress')({
  component: TeamProgress,
});
