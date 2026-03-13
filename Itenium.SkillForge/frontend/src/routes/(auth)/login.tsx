import { createFileRoute, redirect } from '@tanstack/react-router';
import { z } from 'zod';
import { Login } from '@/pages/Login';
import { useAuthStore } from '@/stores';

const searchSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute('/(auth)/login')({
  component: Login,
  validateSearch: searchSchema,
  beforeLoad: () => {
    const { isAuthenticated } = useAuthStore.getState();
    if (isAuthenticated) {
      throw redirect({ to: '/' });
    }
  },
});
