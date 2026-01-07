import { useState } from 'react';
import { useRouter, useSearch } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { LogIn, Loader2, GraduationCap } from 'lucide-react';
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
import { loginApi } from '@/api/client';

const formSchema = z.object({
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required'),
});

type FormData = z.infer<typeof formSchema>;

export function SignIn() {
  const { t } = useTranslation();
  const router = useRouter();
  const search = useSearch({ from: '/(auth)/sign-in' });
  const { setToken } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      username: '',
      password: '',
    },
  });

  const onSubmit = async (data: FormData) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await loginApi(data.username, data.password);
      setToken(response.access_token);

      // Navigate to redirect URL or home
      const redirectTo = search.redirect || '/';
      router.navigate({ to: redirectTo });
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosError = err as { response?: { data?: { error_description?: string } } };
        setError(axiosError.response?.data?.error_description || t('auth.invalidCredentials'));
      } else {
        setError(t('auth.invalidCredentials'));
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="relative container grid h-svh flex-col items-center justify-center lg:max-w-none lg:grid-cols-2 lg:px-0">
      {/* Left side - Image panel */}
      <div className="relative hidden h-full flex-col bg-sidebar p-10 text-sidebar-foreground lg:flex">
        <img
          src="/login-bg.png"
          alt="Login background"
          className="absolute inset-0 h-full w-full object-cover"
        />
        <div className="absolute inset-0 bg-black/60" />
        <div className="relative z-20 flex items-center gap-2 text-lg font-medium">
          <GraduationCap className="size-6" />
          <span>Itenium SkillForge</span>
        </div>
        <div className="relative z-20 mt-auto">
          <blockquote className="space-y-2">
            <p className="text-lg">
              <i>"Empower your team with continuous learning. Track progress, manage courses, and build skills together."</i>
            </p>
            <footer className="text-sm text-sidebar-foreground/70">
              Steven Robijns
            </footer>
          </blockquote>
        </div>
      </div>

      {/* Right side - Login form */}
      <div className="flex items-center justify-center p-4 lg:p-8">
        <Card className="w-full max-w-[400px]">
          {/* Mobile logo */}
          <div className="flex items-center justify-center gap-2 pt-6 lg:hidden">
            <GraduationCap className="size-6 text-primary" />
            <span className="text-xl font-medium">Itenium SkillForge</span>
          </div>

          <CardHeader className="text-center">
            <CardTitle className="text-2xl">{t('auth.welcome')}</CardTitle>
            <CardDescription>{t('auth.signInDescription')}</CardDescription>
          </CardHeader>

          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                {error && (
                  <div className="p-3 text-sm text-destructive bg-destructive/10 rounded-md">
                    {error}
                  </div>
                )}
                <FormField
                  control={form.control}
                  name="username"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('auth.username')}</FormLabel>
                      <FormControl>
                        <Input placeholder={t('auth.enterUsername')} {...field} />
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
                        <Input
                          type="password"
                          placeholder={t('auth.enterPassword')}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading ? (
                    <Loader2 className="size-4 animate-spin" />
                  ) : (
                    <LogIn className="size-4" />
                  )}
                  <span className="ml-2">{t('auth.signIn')}</span>
                </Button>
              </form>
            </Form>
          </CardContent>

          <CardFooter className="flex flex-col gap-1 text-center text-xs text-muted-foreground">
            <div>Test users: backoffice / java / dotnet / multi</div>
            <div>Passwords: AdminPassword123! (backoffice) / UserPassword123! (others)</div>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
