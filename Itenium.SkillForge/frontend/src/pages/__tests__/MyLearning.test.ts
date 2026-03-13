import { describe, it, expect } from 'vitest';
import { filterEnrollments, type Enrollment, type EnrollmentFilters } from '../MyLearning';

const enrollments: Enrollment[] = [
  {
    id: 1,
    courseId: 1,
    courseName: 'Intro to Programming',
    courseCategory: 'Development',
    courseLevel: 'Beginner',
    enrolledAt: '2026-01-01',
    status: 'Active',
    completedAt: null,
    moduleName: null,
  },
  {
    id: 2,
    courseId: 2,
    courseName: 'Advanced C#',
    courseCategory: 'Development',
    courseLevel: 'Advanced',
    enrolledAt: '2026-01-05',
    status: 'Active',
    completedAt: null,
    moduleName: 'Backend Development',
  },
  {
    id: 3,
    courseId: 3,
    courseName: 'Cloud Architecture',
    courseCategory: 'Architecture',
    courseLevel: 'Intermediate',
    enrolledAt: '2026-01-10',
    status: 'Completed',
    completedAt: '2026-02-15',
    moduleName: 'Cloud Path',
  },
  {
    id: 4,
    courseId: 4,
    courseName: 'Agile Management',
    courseCategory: 'Management',
    courseLevel: 'Beginner',
    enrolledAt: '2026-01-15',
    status: 'Completed',
    completedAt: '2026-03-01',
    moduleName: null,
  },
];

const noFilters: EnrollmentFilters = { search: '', status: '' };

describe('filterEnrollments', () => {
  it('returns all enrollments when no filters applied', () => {
    expect(filterEnrollments(enrollments, noFilters)).toHaveLength(4);
  });

  it('filters by course name (case-insensitive)', () => {
    const result = filterEnrollments(enrollments, { ...noFilters, search: 'advanced' });
    expect(result).toHaveLength(1);
    expect(result[0].courseId).toBe(2);
  });

  it('filters by category', () => {
    const result = filterEnrollments(enrollments, { ...noFilters, search: 'Architecture' });
    expect(result).toHaveLength(1);
    expect(result[0].courseId).toBe(3);
  });

  it('filters active enrollments', () => {
    const result = filterEnrollments(enrollments, { ...noFilters, status: 'Active' });
    expect(result).toHaveLength(2);
  });

  it('filters completed enrollments', () => {
    const result = filterEnrollments(enrollments, { ...noFilters, status: 'Completed' });
    expect(result).toHaveLength(2);
  });

  it('combines search and status', () => {
    const result = filterEnrollments(enrollments, { search: 'c#', status: 'Active' });
    expect(result).toHaveLength(1);
    expect(result[0].courseId).toBe(2);
  });

  it('returns empty when nothing matches', () => {
    const result = filterEnrollments(enrollments, { ...noFilters, search: 'kubernetes' });
    expect(result).toHaveLength(0);
  });
});
