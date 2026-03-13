import { createFileRoute } from '@tanstack/react-router';
import { LearnerModuleDetail } from '@/pages/LearnerModuleDetail';

export const Route = createFileRoute('/_authenticated/modules/$id')({
  component: LearnerModuleDetail,
});
