import { describe, it, expect } from 'vitest';
import { filterCourses, type Course, type CourseFilters } from '../Courses';

const courses: Course[] = [
  { id: 1, name: 'Introduction to Programming', description: 'Learn the basics', category: 'Development', level: 'Beginner' },
  { id: 2, name: 'Advanced C#', description: 'Deep dive into C#', category: 'Development', level: 'Advanced' },
  { id: 3, name: 'Cloud Architecture', description: 'AWS and Azure patterns', category: 'Architecture', level: 'Intermediate' },
  { id: 4, name: 'Agile Project Management', description: null, category: 'Management', level: 'Beginner' },
];

const noFilters: CourseFilters = { search: '', category: '', level: '' };

describe('filterCourses', () => {
  it('returns all courses when no filters applied', () => {
    expect(filterCourses(courses, noFilters)).toHaveLength(4);
  });

  it('filters by name (case-insensitive)', () => {
    const result = filterCourses(courses, { ...noFilters, search: 'advanced' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(2);
  });

  it('filters by description', () => {
    const result = filterCourses(courses, { ...noFilters, search: 'AWS' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(3);
  });

  it('handles null description without crashing', () => {
    const result = filterCourses(courses, { ...noFilters, search: 'agile' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(4);
  });

  it('filters by category', () => {
    const result = filterCourses(courses, { ...noFilters, category: 'Development' });
    expect(result).toHaveLength(2);
  });

  it('filters by level', () => {
    const result = filterCourses(courses, { ...noFilters, level: 'Beginner' });
    expect(result).toHaveLength(2);
  });

  it('combines search with category filter', () => {
    const result = filterCourses(courses, { ...noFilters, search: 'programming', category: 'Development' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(1);
  });

  it('combines all three filters', () => {
    const result = filterCourses(courses, { search: 'c#', category: 'Development', level: 'Advanced' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(2);
  });

  it('returns empty array when nothing matches', () => {
    const result = filterCourses(courses, { ...noFilters, search: 'kubernetes' });
    expect(result).toHaveLength(0);
  });

  it('is case-insensitive for search', () => {
    const upper = filterCourses(courses, { ...noFilters, search: 'CLOUD' });
    const lower = filterCourses(courses, { ...noFilters, search: 'cloud' });
    expect(upper).toEqual(lower);
  });
});
