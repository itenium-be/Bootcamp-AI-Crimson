import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchTeamProgress, fetchCourseProgress, fetchCourses } from '@/api/client';
import { useTeamStore } from '@/stores';
import type { TeamMemberProgress, CourseMemberItem, CourseMemberProgress, TeamProgressData } from '@/api/client';

export function TeamProgress() {
  const { t } = useTranslation();
  const { teams, selectedTeam } = useTeamStore();
  const [activeTeam, setActiveTeam] = useState(selectedTeam ?? teams[0] ?? null);
  const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null);
  const [filterMember, setFilterMember] = useState('');

  const { data: courses } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const { data: progress, isLoading: progressLoading } = useQuery({
    queryKey: ['team-progress', activeTeam?.id],
    queryFn: () => fetchTeamProgress(activeTeam?.id ?? 0),
    enabled: activeTeam != null,
  });

  const { data: courseProgress, isLoading: courseProgressLoading } = useQuery({
    queryKey: ['course-progress', activeTeam?.id, selectedCourseId],
    queryFn: () => fetchCourseProgress(activeTeam?.id ?? 0, selectedCourseId ?? 0),
    enabled: activeTeam != null && selectedCourseId != null,
  });

  const filteredMembers: TeamProgressData['members'] = (progress?.members ?? []).filter(
    (m) => filterMember === '' || m.userName.toLowerCase().includes(filterMember.toLowerCase()),
  );

  const filteredCourseMembers: CourseMemberProgress['members'] = (courseProgress?.members ?? []).filter(
    (m) => filterMember === '' || m.userName.toLowerCase().includes(filterMember.toLowerCase()),
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('teamProgress.title')}</h1>
        <p className="text-muted-foreground">{t('teamProgress.subtitle')}</p>
      </div>

      {teams.length > 1 && (
        <div className="flex gap-2 border-b">
          {teams.map((team) => (
            <button
              key={team.id}
              onClick={() => setActiveTeam(team)}
              className={`px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors ${
                activeTeam?.id === team.id
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              {team.name}
            </button>
          ))}
        </div>
      )}

      <div className="flex gap-4 flex-wrap">
        <div className="flex-1 min-w-48">
          <label className="block text-sm font-medium mb-1">{t('teamProgress.filterByMember')}</label>
          <input
            type="text"
            value={filterMember}
            onChange={(e) => setFilterMember(e.target.value)}
            placeholder={t('teamProgress.memberPlaceholder')}
            className="border rounded px-3 py-2 text-sm w-full"
          />
        </div>
        <div className="flex-1 min-w-48">
          <label className="block text-sm font-medium mb-1">{t('teamProgress.filterByCourse')}</label>
          <select
            value={selectedCourseId ?? ''}
            onChange={(e) => setSelectedCourseId(e.target.value ? Number(e.target.value) : null)}
            className="border rounded px-3 py-2 text-sm w-full"
          >
            <option value="">{t('teamProgress.allCourses')}</option>
            {courses?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
      </div>

      {!activeTeam ? (
        <p className="text-muted-foreground">{t('teamProgress.noTeam')}</p>
      ) : selectedCourseId ? (
        courseProgressLoading ? (
          <div>{t('common.loading')}</div>
        ) : (
          <CourseProgressView
            courseName={courseProgress?.courseName ?? ''}
            totalLessons={courseProgress?.totalLessons ?? 0}
            members={filteredCourseMembers}
            t={t}
          />
        )
      ) : progressLoading ? (
        <div>{t('common.loading')}</div>
      ) : (
        <MemberProgressView members={filteredMembers} t={t} />
      )}
    </div>
  );
}

function MemberProgressView({
  members,
  t,
}: {
  members: TeamMemberProgress[];
  t: (key: string, opts?: Record<string, unknown>) => string;
}) {
  if (members.length === 0) {
    return <p className="text-muted-foreground">{t('teamProgress.noMembers')}</p>;
  }

  return (
    <div className="rounded-md border overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b bg-muted/50">
            <th className="p-3 text-left font-medium">{t('teamProgress.member')}</th>
            <th className="p-3 text-right font-medium">{t('teamProgress.enrolled')}</th>
            <th className="p-3 text-right font-medium">{t('teamProgress.completed')}</th>
            <th className="p-3 text-right font-medium">{t('teamProgress.overallPercent')}</th>
            <th className="p-3 text-left font-medium">{t('teamProgress.overdueWarning')}</th>
          </tr>
        </thead>
        <tbody>
          {members.map((member) => {
            const overdueCourses = member.courses.filter((c) => c.isOverdue);
            return (
              <tr key={member.userId} className="border-b last:border-0">
                <td className="p-3 font-medium">{member.userName}</td>
                <td className="p-3 text-right">{member.enrolledCourses}</td>
                <td className="p-3 text-right">{member.completedCourses}</td>
                <td className="p-3 text-right">
                  <ProgressBar percent={member.overallPercent} />
                </td>
                <td className="p-3">
                  {overdueCourses.length > 0 && (
                    <span className="text-xs px-2 py-0.5 bg-red-100 text-red-800 rounded">
                      {t('teamProgress.overdueCount', { count: overdueCourses.length })}
                    </span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

function CourseProgressView({
  courseName,
  totalLessons,
  members,
  t,
}: {
  courseName: string;
  totalLessons: number;
  members: CourseMemberItem[];
  t: (key: string, opts?: Record<string, unknown>) => string;
}) {
  if (members.length === 0) {
    return <p className="text-muted-foreground">{t('teamProgress.noMembers')}</p>;
  }

  return (
    <div className="space-y-2">
      <h2 className="text-lg font-semibold">
        {courseName} — {t('teamProgress.totalLessons', { count: totalLessons })}
      </h2>
      <div className="rounded-md border overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('teamProgress.member')}</th>
              <th className="p-3 text-left font-medium">{t('teamProgress.status')}</th>
              <th className="p-3 text-right font-medium">{t('teamProgress.completedLessons')}</th>
              <th className="p-3 text-right font-medium">{t('teamProgress.percent')}</th>
              <th className="p-3 text-left font-medium"></th>
            </tr>
          </thead>
          <tbody>
            {members.map((member) => (
              <tr key={member.userId} className="border-b last:border-0">
                <td className="p-3 font-medium">{member.userName}</td>
                <td className="p-3">
                  <StatusBadge status={member.status} t={t} />
                </td>
                <td className="p-3 text-right">{member.completedLessons}</td>
                <td className="p-3 text-right">
                  <ProgressBar percent={member.percentComplete} />
                </td>
                <td className="p-3">
                  {member.isOverdue && (
                    <span className="text-xs px-2 py-0.5 bg-red-100 text-red-800 rounded">
                      {t('teamProgress.overdue')}
                    </span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ProgressBar({ percent }: { percent: number }) {
  return (
    <div className="flex items-center gap-2 justify-end">
      <div className="w-20 bg-muted rounded-full h-2 overflow-hidden">
        <div className="h-2 rounded-full bg-primary transition-all" style={{ width: `${Math.min(percent, 100)}%` }} />
      </div>
      <span className="text-xs text-muted-foreground w-10 text-right">{percent}%</span>
    </div>
  );
}

function StatusBadge({ status, t }: { status: string; t: (key: string) => string }) {
  const cfg: Record<string, { label: string; className: string }> = {
    NotStarted: { label: t('teamProgress.notStarted'), className: 'bg-muted text-muted-foreground' },
    InProgress: { label: t('teamProgress.inProgress'), className: 'bg-blue-100 text-blue-800' },
    Completed: { label: t('teamProgress.completedStatus'), className: 'bg-green-100 text-green-800' },
  };
  const { label, className } = cfg[status] ?? cfg['NotStarted'];
  return <span className={`text-xs px-2 py-0.5 rounded ${className}`}>{label}</span>;
}
