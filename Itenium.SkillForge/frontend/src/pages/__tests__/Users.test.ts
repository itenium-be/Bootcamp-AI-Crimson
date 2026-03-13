import { describe, it, expect } from 'vitest';
import { filterUsers, type User, type UserFilters } from '../Users';

const users: User[] = [
  { id: '1', name: 'Alice Smith', email: 'alice@test.com', role: 'learner', isActive: true, lastActiveAt: null },
  { id: '2', name: 'Bob Jones', email: 'bob@test.com', role: 'team_manager', isActive: true, lastActiveAt: null },
  { id: '3', name: 'Carol Admin', email: 'carol@test.com', role: 'backoffice', isActive: true, lastActiveAt: null },
  { id: '4', name: 'Dave Inactive', email: 'dave@test.com', role: 'learner', isActive: false, lastActiveAt: null },
];

const noFilters: UserFilters = { search: '', role: '', status: '' };

describe('filterUsers', () => {
  it('returns all users when no filters applied', () => {
    expect(filterUsers(users, noFilters)).toHaveLength(4);
  });

  it('filters by name (case-insensitive)', () => {
    const result = filterUsers(users, { ...noFilters, search: 'alice' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('1');
  });

  it('filters by email (case-insensitive)', () => {
    const result = filterUsers(users, { ...noFilters, search: 'BOB@TEST' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('2');
  });

  it('filters by role', () => {
    const result = filterUsers(users, { ...noFilters, role: 'learner' });
    expect(result).toHaveLength(2);
  });

  it('filters active users', () => {
    const result = filterUsers(users, { ...noFilters, status: 'active' });
    expect(result).toHaveLength(3);
  });

  it('filters inactive users', () => {
    const result = filterUsers(users, { ...noFilters, status: 'inactive' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('4');
  });

  it('combines search and role', () => {
    const result = filterUsers(users, { ...noFilters, search: 'test.com', role: 'backoffice' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('3');
  });

  it('combines all three filters', () => {
    const result = filterUsers(users, { search: 'dave', role: 'learner', status: 'inactive' });
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('4');
  });

  it('returns empty array when nothing matches', () => {
    const result = filterUsers(users, { ...noFilters, search: 'nobody' });
    expect(result).toHaveLength(0);
  });
});
