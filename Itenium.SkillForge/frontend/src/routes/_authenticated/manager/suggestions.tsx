import { createFileRoute } from '@tanstack/react-router';
import { ManagerSuggestions } from '@/pages/ManagerSuggestions';

export const Route = createFileRoute('/_authenticated/manager/suggestions')({
  component: ManagerSuggestions,
});
