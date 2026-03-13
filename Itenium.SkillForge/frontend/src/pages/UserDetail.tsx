import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from '@tanstack/react-router';
import { fetchUser, changeUserRole, deactivateUser, activateUser } from '@/api/client';

const ROLES = ['learner', 'team_manager', 'backoffice'];

interface UserDetailProps {
  userId: string;
}

export function UserDetail({ userId }: UserDetailProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [selectedRole, setSelectedRole] = useState('');
  const [confirmAction, setConfirmAction] = useState<'deactivate' | 'activate' | null>(null);

  const { data: user, isLoading } = useQuery({
    queryKey: ['users', userId],
    queryFn: () => fetchUser(userId),
  });

  const roleMutation = useMutation({
    mutationFn: (role: string) => changeUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users', userId] });
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: () => deactivateUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users', userId] });
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setConfirmAction(null);
    },
  });

  const activateMutation = useMutation({
    mutationFn: () => activateUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users', userId] });
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setConfirmAction(null);
    },
  });

  if (isLoading) return <div>{t('common.loading')}</div>;
  if (!user) return <div>{t('common.noResults')}</div>;

  const currentRole = selectedRole || user.role;

  function handleRoleSave() {
    if (selectedRole && user && selectedRole !== user.role) {
      roleMutation.mutate(selectedRole);
    }
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate({ to: '/admin/users' })}
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← {t('userDetail.backToUsers')}
        </button>
      </div>

      <div>
        <h1 className="text-3xl font-bold">{user.name || user.email}</h1>
        <p className="text-muted-foreground mt-1">{user.email}</p>
      </div>

      <div className="rounded-md border p-4 space-y-4">
        <div className="flex items-center gap-2">
          <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${
            user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
          }`}>
            {user.isActive ? t('common.active') : t('users.inactive')}
          </span>
          {user.lastActiveAt && (
            <span className="text-sm text-muted-foreground">
              {t('users.lastActive')}: {new Date(user.lastActiveAt).toLocaleDateString()}
            </span>
          )}
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium">{t('userDetail.changeRole')}</label>
          <div className="flex gap-2">
            <select
              value={currentRole}
              onChange={(e) => setSelectedRole(e.target.value)}
              className="h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            >
              {ROLES.map((role) => (
                <option key={role} value={role}>{t(`users.role.${role}`)}</option>
              ))}
            </select>
            <button
              onClick={handleRoleSave}
              disabled={!selectedRole || selectedRole === user.role || roleMutation.isPending}
              className="h-9 rounded-md bg-primary text-primary-foreground px-3 text-sm font-medium hover:bg-primary/90 disabled:opacity-50"
            >
              {t('common.save')}
            </button>
          </div>
        </div>

        <div className="pt-2 border-t">
          {user.isActive ? (
            <button
              onClick={() => setConfirmAction('deactivate')}
              className="rounded-md border border-red-300 px-3 py-1.5 text-sm text-red-700 hover:bg-red-50"
            >
              {t('userDetail.deactivate')}
            </button>
          ) : (
            <button
              onClick={() => setConfirmAction('activate')}
              className="rounded-md border border-green-300 px-3 py-1.5 text-sm text-green-700 hover:bg-green-50"
            >
              {t('userDetail.activate')}
            </button>
          )}
        </div>
      </div>

      {confirmAction && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-background rounded-md border p-6 max-w-sm w-full space-y-4">
            <p className="text-sm">
              {confirmAction === 'deactivate'
                ? t('userDetail.confirmDeactivate', { name: user.name || user.email })
                : t('userDetail.confirmActivate', { name: user.name || user.email })}
            </p>
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => setConfirmAction(null)}
                className="h-9 rounded-md border px-3 text-sm text-muted-foreground hover:bg-muted"
              >
                {t('common.cancel')}
              </button>
              <button
                onClick={() => confirmAction === 'deactivate' ? deactivateMutation.mutate() : activateMutation.mutate()}
                disabled={deactivateMutation.isPending || activateMutation.isPending}
                className={`h-9 rounded-md px-3 text-sm font-medium ${
                  confirmAction === 'deactivate'
                    ? 'bg-red-600 text-white hover:bg-red-700'
                    : 'bg-green-600 text-white hover:bg-green-700'
                } disabled:opacity-50`}
              >
                {t('common.save')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
