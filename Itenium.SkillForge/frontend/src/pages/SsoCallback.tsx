import { useEffect, useRef } from 'react';
import { useRouter, useSearch } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { Loader2 } from 'lucide-react';
import { exchangeSsoCode } from '@/api/client';
import { useAuthStore } from '@/stores';

export function SsoCallback() {
  const { t } = useTranslation();
  const router = useRouter();
  const search = useSearch({ from: '/callback' });
  const { setToken } = useAuthStore();
  const handled = useRef(false);

  useEffect(() => {
    if (handled.current) return;
    handled.current = true;

    const code = search.code;
    const error = search.error;

    if (error || !code) {
      void router.navigate({ to: '/sign-in' });
      return;
    }

    exchangeSsoCode(code)
      .then((token) => {
        setToken(token);
        void router.navigate({ to: '/' });
      })
      .catch(() => {
        void router.navigate({ to: '/sign-in' });
      });
  }, [search, router, setToken]);

  return (
    <div className="flex items-center justify-center min-h-svh gap-3">
      <Loader2 className="size-6 animate-spin" />
      <span className="text-muted-foreground">{t('auth.completingSignIn')}</span>
    </div>
  );
}
