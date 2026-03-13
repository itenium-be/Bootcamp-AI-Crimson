import { createFileRoute } from '@tanstack/react-router';
import { FeedbackReview } from '@/pages/FeedbackReview';

export const Route = createFileRoute('/_authenticated/reports/feedback')({
  component: FeedbackReview,
});
