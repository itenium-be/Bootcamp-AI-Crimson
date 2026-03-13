import { createFileRoute } from '@tanstack/react-router';
import { z } from 'zod';
import { ResetPassword } from '@/pages/ResetPassword';

const searchSchema = z.object({
  token: z.string().optional(),
  email: z.string().optional(),
});

export const Route = createFileRoute('/(auth)/reset-password')({
  component: ResetPassword,
  validateSearch: searchSchema,
});
