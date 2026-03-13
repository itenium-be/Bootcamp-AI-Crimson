import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { fetchCourses, publishCourse, archiveCourse, deleteCourse } from '@/api/client';
import type { Course, CourseStatus } from '@/api/client';

const STATUS_BADGE: Record<CourseStatus, string> = {
  Draft: 'bg-yellow-100 text-yellow-800',
  Published: 'bg-green-100 text-green-800',
  Archived: 'bg-gray-100 text-gray-600',
};

export function ManagerCourses() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: courses, isLoading } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const publishMutation = useMutation({
    mutationFn: (id: number) => publishCourse(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courses'] }),
  });

  const archiveMutation = useMutation({
    mutationFn: (id: number) => archiveCourse(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courses'] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteCourse(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courses'] }),
  });

  function handlePublish(course: Course) {
    if (window.confirm(t('managerCourses.confirmPublish', { name: course.name }))) {
      publishMutation.mutate(course.id);
    }
  }

  function handleArchive(course: Course) {
    if (window.confirm(t('managerCourses.confirmArchive', { name: course.name }))) {
      archiveMutation.mutate(course.id);
    }
  }

  function handleDelete(course: Course) {
    if (window.confirm(t('managerCourses.confirmDelete', { name: course.name }))) {
      deleteMutation.mutate(course.id);
    }
  }

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('managerCourses.title')}</h1>
        <Link
          to="/manager/courses/$id"
          params={{ id: 'new' }}
          className="rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          {t('managerCourses.newCourse')}
        </Link>
      </div>

      <div className="rounded-md border">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('courses.name')}</th>
              <th className="p-3 text-left font-medium">{t('courses.category')}</th>
              <th className="p-3 text-left font-medium">{t('courses.level')}</th>
              <th className="p-3 text-left font-medium">{t('managerCourses.duration')}</th>
              <th className="p-3 text-left font-medium">{t('courses.status')}</th>
              <th className="p-3 text-left font-medium">{t('courses.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {courses?.map((course) => (
              <tr key={course.id} className="border-b">
                <td className="p-3 font-medium">{course.name}</td>
                <td className="p-3 text-muted-foreground">{course.category ?? '-'}</td>
                <td className="p-3">{course.level ?? '-'}</td>
                <td className="p-3">{course.estimatedDuration ? `${course.estimatedDuration} min` : '-'}</td>
                <td className="p-3">
                  <span className={`rounded px-2 py-0.5 text-xs font-medium ${STATUS_BADGE[course.status]}`}>
                    {t(`managerCourses.status.${course.status.toLowerCase()}`)}
                  </span>
                </td>
                <td className="p-3">
                  <div className="flex gap-2">
                    <Link
                      to="/manager/courses/$id"
                      params={{ id: String(course.id) }}
                      className="text-xs underline hover:no-underline"
                    >
                      {t('common.edit')}
                    </Link>
                    {course.status !== 'Published' && (
                      <button
                        onClick={() => handlePublish(course)}
                        className="text-xs text-green-700 underline hover:no-underline"
                      >
                        {t('managerCourses.publish')}
                      </button>
                    )}
                    {course.status !== 'Archived' && (
                      <button
                        onClick={() => handleArchive(course)}
                        className="text-xs text-orange-600 underline hover:no-underline"
                      >
                        {t('managerCourses.archive')}
                      </button>
                    )}
                    {course.status === 'Draft' && (
                      <button
                        onClick={() => handleDelete(course)}
                        className="text-xs text-destructive underline hover:no-underline"
                      >
                        {t('common.delete')}
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {courses?.length === 0 && (
              <tr>
                <td colSpan={6} className="p-3 text-center text-muted-foreground">
                  {t('courses.noCourses')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
