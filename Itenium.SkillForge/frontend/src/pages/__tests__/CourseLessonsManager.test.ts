import { describe, it, expect } from 'vitest';
import { sortLessons, filterLessonsByTitle, getNextSortOrder, type LessonItem } from '../CourseLessonsManager';

const lessons: LessonItem[] = [
  { id: 1, title: 'Introduction', estimatedDuration: 10, sortOrder: 1 },
  { id: 2, title: 'Advanced Topics', estimatedDuration: 30, sortOrder: 2 },
  { id: 3, title: 'Summary', estimatedDuration: null, sortOrder: 3 },
];

describe('sortLessons', () => {
  it('sorts lessons by sortOrder ascending', () => {
    const unordered: LessonItem[] = [
      { id: 3, title: 'Summary', estimatedDuration: null, sortOrder: 3 },
      { id: 1, title: 'Introduction', estimatedDuration: 10, sortOrder: 1 },
      { id: 2, title: 'Advanced Topics', estimatedDuration: 30, sortOrder: 2 },
    ];
    const sorted = sortLessons(unordered);
    expect(sorted[0].id).toBe(1);
    expect(sorted[1].id).toBe(2);
    expect(sorted[2].id).toBe(3);
  });

  it('does not mutate original array', () => {
    const original = [...lessons];
    sortLessons(lessons);
    expect(lessons).toEqual(original);
  });
});

describe('filterLessonsByTitle', () => {
  it('returns all lessons when search is empty', () => {
    expect(filterLessonsByTitle(lessons, '')).toHaveLength(3);
  });

  it('filters by title case-insensitively', () => {
    const result = filterLessonsByTitle(lessons, 'advanced');
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(2);
  });

  it('returns empty array when no match', () => {
    const result = filterLessonsByTitle(lessons, 'nonexistent');
    expect(result).toHaveLength(0);
  });

  it('matches partial title', () => {
    const result = filterLessonsByTitle(lessons, 'intro');
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(1);
  });
});

describe('getNextSortOrder', () => {
  it('returns 1 when no lessons exist', () => {
    expect(getNextSortOrder([])).toBe(1);
  });

  it('returns max sortOrder + 1', () => {
    expect(getNextSortOrder(lessons)).toBe(4);
  });

  it('handles unsorted lessons', () => {
    const unordered: LessonItem[] = [
      { id: 3, title: 'C', estimatedDuration: null, sortOrder: 5 },
      { id: 1, title: 'A', estimatedDuration: null, sortOrder: 2 },
    ];
    expect(getNextSortOrder(unordered)).toBe(6);
  });
});
