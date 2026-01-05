import { createFileRoute } from '@tanstack/react-router';
import { Courses } from '@/pages/Courses';

export const Route = createFileRoute('/_authenticated/courses')({
  component: Courses,
});
