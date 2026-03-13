import { createFileRoute } from '@tanstack/react-router';
import { Teams } from '@/pages/Teams';

export const Route = createFileRoute('/_authenticated/admin/teams')({
  component: Teams,
});
