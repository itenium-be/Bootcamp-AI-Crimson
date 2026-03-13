import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearch } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2 } from 'lucide-react';
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
import { confirmPasswordReset } from '@/api/client';

const createFormSchema = (t: (key: string) => string) =>
  z
    .object({
      newPassword: z.string().min(8, t('auth.passwordTooShort')),
      confirmPassword: z.string().min(1, t('auth.confirmPasswordRequired')),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t('auth.passwordsMustMatch'),
      path: ['confirmPassword'],
    });

type FormData = z.infer<ReturnType<typeof createFormSchema>>;

export function ResetPassword() {
  const { t } = useTranslation();
  const formSchema = createFormSchema(t);
  const search = useSearch({ from: '/(auth)/reset-password' });
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  });

  const token = search.token ?? '';
  const email = search.email ?? '';

  const onSubmit = async (data: FormData) => {
    setIsLoading(true);
    setError(null);
    try {
      await confirmPasswordReset(email, token, data.newPassword);
      setSuccess(true);
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosError = err as { response?: { data?: { error?: string } } };
        const code = axiosError.response?.data?.error;
        if (code === 'SsoUser') {
          setError(t('auth.ssoUserCannotReset'));
        } else {
          setError(t('auth.resetTokenExpired'));
        }
      } else {
        setError(t('auth.resetTokenExpired'));
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (!token || !email) {
    return (
      <div className="flex items-center justify-center min-h-svh p-4">
        <Card className="w-full max-w-[400px]">
          <CardContent className="pt-6">
            <p className="text-sm text-destructive text-center">{t('auth.invalidResetLink')}</p>
          </CardContent>
          <CardFooter className="justify-center">
            <Link to="/forgot-password" className="text-sm underline hover:text-foreground text-muted-foreground">
              {t('auth.requestNewResetLink')}
            </Link>
          </CardFooter>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-svh p-4">
      <Card className="w-full max-w-[400px]">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">{t('auth.resetPassword')}</CardTitle>
          <CardDescription>{t('auth.resetPasswordDescription')}</CardDescription>
        </CardHeader>

        <CardContent>
          {success ? (
            <div className="p-4 text-sm text-center bg-muted rounded-md">{t('auth.passwordResetSuccess')}</div>
          ) : (
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                {error && <div className="p-3 text-sm text-destructive bg-destructive/10 rounded-md">{error}</div>}
                <FormField
                  control={form.control}
                  name="newPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('auth.newPassword')}</FormLabel>
                      <FormControl>
                        <Input type="password" placeholder={t('auth.enterNewPassword')} {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="confirmPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('auth.confirmPassword')}</FormLabel>
                      <FormControl>
                        <Input type="password" placeholder={t('auth.confirmYourPassword')} {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading && <Loader2 className="size-4 animate-spin mr-2" />}
                  {t('auth.resetPassword')}
                </Button>
              </form>
            </Form>
          )}
        </CardContent>

        {success && (
          <CardFooter className="justify-center">
            <Link to="/sign-in" className="text-sm underline hover:text-foreground text-muted-foreground">
              {t('auth.backToSignIn')}
            </Link>
          </CardFooter>
        )}
      </Card>
    </div>
  );
}
