import { createFileRoute } from '@tanstack/react-router';
import { TeamDetail } from '@/pages/TeamDetail';

export const Route = createFileRoute('/_authenticated/admin/teams/$id')({
  component: TeamDetailPage,
});

function TeamDetailPage() {
  const { id } = Route.useParams();
  return <TeamDetail teamId={Number(id)} />;
}
