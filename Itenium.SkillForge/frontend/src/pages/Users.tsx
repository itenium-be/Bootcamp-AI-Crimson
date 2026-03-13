import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchUsers } from '@/api/client';

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  lastActiveAt: string | null;
}

export interface UserFilters {
  search: string;
  role: string;
  status: string;
}

export function filterUsers(users: User[], filters: UserFilters): User[] {
  const search = filters.search.toLowerCase().trim();
  return users.filter((user) => {
    if (search) {
      const inName = user.name.toLowerCase().includes(search);
      const inEmail = user.email.toLowerCase().includes(search);
      if (!inName && !inEmail) return false;
    }
    if (filters.role && user.role !== filters.role) return false;
    if (filters.status === 'active' && !user.isActive) return false;
    if (filters.status === 'inactive' && user.isActive) return false;
    return true;
  });
}

const ROLES = ['learner', 'team_manager', 'backoffice'];

export function Users() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [filters, setFilters] = useState<UserFilters>({ search: '', role: '', status: '' });

  const { data: users = [], isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: fetchUsers,
  });

  const filtered = useMemo(() => filterUsers(users, filters), [users, filters]);
  const hasActiveFilters = filters.search !== '' || filters.role !== '' || filters.status !== '';

  function clearFilters() {
    setFilters({ search: '', role: '', status: '' });
  }

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('users.title')}</h1>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <input
          type="text"
          placeholder={t('users.searchPlaceholder')}
          value={filters.search}
          onChange={(e) => setFilters((f) => ({ ...f, search: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring w-72"
        />

        <select
          value={filters.role}
          onChange={(e) => setFilters((f) => ({ ...f, role: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('users.allRoles')}</option>
          {ROLES.map((role) => (
            <option key={role} value={role}>
              {t(`users.role.${role}`)}
            </option>
          ))}
        </select>

        <select
          value={filters.status}
          onChange={(e) => setFilters((f) => ({ ...f, status: e.target.value }))}
          className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('users.allStatuses')}</option>
          <option value="active">{t('common.active')}</option>
          <option value="inactive">{t('users.inactive')}</option>
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
          {filtered.length} / {users.length}
        </span>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('users.name')}</th>
              <th className="p-3 text-left font-medium">{t('users.email')}</th>
              <th className="p-3 text-left font-medium">{t('users.role')}</th>
              <th className="p-3 text-left font-medium">{t('users.status')}</th>
              <th className="p-3 text-left font-medium">{t('users.lastActive')}</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((user) => (
              <tr
                key={user.id}
                className="border-b cursor-pointer hover:bg-muted/50"
                onClick={() => navigate({ to: '/admin/users/$id', params: { id: user.id } })}
              >
                <td className="p-3 font-medium">{user.name || '-'}</td>
                <td className="p-3 text-muted-foreground">{user.email}</td>
                <td className="p-3">
                  <span className="rounded-full px-2 py-0.5 text-xs font-medium bg-muted">
                    {t(`users.role.${user.role}`)}
                  </span>
                </td>
                <td className="p-3">
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs font-medium ${user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}
                  >
                    {user.isActive ? t('common.active') : t('users.inactive')}
                  </span>
                </td>
                <td className="p-3 text-muted-foreground">
                  {user.lastActiveAt ? new Date(user.lastActiveAt).toLocaleDateString() : '-'}
                </td>
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr>
                <td colSpan={5} className="p-3 text-center text-muted-foreground">
                  {hasActiveFilters ? t('common.noResults') : t('users.noUsers')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
