import { createFileRoute } from '@tanstack/react-router';
import { ManagerCourses } from '@/pages/ManagerCourses';

export const Route = createFileRoute('/_authenticated/manager/courses/')({
  component: ManagerCourses,
});
