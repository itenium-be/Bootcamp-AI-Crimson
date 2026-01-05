import { createFileRoute, redirect } from '@tanstack/react-router';
import { z } from 'zod';
import { SignIn } from '@/pages/SignIn';
import { useAuthStore } from '@/stores';

const searchSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute('/(auth)/sign-in')({
  component: SignIn,
  validateSearch: searchSchema,
  beforeLoad: () => {
    const { isAuthenticated } = useAuthStore.getState();
    if (isAuthenticated) {
      throw redirect({ to: '/' });
    }
  },
});
