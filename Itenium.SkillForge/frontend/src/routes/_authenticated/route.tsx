import { createFileRoute, redirect } from '@tanstack/react-router';
import { Layout } from '@/components/Layout';
import { useAuthStore } from '@/stores';

export const Route = createFileRoute('/_authenticated')({
  component: Layout,
  beforeLoad: ({ location }) => {
    const { isAuthenticated } = useAuthStore.getState();
    if (!isAuthenticated) {
      throw redirect({
        to: '/sign-in',
        search: {
          redirect: location.href,
        },
      });
    }
  },
});
