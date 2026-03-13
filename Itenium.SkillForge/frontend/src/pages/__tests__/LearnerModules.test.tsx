import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import type { LearnerModule } from '@/api/client';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, opts?: Record<string, unknown>) => {
      if (opts) return `${key}:${JSON.stringify(opts)}`;
      return key;
    },
  }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
}));

const mockUseQuery = vi.fn();
vi.mock('@tanstack/react-query', () => ({
  useQuery: (...args: unknown[]) => mockUseQuery(...args),
}));

vi.mock('@/api/client', () => ({
  fetchMyModules: vi.fn(),
}));

// eslint-disable-next-line import-x/order
import { LearnerModules } from '../LearnerModules';

function makeModule(overrides: Partial<LearnerModule> = {}): LearnerModule {
  return {
    id: 1,
    name: 'Test Module',
    description: null,
    completionPercent: 0,
    courses: [],
    ...overrides,
  };
}

beforeEach(() => {
  mockUseQuery.mockReturnValue({ data: [], isLoading: false });
});

describe('LearnerModules', () => {
  it('shows loading state', () => {
    mockUseQuery.mockReturnValue({ data: undefined, isLoading: true });
    render(<LearnerModules />);
    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('shows empty state when no modules', () => {
    mockUseQuery.mockReturnValue({ data: [], isLoading: false });
    render(<LearnerModules />);
    expect(screen.getByText('learnerModules.noModules')).toBeInTheDocument();
  });

  it('renders module name', () => {
    mockUseQuery.mockReturnValue({
      data: [makeModule({ name: 'Frontend Basics' })],
      isLoading: false,
    });
    render(<LearnerModules />);
    expect(screen.getByText('Frontend Basics')).toBeInTheDocument();
  });

  it('renders completion percent', () => {
    mockUseQuery.mockReturnValue({
      data: [makeModule({ completionPercent: 75 })],
      isLoading: false,
    });
    render(<LearnerModules />);
    expect(screen.getByText('75%')).toBeInTheDocument();
  });

  it('renders course count', () => {
    mockUseQuery.mockReturnValue({
      data: [
        makeModule({
          courses: [
            {
              courseId: 1,
              courseName: 'A',
              completedLessons: 0,
              totalLessons: 2,
              completionPercent: 0,
              isMandatory: false,
            },
            {
              courseId: 2,
              courseName: 'B',
              completedLessons: 1,
              totalLessons: 3,
              completionPercent: 33,
              isMandatory: true,
            },
          ],
        }),
      ],
      isLoading: false,
    });
    render(<LearnerModules />);
    expect(screen.getByText('learnerModules.courseCount:{"count":2}')).toBeInTheDocument();
  });
});
