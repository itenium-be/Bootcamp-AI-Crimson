import { createFileRoute } from '@tanstack/react-router';
import { CourseLessonsManager } from '@/pages/CourseLessonsManager';

export const Route = createFileRoute('/_authenticated/manager/courses/$id/lessons/')({
  component: CourseLessonsManagerPage,
});

function CourseLessonsManagerPage() {
  const { id } = Route.useParams();
  return <CourseLessonsManager courseId={Number(id)} />;
}
