import { createFileRoute } from '@tanstack/react-router';
import { CourseLessons } from '@/pages/CourseLessons';

export const Route = createFileRoute('/_authenticated/courses/$id')({
  component: CourseLessons,
});
