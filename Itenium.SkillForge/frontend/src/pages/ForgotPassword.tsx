import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from '@tanstack/react-router';
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
import { requestPasswordReset } from '@/api/client';

const createFormSchema = (t: (key: string) => string) =>
  z.object({
    email: z.string().min(1, t('auth.emailRequired')).email(t('auth.emailInvalid')),
  });

type FormData = z.infer<ReturnType<typeof createFormSchema>>;

export function ForgotPassword() {
  const { t } = useTranslation();
  const formSchema = createFormSchema(t);
  const [submitted, setSubmitted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: { email: '' },
  });

  const onSubmit = async (data: FormData) => {
    setIsLoading(true);
    try {
      await requestPasswordReset(data.email);
    } finally {
      setIsLoading(false);
      setSubmitted(true);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-svh p-4">
      <Card className="w-full max-w-[400px]">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">{t('auth.forgotPassword')}</CardTitle>
          <CardDescription>{t('auth.forgotPasswordDescription')}</CardDescription>
        </CardHeader>

        <CardContent>
          {submitted ? (
            <div className="p-4 text-sm text-center bg-muted rounded-md">{t('auth.resetEmailSent')}</div>
          ) : (
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
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
                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading && <Loader2 className="size-4 animate-spin mr-2" />}
                  {t('auth.sendResetLink')}
                </Button>
              </form>
            </Form>
          )}
        </CardContent>

        <CardFooter className="justify-center">
          <Link to="/sign-in" className="text-sm text-muted-foreground hover:text-foreground underline">
            {t('auth.backToSignIn')}
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}
