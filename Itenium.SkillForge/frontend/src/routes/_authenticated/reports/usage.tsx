import { createFileRoute } from '@tanstack/react-router';
import { CourseUsageReport } from '@/pages/CourseUsageReport';

export const Route = createFileRoute('/_authenticated/reports/usage')({
  component: CourseUsageReport,
});
