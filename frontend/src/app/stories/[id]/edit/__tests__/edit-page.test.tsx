import { render, screen } from '@testing-library/react'
import { act } from 'react'

// ─── Mock React Flow ──────────────────────────────────────────────────────────

jest.mock('@xyflow/react', () => ({
  ReactFlow: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="react-flow">{children}</div>
  ),
  Background: () => null,
  Controls: () => null,
  MiniMap: () => null,
  useNodesState: jest.fn(() => [[], jest.fn(), jest.fn()]),
  useEdgesState: jest.fn(() => [[], jest.fn(), jest.fn()]),
  addEdge: jest.fn(),
  MarkerType: { ArrowClosed: 'arrowclosed' },
  Position: { Top: 'top', Bottom: 'bottom' },
  Handle: () => null,
}))

jest.mock('@xyflow/react/dist/style.css', () => ({}), { virtual: true })

// ─── Mock API slices ──────────────────────────────────────────────────────────

jest.mock('@/store/api/storiesApi', () => ({
  useGetStoryQuery: jest.fn(),
  useGetStoryGraphQuery: jest.fn(),
  useCreateNodeMutation: jest.fn(),
  useUpdateNodeMutation: jest.fn(),
  useDeleteNodeMutation: jest.fn(),
  useCreateChoiceMutation: jest.fn(),
  useUpdateChoiceMutation: jest.fn(),
  useDeleteChoiceMutation: jest.fn(),
  useUpdateStoryMutation: jest.fn(),
  usePublishStoryMutation: jest.fn(),
}))

jest.mock('@/store/api/aiApi', () => ({
  useGenerateStoryMutation: jest.fn(),
  useSuggestNodeContentMutation: jest.fn(),
}))

jest.mock('@/store/hooks', () => ({
  useAppSelector: jest.fn(),
  useAppDispatch: jest.fn(() => jest.fn()),
}))

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(() => ({ push: jest.fn() })),
}))

jest.mock('next/link', () => ({
  __esModule: true,
  default: ({ href, children, ...p }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...p}>
      {children}
    </a>
  ),
}))

// ─── Imports (after mocks) ────────────────────────────────────────────────────

import EditPage from '../page'

import {
  useGetStoryQuery,
  useGetStoryGraphQuery,
  useCreateNodeMutation,
  useUpdateNodeMutation,
  useDeleteNodeMutation,
  useCreateChoiceMutation,
  useUpdateChoiceMutation,
  useDeleteChoiceMutation,
  usePublishStoryMutation,
} from '@/store/api/storiesApi'
import { useGenerateStoryMutation, useSuggestNodeContentMutation } from '@/store/api/aiApi'
import { useAppSelector } from '@/store/hooks'

// ─── Mock data ────────────────────────────────────────────────────────────────

const mockStory = {
  id: 'story-1',
  title: 'My Story',
  description: 'desc',
  authorId: 'user-1',
  authorUsername: 'alice',
  isPublished: false,
  coverImageUrl: null,
  createdAt: '',
  updatedAt: '',
}

const mockGraph = {
  story: mockStory,
  nodes: [
    {
      id: 'n1',
      storyId: 'story-1',
      title: 'Start',
      content: 'Begin here',
      isStart: true,
      isEnd: false,
      positionX: 100,
      positionY: 100,
    },
  ],
  choices: [],
}

const noopMutation = [jest.fn(), { isLoading: false }]

function setupMocks({ isPublished = false, isAuthenticated = true } = {}) {
  const story = { ...mockStory, isPublished }

  ;(useAppSelector as jest.Mock).mockImplementation((selector) =>
    selector({ auth: { isAuthenticated, userId: 'user-1', username: 'alice', token: 'tok' } }),
  )
  ;(useGetStoryQuery as jest.Mock).mockReturnValue({ data: story, isLoading: false })
  ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({ data: mockGraph, isLoading: false })
  ;(useCreateNodeMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useUpdateNodeMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useDeleteNodeMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useCreateChoiceMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useUpdateChoiceMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useDeleteChoiceMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(usePublishStoryMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useGenerateStoryMutation as jest.Mock).mockReturnValue(noopMutation)
  ;(useSuggestNodeContentMutation as jest.Mock).mockReturnValue(noopMutation)
}

const resolvedParams = Promise.resolve({ id: 'story-1' })

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('EditPage', () => {
  beforeEach(() => {
    setupMocks()
  })

  it('shows loading skeleton when data is loading', async () => {
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({ data: undefined, isLoading: true })
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({ data: undefined, isLoading: true })

    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    const pulseEls = document.querySelectorAll('.animate-pulse')
    expect(pulseEls.length).toBeGreaterThan(0)
  })

  it('renders story title in toolbar', async () => {
    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByText('My Story')).toBeInTheDocument()
  })

  it('renders React Flow canvas', async () => {
    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByTestId('react-flow')).toBeInTheDocument()
  })

  it('shows "+ Add Node" button', async () => {
    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByRole('button', { name: /\+ add node/i })).toBeInTheDocument()
  })

  it('shows "Publish" button when story is not published', async () => {
    setupMocks({ isPublished: false })

    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByRole('button', { name: /^publish$/i })).toBeInTheDocument()
  })

  it('shows "Unpublish" button when story is published', async () => {
    setupMocks({ isPublished: true })

    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByRole('button', { name: /unpublish/i })).toBeInTheDocument()
  })

  it('shows "AI Generate" button', async () => {
    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByRole('button', { name: /ai generate/i })).toBeInTheDocument()
  })

  it('shows "Not authorised" when not authenticated', async () => {
    setupMocks({ isAuthenticated: false })

    await act(async () => {
      render(<EditPage params={resolvedParams} />)
    })

    expect(screen.getByText(/not authorised/i)).toBeInTheDocument()
  })
})
