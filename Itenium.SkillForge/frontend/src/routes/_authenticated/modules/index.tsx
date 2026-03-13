import { createFileRoute } from '@tanstack/react-router';
import { LearnerModules } from '@/pages/LearnerModules';

export const Route = createFileRoute('/_authenticated/modules/')({
  component: LearnerModules,
});
