import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import { useTeamStore } from '@/stores/teamStore';
import { ManagerSuggestions } from '@/pages/ManagerSuggestions';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, opts?: Record<string, unknown>) => {
      if (opts) {
        return Object.entries(opts).reduce((s, [k, v]) => s.replace(`{{${k}}}`, String(v)), key);
      }
      return key;
    },
  }),
}));

const mockApprove = vi.fn().mockResolvedValue(undefined);
const mockReject = vi.fn().mockResolvedValue(undefined);

vi.mock('@/api/client', () => ({
  fetchContentSuggestions: vi.fn(),
  approveContentSuggestion: (...args: unknown[]) => mockApprove(...args),
  rejectContentSuggestion: (...args: unknown[]) => mockReject(...args),
}));

const mockUseQuery = vi.fn();
const mockInvalidate = vi.fn();
const mockMutate = vi.fn();

vi.mock('@tanstack/react-query', () => ({
  useQuery: (...args: unknown[]) => mockUseQuery(...args),
  useMutation: ({ mutationFn }: { mutationFn: (args: unknown) => Promise<void> }) => ({
    mutate: (args: unknown) => {
      mockMutate(args);
      return mutationFn(args);
    },
    isPending: false,
  }),
  useQueryClient: () => ({ invalidateQueries: mockInvalidate }),
}));

const TEAM = { id: 42, name: 'Team Alpha' };

const pendingSuggestion = {
  id: 1,
  submittedBy: 'user-1',
  submitterName: 'Alice',
  teamId: 42,
  title: 'Great Resource',
  description: 'A very useful link',
  url: 'https://example.com',
  relatedCourseId: null,
  topic: 'TypeScript',
  status: 'pending',
  reviewedBy: null,
  reviewedAt: null,
  reviewNote: null,
  submittedAt: '2026-03-01T10:00:00Z',
};

const approvedSuggestion = {
  ...pendingSuggestion,
  id: 2,
  title: 'Approved Resource',
  status: 'approved',
  reviewNote: 'Well done!',
  reviewedBy: 'manager-1',
  reviewedAt: '2026-03-02T10:00:00Z',
};

beforeEach(() => {
  useTeamStore.setState({ mode: 'manager', selectedTeam: TEAM, teams: [TEAM] });
  mockUseQuery.mockReturnValue({ data: [], isLoading: false });
  mockApprove.mockReset().mockResolvedValue(undefined);
  mockReject.mockReset().mockResolvedValue(undefined);
  mockMutate.mockReset();
  mockInvalidate.mockReset();
});

afterEach(() => {
  useTeamStore.setState({ mode: 'backoffice', selectedTeam: null, teams: [] });
});

describe('ManagerSuggestions', () => {
  it('shows "no team" message when no team selected', () => {
    useTeamStore.setState({ mode: 'backoffice', selectedTeam: null, teams: [] });
    render(<ManagerSuggestions />);
    expect(screen.getByText('suggestions.noTeam')).toBeInTheDocument();
  });

  it('shows page title', () => {
    render(<ManagerSuggestions />);
    expect(screen.getByText('suggestions.title')).toBeInTheDocument();
  });

  it('shows "no suggestions" when list is empty', () => {
    render(<ManagerSuggestions />);
    expect(screen.getByText('suggestions.noSuggestions')).toBeInTheDocument();
  });

  it('renders a pending suggestion with title and submitter', () => {
    mockUseQuery.mockReturnValue({ data: [pendingSuggestion], isLoading: false });
    render(<ManagerSuggestions />);
    expect(screen.getByText('Great Resource')).toBeInTheDocument();
    expect(screen.getByText(/suggestions\.submittedBy/)).toBeInTheDocument();
  });

  it('shows approve and reject buttons for pending suggestions', () => {
    mockUseQuery.mockReturnValue({ data: [pendingSuggestion], isLoading: false });
    render(<ManagerSuggestions />);
    expect(screen.getByText('suggestions.approve')).toBeInTheDocument();
    expect(screen.getByText('suggestions.reject')).toBeInTheDocument();
  });

  it('does not show approve/reject for approved suggestions', () => {
    mockUseQuery.mockReturnValue({ data: [approvedSuggestion], isLoading: false });
    render(<ManagerSuggestions />);
    expect(screen.queryByText('suggestions.approve')).not.toBeInTheDocument();
    expect(screen.queryByText('suggestions.reject')).not.toBeInTheDocument();
  });

  it('calls approve with note when approve button clicked', async () => {
    mockUseQuery.mockReturnValue({ data: [pendingSuggestion], isLoading: false });
    render(<ManagerSuggestions />);

    const noteInput = screen.getByPlaceholderText('suggestions.notePlaceholder');
    fireEvent.change(noteInput, { target: { value: 'Great!' } });
    fireEvent.click(screen.getByText('suggestions.approve'));

    await waitFor(() => {
      expect(mockApprove).toHaveBeenCalledWith(1, 'Great!');
    });
  });

  it('calls reject when reject button clicked', async () => {
    mockUseQuery.mockReturnValue({ data: [pendingSuggestion], isLoading: false });
    render(<ManagerSuggestions />);

    fireEvent.click(screen.getByText('suggestions.reject'));

    await waitFor(() => {
      expect(mockReject).toHaveBeenCalledWith(1, undefined);
    });
  });

  it('shows review note for non-pending suggestions', () => {
    mockUseQuery.mockReturnValue({ data: [approvedSuggestion], isLoading: false });
    render(<ManagerSuggestions />);
    expect(screen.getByText(/Well done!/)).toBeInTheDocument();
  });

  it('shows topic when present', () => {
    mockUseQuery.mockReturnValue({ data: [pendingSuggestion], isLoading: false });
    render(<ManagerSuggestions />);
    expect(screen.getByText(/TypeScript/)).toBeInTheDocument();
  });

  it('shows loading state', () => {
    mockUseQuery.mockReturnValue({ data: undefined, isLoading: true });
    render(<ManagerSuggestions />);
    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });
});
