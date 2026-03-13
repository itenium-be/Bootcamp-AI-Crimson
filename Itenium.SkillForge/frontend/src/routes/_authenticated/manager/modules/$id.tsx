import { createFileRoute } from '@tanstack/react-router';
import { ManagerModuleDetail } from '@/pages/ManagerModuleDetail';

export const Route = createFileRoute('/_authenticated/manager/modules/$id')({
  component: ManagerModuleDetail,
});
