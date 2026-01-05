import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchCourses } from '@/api/client';

export function Courses() {
  const { t } = useTranslation();

  const { data: courses, isLoading } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('courses.title')}</h1>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('courses.name')}</th>
              <th className="p-3 text-left font-medium">{t('courses.description')}</th>
              <th className="p-3 text-left font-medium">{t('courses.category')}</th>
              <th className="p-3 text-left font-medium">{t('courses.level')}</th>
            </tr>
          </thead>
          <tbody>
            {courses?.map((course) => (
              <tr key={course.id} className="border-b">
                <td className="p-3">{course.name}</td>
                <td className="p-3 text-muted-foreground">{course.description || '-'}</td>
                <td className="p-3">{course.category || '-'}</td>
                <td className="p-3">{course.level || '-'}</td>
              </tr>
            ))}
            {courses?.length === 0 && (
              <tr>
                <td colSpan={4} className="p-3 text-center text-muted-foreground">
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
