import { createFileRoute } from '@tanstack/react-router';
import { UserDetail } from '@/pages/UserDetail';

export const Route = createFileRoute('/_authenticated/admin/users/$id')({
  component: UserDetailPage,
});

function UserDetailPage() {
  const { id } = Route.useParams();
  return <UserDetail userId={id} />;
}
