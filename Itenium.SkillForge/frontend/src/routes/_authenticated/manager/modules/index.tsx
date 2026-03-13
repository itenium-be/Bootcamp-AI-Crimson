import { createFileRoute } from '@tanstack/react-router';
import { ManagerModules } from '@/pages/ManagerModules';

export const Route = createFileRoute('/_authenticated/manager/modules/')({
  component: ManagerModules,
});
