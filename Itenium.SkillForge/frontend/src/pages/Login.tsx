import { useState } from 'react';
import { useRouter, useSearch } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { LogIn, Loader2 } from 'lucide-react';
import {
  Button,
  Input,
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@itenium-forge/ui';
import { useAuthStore } from '@/stores';
import { loginWithEmailApi } from '@/api/client';

const createFormSchema = (t: (key: string) => string) =>
  z.object({
    email: z.string().min(1, t('auth.emailRequired')).email(t('auth.invalidEmail')),
    password: z.string().min(1, t('auth.passwordRequired')),
  });

type FormData = z.infer<ReturnType<typeof createFormSchema>>;

export function Login() {
  const { t } = useTranslation();
  const formSchema = createFormSchema(t);
  const router = useRouter();
  const search = useSearch({ from: '/(auth)/login' });
  const { setToken } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  const onSubmit = async (data: FormData) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await loginWithEmailApi(data.email, data.password);
      setToken(response.access_token);

      const redirectTo = (search as { redirect?: string }).redirect || '/';
      router.navigate({ to: redirectTo });
    } catch {
      setError(t('auth.invalidCredentials'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="relative container grid h-svh flex-col items-center justify-center lg:max-w-none lg:grid-cols-[40%_1fr] lg:px-0">
      {/* Left side - Image panel */}
      <div className="relative hidden h-full flex-col bg-[#EFE3D3] p-10 text-sidebar-foreground lg:flex">
        <img
          src="/login-bg.png"
          alt={t('auth.loginBackground')}
          className="absolute inset-0 h-full w-full object-contain object-center"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/30 to-black/20" />
        <div className="relative z-20 flex items-center gap-2 text-lg font-medium">
          <img src="/favicon.png" alt={t('app.title')} className="size-6" />
          <span>{t('app.title')}</span>
        </div>
        <div className="relative z-20 mt-auto">
          <blockquote className="space-y-2">
            <p className="text-lg">
              <i>
                "Empower your team with continuous learning. Track progress, manage courses, and build skills together."
              </i>
            </p>
            <footer className="text-sm text-sidebar-foreground/70">Steven Robijns</footer>
          </blockquote>
        </div>
      </div>

      {/* Right side - Login form */}
      <div className="flex items-center justify-center p-4 lg:p-8">
        <Card className="w-full max-w-[400px]">
          {/* Mobile logo */}
          <div className="flex items-center justify-center gap-2 pt-6 lg:hidden">
            <img src="/favicon.png" alt={t('app.title')} className="size-6" />
            <span className="text-xl font-medium">{t('app.title')}</span>
          </div>

          <CardHeader className="text-center">
            <CardTitle className="text-2xl">{t('auth.welcome')}</CardTitle>
            <CardDescription>{t('auth.signInDescription')}</CardDescription>
          </CardHeader>

          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                {error && <div className="p-3 text-sm text-destructive bg-destructive/10 rounded-md">{error}</div>}
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('auth.email')}</FormLabel>
                      <FormControl>
                        <Input type="email" placeholder={t('auth.enterEmail')} {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('auth.password')}</FormLabel>
                      <FormControl>
                        <Input type="password" placeholder={t('auth.enterPassword')} {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading ? <Loader2 className="size-4 animate-spin" /> : <LogIn className="size-4" />}
                  <span className="ml-2">{t('auth.signIn')}</span>
                </Button>
              </form>
            </Form>
          </CardContent>

          <CardFooter className="flex flex-col gap-2 text-center text-sm text-muted-foreground">
            <a href="#" className="underline underline-offset-4 hover:text-primary">
              {t('auth.forgotPassword')}
            </a>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
