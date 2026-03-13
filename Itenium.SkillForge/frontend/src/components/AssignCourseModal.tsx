import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { X, Trash2 } from 'lucide-react';
import {
  fetchCourseAssignments,
  createCourseAssignment,
  deleteCourseAssignment,
  fetchUserTeams,
  fetchUsers,
  type AssigneeType,
  type AssignmentType,
  type CourseAssignment,
} from '@/api/client';
import { useAuthStore } from '@/stores';

interface Props {
  courseId: number;
  courseName: string;
  onClose: () => void;
}

export function AssignCourseModal({ courseId, courseName, onClose }: Props) {
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const queryClient = useQueryClient();

  const [assigneeType, setAssigneeType] = useState<AssigneeType>('Team');
  const [assigneeId, setAssigneeId] = useState('');
  const [assigneeName, setAssigneeName] = useState('');
  const [assignmentType, setAssignmentType] = useState<AssignmentType>('Mandatory');

  const { data: assignments = [], isLoading } = useQuery({
    queryKey: ['course-assignments', courseId],
    queryFn: () => fetchCourseAssignments(courseId),
  });

  const { data: teams = [] } = useQuery({
    queryKey: ['teams'],
    queryFn: fetchUserTeams,
  });

  const { data: users = [] } = useQuery({
    queryKey: ['users'],
    queryFn: fetchUsers,
  });

  const createMutation = useMutation({
    mutationFn: () =>
      createCourseAssignment(courseId, {
        assigneeType,
        assigneeId,
        assigneeName: assigneeName || null,
        type: assignmentType,
        assignedBy: user?.name ?? 'unknown',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['course-assignments', courseId] });
      setAssigneeId('');
      setAssigneeName('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (assignmentId: number) => deleteCourseAssignment(courseId, assignmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['course-assignments', courseId] });
    },
  });

  function handleAssigneeChange(id: string) {
    setAssigneeId(id);
    if (assigneeType === 'Team') {
      const team = teams.find((t) => String(t.id) === id);
      setAssigneeName(team?.name ?? '');
    } else {
      const u = users.find((u) => u.id === id);
      setAssigneeName(u?.name ?? '');
    }
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!assigneeId) return;
    createMutation.mutate();
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-card text-card-foreground rounded-xl shadow-lg w-full max-w-lg mx-4">
        <div className="flex items-center justify-between p-6 border-b">
          <div>
            <h2 className="text-lg font-semibold">{t('assignments.title')}</h2>
            <p className="text-sm text-muted-foreground">{courseName}</p>
          </div>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground">
            <X className="size-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-sm font-medium mb-1 block">{t('assignments.assignTo')}</label>
              <select
                value={assigneeType}
                onChange={(e) => {
                  setAssigneeType(e.target.value as AssigneeType);
                  setAssigneeId('');
                  setAssigneeName('');
                }}
                className="w-full h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="Team">{t('assignments.team')}</option>
                <option value="User">{t('assignments.individual')}</option>
              </select>
            </div>
            <div>
              <label className="text-sm font-medium mb-1 block">{t('assignments.type')}</label>
              <select
                value={assignmentType}
                onChange={(e) => setAssignmentType(e.target.value as AssignmentType)}
                className="w-full h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="Mandatory">{t('assignments.mandatory')}</option>
                <option value="Optional">{t('assignments.optional')}</option>
              </select>
            </div>
          </div>

          <div>
            <label className="text-sm font-medium mb-1 block">
              {assigneeType === 'Team' ? t('assignments.selectTeam') : t('assignments.selectLearner')}
            </label>
            <select
              value={assigneeId}
              onChange={(e) => handleAssigneeChange(e.target.value)}
              className="w-full h-9 rounded-md border bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              required
            >
              <option value="">{t('assignments.selectPlaceholder')}</option>
              {assigneeType === 'Team'
                ? teams.map((team) => (
                    <option key={team.id} value={String(team.id)}>
                      {team.name}
                    </option>
                  ))
                : users.map((u) => (
                    <option key={u.id} value={u.id}>
                      {u.name}
                    </option>
                  ))}
            </select>
          </div>

          <button
            type="submit"
            disabled={!assigneeId || createMutation.isPending}
            className="w-full h-9 rounded-md bg-primary text-primary-foreground text-sm font-medium disabled:opacity-50"
          >
            {createMutation.isPending ? t('common.loading') : t('assignments.assign')}
          </button>
          {createMutation.isError && <p className="text-sm text-destructive">{t('assignments.alreadyAssigned')}</p>}
        </form>

        <div className="px-6 pb-6">
          <h3 className="text-sm font-medium mb-3">{t('assignments.existing')}</h3>
          {isLoading ? (
            <p className="text-sm text-muted-foreground">{t('common.loading')}</p>
          ) : assignments.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t('assignments.none')}</p>
          ) : (
            <ul className="space-y-2">
              {assignments.map((a: CourseAssignment) => (
                <li key={a.id} className="flex items-center justify-between rounded-md border px-3 py-2 text-sm">
                  <div className="flex items-center gap-2">
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                        a.type === 'Mandatory'
                          ? 'bg-destructive/10 text-destructive'
                          : 'bg-accent/10 text-accent-foreground'
                      }`}
                    >
                      {a.type === 'Mandatory' ? t('assignments.mandatory') : t('assignments.optional')}
                    </span>
                    <span>{a.assigneeName ?? a.assigneeId}</span>
                    <span className="text-muted-foreground">
                      ({a.assigneeType === 'Team' ? t('assignments.team') : t('assignments.individual')})
                    </span>
                  </div>
                  <button
                    onClick={() => deleteMutation.mutate(a.id)}
                    disabled={deleteMutation.isPending}
                    className="text-muted-foreground hover:text-destructive"
                  >
                    <Trash2 className="size-4" />
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}
