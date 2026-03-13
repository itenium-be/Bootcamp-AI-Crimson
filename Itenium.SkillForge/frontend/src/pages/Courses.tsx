import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchCourses, fetchMyEnrollments, enrollCourse } from '@/api/client';
import { useTeamStore } from '@/stores';
import { AssignCourseModal } from '@/components/AssignCourseModal';

export interface Course {
  id: number;
  name: string;
  description: string | null;
  category: string | null;
  level: string | null;
}

export interface CourseFilters {
  search: string;
  category: string;
  level: string;
}

export function filterCourses(courses: Course[], filters: CourseFilters): Course[] {
  const search = filters.search.toLowerCase().trim();
  return courses.filter((course) => {
    if (search) {
      const inName = course.name.toLowerCase().includes(search);
      const inDescription = (course.description ?? '').toLowerCase().includes(search);
      if (!inName && !inDescription) return false;
    }
    if (filters.category && course.category !== filters.category) return false;
    if (filters.level && course.level !== filters.level) return false;
    return true;
  });
}

export function Courses() {
  const { t } = useTranslation();
  const { mode } = useTeamStore();
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<CourseFilters>({ search: '', category: '', level: '' });
  const [assigningCourse, setAssigningCourse] = useState<{ id: number; name: string } | null>(null);

  const { data: courses = [], isLoading } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const { data: enrollments = [] } = useQuery({
    queryKey: ['enrollments', 'me'],
    queryFn: fetchMyEnrollments,
  });

  const enrolledCourseIds = useMemo(() => new Set(enrollments.map((e) => e.courseId)), [enrollments]);

  const enrollMutation = useMutation({
    mutationFn: enrollCourse,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['enrollments', 'me'] }),
  });

  const categories = useMemo(
    () => [...new Set(courses.map((c) => c.category).filter(Boolean) as string[])].sort(),
    [courses],
  );

  const levels = useMemo(() => [...new Set(courses.map((c) => c.level).filter(Boolean) as string[])].sort(), [courses]);

  const filtered = useMemo(() => filterCourses(courses, filters), [courses, filters]);

  const hasActiveFilters = filters.search !== '' || filters.category !== '' || filters.level !== '';

  function clearFilters() {
    setFilters({ search: '', category: '', level: '' });
  }

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('courses.title')}</h1>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <input
          type="text"
          placeholder={t('courses.searchPlaceholder')}
          value={filters.search}
          onChange={(e) => setFilters((f) => ({ ...f, search: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring w-72"
        />

        <select
          value={filters.category}
          onChange={(e) => setFilters((f) => ({ ...f, category: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('courses.allCategories')}</option>
          {categories.map((cat) => (
            <option key={cat} value={cat}>
              {cat}
            </option>
          ))}
        </select>

        <select
          value={filters.level}
          onChange={(e) => setFilters((f) => ({ ...f, level: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('courses.allLevels')}</option>
          {levels.map((lvl) => (
            <option key={lvl} value={lvl}>
              {lvl}
            </option>
          ))}
        </select>

        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="h-9 rounded-md border px-3 text-sm text-muted-foreground hover:bg-muted"
          >
            {t('courses.clearFilters')}
          </button>
        )}

        <span className="text-sm text-muted-foreground ml-auto">
          {filtered.length} / {courses.length}
        </span>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('courses.name')}</th>
              <th className="p-3 text-left font-medium">{t('courses.description')}</th>
              <th className="p-3 text-left font-medium">{t('courses.category')}</th>
              <th className="p-3 text-left font-medium">{t('courses.level')}</th>
              <th className="p-3 text-left font-medium">{mode === 'manager' ? t('courses.actions') : ''}</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((course) => {
              const isEnrolled = enrolledCourseIds.has(course.id);
              return (
                <tr key={course.id} className="border-b">
                  <td className="p-3">{course.name}</td>
                  <td className="p-3 text-muted-foreground">{course.description || '-'}</td>
                  <td className="p-3">{course.category || '-'}</td>
                  <td className="p-3">{course.level || '-'}</td>
                  <td className="p-3 text-right">
                    {mode === 'manager' ? (
                      <button
                        onClick={() => setAssigningCourse({ id: course.id, name: course.name })}
                        className="h-8 rounded-md bg-primary px-3 text-xs font-medium text-primary-foreground hover:bg-primary/90"
                      >
                        {t('assignments.assign')}
                      </button>
                    ) : (
                      <button
                        onClick={() => !isEnrolled && enrollMutation.mutate(course.id)}
                        disabled={isEnrolled || enrollMutation.isPending}
                        className={`rounded-md px-3 py-1 text-sm font-medium transition-colors ${
                          isEnrolled
                            ? 'bg-green-100 text-green-800 cursor-default'
                            : 'bg-primary text-primary-foreground hover:bg-primary/90'
                        }`}
                      >
                        {isEnrolled ? t('courses.enrolled') : t('courses.enroll')}
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
            {filtered.length === 0 && (
              <tr>
                <td colSpan={5} className="p-3 text-center text-muted-foreground">
                  {hasActiveFilters ? t('common.noResults') : t('courses.noCourses')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {assigningCourse && (
        <AssignCourseModal
          courseId={assigningCourse.id}
          courseName={assigningCourse.name}
          onClose={() => setAssigningCourse(null)}
        />
      )}
    </div>
  );
}
