import { createFileRoute } from '@tanstack/react-router';
import { ManagerCourseForm } from '@/pages/ManagerCourseForm';

export const Route = createFileRoute('/_authenticated/manager/courses/$id')({
  component: ManagerCourseForm,
});
