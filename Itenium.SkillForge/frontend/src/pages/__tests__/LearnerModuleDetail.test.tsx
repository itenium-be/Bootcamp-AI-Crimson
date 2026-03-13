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
  useParams: () => ({ id: '1' }),
  Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
}));

const mockUseQuery = vi.fn();
vi.mock('@tanstack/react-query', () => ({
  useQuery: (...args: unknown[]) => mockUseQuery(...args),
}));

vi.mock('@/api/client', () => ({
  fetchModuleProgress: vi.fn(),
}));

// eslint-disable-next-line import-x/order
import { LearnerModuleDetail } from '../LearnerModuleDetail';

function makeModule(overrides: Partial<LearnerModule> = {}): LearnerModule {
  return {
    id: 1,
    name: 'Frontend Basics',
    description: 'Learn the basics',
    completionPercent: 50,
    courses: [
      {
        courseId: 10,
        courseName: 'HTML & CSS',
        completedLessons: 2,
        totalLessons: 4,
        completionPercent: 50,
        isMandatory: true,
      },
      {
        courseId: 11,
        courseName: 'JavaScript',
        completedLessons: 0,
        totalLessons: 5,
        completionPercent: 0,
        isMandatory: false,
      },
    ],
    ...overrides,
  };
}

beforeEach(() => {
  mockUseQuery.mockReturnValue({ data: undefined, isLoading: true });
});

describe('LearnerModuleDetail', () => {
  it('shows loading state', () => {
    render(<LearnerModuleDetail />);
    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('renders module name and progress', () => {
    mockUseQuery.mockReturnValue({ data: makeModule(), isLoading: false });
    render(<LearnerModuleDetail />);
    expect(screen.getByText('Frontend Basics')).toBeInTheDocument();
    expect(screen.getAllByText('50%').length).toBeGreaterThanOrEqual(1);
  });

  it('renders courses with completion stats', () => {
    mockUseQuery.mockReturnValue({ data: makeModule(), isLoading: false });
    render(<LearnerModuleDetail />);
    expect(screen.getByText('HTML & CSS')).toBeInTheDocument();
    expect(screen.getByText('JavaScript')).toBeInTheDocument();
  });

  it('marks mandatory courses', () => {
    mockUseQuery.mockReturnValue({ data: makeModule(), isLoading: false });
    render(<LearnerModuleDetail />);
    expect(screen.getByText('learnerModules.mandatory')).toBeInTheDocument();
  });

  it('shows lesson progress for courses', () => {
    mockUseQuery.mockReturnValue({ data: makeModule(), isLoading: false });
    render(<LearnerModuleDetail />);
    expect(screen.getByText('learnerModules.lessonProgress:{"done":2,"total":4}')).toBeInTheDocument();
  });
});
