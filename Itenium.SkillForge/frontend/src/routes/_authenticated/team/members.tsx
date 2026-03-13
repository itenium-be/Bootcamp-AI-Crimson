import { createFileRoute } from '@tanstack/react-router';
import { MyTeam } from '@/pages/MyTeam';

export const Route = createFileRoute('/_authenticated/team/members')({
  component: MyTeam,
});
