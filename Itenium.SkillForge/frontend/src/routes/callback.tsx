import { createFileRoute } from '@tanstack/react-router';
import { z } from 'zod';
import { SsoCallback } from '@/pages/SsoCallback';

const searchSchema = z.object({
  code: z.string().optional(),
  error: z.string().optional(),
});

export const Route = createFileRoute('/callback')({
  component: SsoCallback,
  validateSearch: searchSchema,
});
