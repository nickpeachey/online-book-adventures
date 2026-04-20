import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { act } from 'react'
import StoryDetailPage from '../page'

jest.mock('@/store/api/storiesApi', () => ({ useGetStoryQuery: jest.fn() }))
jest.mock('@/store/api/ratingsApi', () => ({
  useGetRatingQuery: jest.fn(),
  useRateStoryMutation: jest.fn(),
}))
jest.mock('@/store/api/commentsApi', () => ({
  useListCommentsQuery: jest.fn(),
  useAddCommentMutation: jest.fn(),
  useDeleteCommentMutation: jest.fn(),
}))
jest.mock('@/store/hooks', () => ({
  useAppSelector: jest.fn(),
  useAppDispatch: jest.fn(() => jest.fn()),
}))
jest.mock('next/navigation', () => ({ useRouter: jest.fn(() => ({ push: jest.fn() })) }))

import { useGetStoryQuery } from '@/store/api/storiesApi'
import { useGetRatingQuery, useRateStoryMutation } from '@/store/api/ratingsApi'
import { useListCommentsQuery, useAddCommentMutation, useDeleteCommentMutation } from '@/store/api/commentsApi'
import { useAppSelector } from '@/store/hooks'

const mockStory = {
  id: 'story-1',
  authorId: 'user-1',
  authorUsername: 'alice',
  title: 'The Lost Forest',
  description: 'A dark adventure in an enchanted wood.',
  coverImageUrl: null,
  isPublished: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-02T00:00:00Z',
}

const mockRating = { storyId: 'story-1', averageScore: 4.2, totalRatings: 18, userScore: null }

const mockComments = {
  comments: [
    {
      id: 'c1',
      userId: 'user-2',
      username: 'bob',
      body: 'Great story!',
      createdAt: '2024-01-03T00:00:00Z',
      editedAt: null,
    },
  ],
  totalCount: 1,
  page: 1,
  pageSize: 20,
}

function setupAuthenticatedUser(userId = 'user-1') {
  ;(useAppSelector as jest.Mock).mockImplementation((selector) =>
    selector({ auth: { isAuthenticated: true, userId, username: 'alice', token: 'tok' } })
  )
}

function setupUnauthenticatedUser() {
  ;(useAppSelector as jest.Mock).mockImplementation((selector) =>
    selector({ auth: { isAuthenticated: false, userId: null, username: null, token: null } })
  )
}

beforeEach(() => {
  ;(useGetRatingQuery as jest.Mock).mockReturnValue({ data: mockRating, isLoading: false })
  ;(useRateStoryMutation as jest.Mock).mockReturnValue([jest.fn(), { isLoading: false }])
  ;(useListCommentsQuery as jest.Mock).mockReturnValue({ data: mockComments, isLoading: false })
  ;(useAddCommentMutation as jest.Mock).mockReturnValue([jest.fn(), { isLoading: false }])
  ;(useDeleteCommentMutation as jest.Mock).mockReturnValue([jest.fn(), { isLoading: false }])
  setupAuthenticatedUser()
})

const resolvedParams = Promise.resolve({ id: 'story-1' })

describe('StoryDetailPage', () => {
  it('shows loading skeleton while fetching', async () => {
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    const pulseEls = document.querySelectorAll('.animate-pulse')
    expect(pulseEls.length).toBeGreaterThan(0)
  })

  it('renders title, description, and author', async () => {
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.getByText('The Lost Forest')).toBeInTheDocument()
    expect(screen.getByText('A dark adventure in an enchanted wood.')).toBeInTheDocument()
    expect(screen.getByText('by alice')).toBeInTheDocument()
  })

  it('shows "Start Reading" link', async () => {
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    const link = screen.getByRole('link', { name: /start reading/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/stories/story-1/read')
  })

  it('shows "Edit Story" link when user is the author', async () => {
    setupAuthenticatedUser('user-1')
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    const link = screen.getByRole('link', { name: /edit story/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/stories/story-1/edit')
  })

  it('does NOT show "Edit Story" link when user is not the author', async () => {
    setupAuthenticatedUser('user-99')
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.queryByRole('link', { name: /edit story/i })).not.toBeInTheDocument()
  })

  it('renders star rating widget for authenticated users', async () => {
    setupAuthenticatedUser()
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.getByLabelText('Rate 1 star')).toBeInTheDocument()
    expect(screen.getByLabelText('Rate 5 stars')).toBeInTheDocument()
  })

  it('shows "Sign in to rate" for unauthenticated users', async () => {
    setupUnauthenticatedUser()
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    const signInLinks = screen.getAllByText(/sign in/i)
    expect(signInLinks.length).toBeGreaterThan(0)
    expect(screen.getByText(/to rate/i)).toBeInTheDocument()
  })

  it('renders comments list', async () => {
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.getByText('bob')).toBeInTheDocument()
    expect(screen.getByText('Great story!')).toBeInTheDocument()
  })

  it('shows comment form for authenticated users', async () => {
    setupAuthenticatedUser()
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.getByPlaceholderText(/write a comment/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /post comment/i })).toBeInTheDocument()
  })

  it('shows "Sign in" instead of form for unauthenticated users', async () => {
    setupUnauthenticatedUser()
    ;(useGetStoryQuery as jest.Mock).mockReturnValue({
      data: mockStory,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoryDetailPage params={resolvedParams} />)
    })

    expect(screen.queryByPlaceholderText(/write a comment/i)).not.toBeInTheDocument()
    expect(screen.getByText(/to leave a comment/i)).toBeInTheDocument()
  })
})
