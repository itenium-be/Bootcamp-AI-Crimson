import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import { Login } from '@/pages/Login';

const mockNavigate = vi.fn();
const mockSetToken = vi.fn();
const mockLoginApi = vi.fn();

vi.mock('@tanstack/react-router', () => ({
  useRouter: () => ({ navigate: mockNavigate }),
  useSearch: () => ({}),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

vi.mock('@/stores', () => ({
  useAuthStore: () => ({ setToken: mockSetToken }),
}));

vi.mock('@/api/client', () => ({
  loginWithEmailApi: (...args: unknown[]) => mockLoginApi(...args),
}));

vi.mock('@itenium-forge/ui', async () => {
  const rhf = await import('react-hook-form');
  const S = ({ children }: { children?: React.ReactNode }) => <>{children}</>;
  return {
    Form: rhf.FormProvider,
    FormField: rhf.Controller,
    FormItem: S,
    FormLabel: ({ children }: { children: React.ReactNode }) => <label>{children}</label>,
    FormControl: ({ children }: { children: React.ReactNode }) => <>{children}</>,
    FormMessage: ({ children }: { children?: React.ReactNode }) =>
      children ? <p role="alert">{children as React.ReactNode}</p> : null,
    Input: (props: React.InputHTMLAttributes<HTMLInputElement>) => <input {...props} />,
    Button: (props: React.ButtonHTMLAttributes<HTMLButtonElement>) => (
      <button type={props.type} disabled={props.disabled} onClick={props.onClick}>
        {props.children}
      </button>
    ),
    Card: S,
    CardHeader: S,
    CardTitle: S,
    CardDescription: S,
    CardContent: S,
    CardFooter: S,
  };
});

vi.mock('lucide-react', () => ({
  LogIn: () => <span />,
  Loader2: () => <span />,
}));

beforeEach(() => {
  vi.clearAllMocks();
});

describe('Login', () => {
  it('renders email and password fields', () => {
    render(<Login />);
    expect(screen.getByPlaceholderText('auth.enterEmail')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('auth.enterPassword')).toBeInTheDocument();
  });

  it('renders forgot password link', () => {
    render(<Login />);
    expect(screen.getByText('auth.forgotPassword')).toBeInTheDocument();
  });

  it('renders sign in button', () => {
    render(<Login />);
    expect(screen.getByRole('button', { name: /auth\.signIn/i })).toBeInTheDocument();
  });

  it('shows generic invalid credentials on failed login', async () => {
    mockLoginApi.mockRejectedValue(new Error('Unauthorized'));
    render(<Login />);

    fireEvent.change(screen.getByPlaceholderText('auth.enterEmail'), {
      target: { value: 'user@test.local' },
    });
    fireEvent.change(screen.getByPlaceholderText('auth.enterPassword'), {
      target: { value: 'wrongpassword' },
    });
    fireEvent.click(screen.getByRole('button', { name: /auth\.signIn/i }));

    await waitFor(() => {
      expect(screen.getByText('auth.invalidCredentials')).toBeInTheDocument();
    });
    expect(mockLoginApi).toHaveBeenCalledWith('user@test.local', 'wrongpassword');
  });

  it('calls setToken and navigates on successful login', async () => {
    mockLoginApi.mockResolvedValue({ access_token: 'test.jwt.token' });
    render(<Login />);

    fireEvent.change(screen.getByPlaceholderText('auth.enterEmail'), {
      target: { value: 'user@test.local' },
    });
    fireEvent.change(screen.getByPlaceholderText('auth.enterPassword'), {
      target: { value: 'ValidPassword123!' },
    });
    fireEvent.click(screen.getByRole('button', { name: /auth\.signIn/i }));

    await waitFor(() => {
      expect(mockSetToken).toHaveBeenCalledWith('test.jwt.token');
    });
    expect(mockNavigate).toHaveBeenCalledWith({ to: '/' });
  });

  it('does not show error message initially', () => {
    render(<Login />);
    expect(screen.queryByText('auth.invalidCredentials')).not.toBeInTheDocument();
  });
});
