import { render, screen, fireEvent } from '@testing-library/react'
import { act } from 'react'
import StoriesPage from '../page'

// Mock RTK Query hook
jest.mock('@/store/api/storiesApi', () => ({
  useListStoriesQuery: jest.fn(),
}))

// Mock Redux selector
jest.mock('@/store/hooks', () => ({
  useAppSelector: jest.fn(),
  useAppDispatch: jest.fn(() => jest.fn()),
}))

import { useListStoriesQuery } from '@/store/api/storiesApi'
import { useAppSelector } from '@/store/hooks'

const mockStories = [
  {
    id: '1',
    title: 'The Lost Forest',
    description: 'A dark adventure in an enchanted wood.',
    authorUsername: 'alice',
    isPublished: true,
    coverImageUrl: null,
    averageRating: 4.5,
    createdAt: '2024-01-01',
  },
  {
    id: '2',
    title: 'Space Odyssey',
    description: 'Explore the cosmos.',
    authorUsername: 'bob',
    isPublished: true,
    coverImageUrl: null,
    averageRating: 0,
    createdAt: '2024-01-02',
  },
  {
    id: '3',
    title: 'Draft Story',
    description: 'Not published.',
    authorUsername: 'carol',
    isPublished: false,
    coverImageUrl: null,
    averageRating: 0,
    createdAt: '2024-01-03',
  },
]

describe('StoriesPage', () => {
  beforeEach(() => {
    ;(useAppSelector as jest.Mock).mockReturnValue(false)
  })

  it('shows loading skeleton while fetching', () => {
    ;(useListStoriesQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    })

    render(<StoriesPage />)
    // Skeleton renders animated pulse elements
    const pulseEls = document.querySelectorAll('.animate-pulse')
    expect(pulseEls.length).toBeGreaterThan(0)
  })

  it('renders story cards for published stories', async () => {
    ;(useListStoriesQuery as jest.Mock).mockReturnValue({
      data: { stories: mockStories, totalCount: 3, page: 1, pageSize: 12 },
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoriesPage />)
    })

    expect(screen.getByText('The Lost Forest')).toBeInTheDocument()
    expect(screen.getByText('Space Odyssey')).toBeInTheDocument()
    // Draft story is filtered out (isPublished: false)
    expect(screen.queryByText('Draft Story')).not.toBeInTheDocument()
  })

  it('shows create story button for authenticated users', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    ;(useListStoriesQuery as jest.Mock).mockReturnValue({
      data: { stories: mockStories, totalCount: 2, page: 1, pageSize: 12 },
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoriesPage />)
    })

    expect(screen.getByRole('link', { name: /create story/i })).toBeInTheDocument()
  })

  it('does not show create story button for unauthenticated users', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(false)
    ;(useListStoriesQuery as jest.Mock).mockReturnValue({
      data: { stories: mockStories, totalCount: 2, page: 1, pageSize: 12 },
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoriesPage />)
    })

    expect(screen.queryByRole('link', { name: /create story/i })).not.toBeInTheDocument()
  })

  it('filters stories via search input using transition', async () => {
    const mockQuery = jest.fn().mockReturnValue({
      data: { stories: mockStories, totalCount: 3, page: 1, pageSize: 12 },
      isLoading: false,
      isError: false,
    })
    ;(useListStoriesQuery as jest.Mock).mockImplementation(mockQuery)

    await act(async () => {
      render(<StoriesPage />)
    })

    const input = screen.getByPlaceholderText(/search stories/i)
    await act(async () => {
      fireEvent.change(input, { target: { value: 'forest' } })
    })

    expect(mockQuery).toHaveBeenCalledWith(expect.objectContaining({ search: 'forest' }))
  })

  it('shows empty state when no stories found', async () => {
    ;(useListStoriesQuery as jest.Mock).mockReturnValue({
      data: { stories: [], totalCount: 0, page: 1, pageSize: 12 },
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<StoriesPage />)
    })

    expect(screen.getByText(/no stories found/i)).toBeInTheDocument()
  })
})
