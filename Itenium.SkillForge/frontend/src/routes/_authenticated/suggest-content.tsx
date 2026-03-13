import { createFileRoute } from '@tanstack/react-router';
import { SuggestContent } from '@/pages/SuggestContent';

export const Route = createFileRoute('/_authenticated/suggest-content')({
  component: SuggestContent,
});
