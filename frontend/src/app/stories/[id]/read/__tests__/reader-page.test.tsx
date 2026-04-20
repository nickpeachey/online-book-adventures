import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { act } from 'react'

// Mock react-markdown to avoid ESM issues in jest
jest.mock('react-markdown', () => ({
  __esModule: true,
  default: ({ children }: { children: string }) => <div data-testid="markdown">{children}</div>,
}))

jest.mock('@/store/api/storiesApi', () => ({
  useGetStoryGraphQuery: jest.fn(),
}))

jest.mock('@/store/api/progressApi', () => ({
  useGetProgressQuery: jest.fn(),
  useStartStoryMutation: jest.fn(),
  useMakeChoiceMutation: jest.fn(),
}))

jest.mock('@/store/hooks', () => ({
  useAppSelector: jest.fn(),
  useAppDispatch: jest.fn(() => jest.fn()),
}))

import ReaderPage from '../page'
import { useGetStoryGraphQuery } from '@/store/api/storiesApi'
import { useGetProgressQuery, useStartStoryMutation, useMakeChoiceMutation } from '@/store/api/progressApi'
import { useAppSelector } from '@/store/hooks'

const mockGraph = {
  story: { id: 'story-1', title: 'My Adventure', authorUsername: 'alice' },
  nodes: [
    {
      id: 'node-1',
      title: 'The Beginning',
      content: 'You stand at a crossroads.',
      isStart: true,
      isEnd: false,
    },
    {
      id: 'node-2',
      title: 'The Forest',
      content: 'You enter the dark forest.',
      isStart: false,
      isEnd: false,
    },
    {
      id: 'node-end',
      title: 'The End',
      content: 'Your journey concludes.',
      isStart: false,
      isEnd: true,
    },
  ],
  choices: [
    { id: 'c1', fromNodeId: 'node-1', toNodeId: 'node-2', label: 'Go into the forest', order: 1 },
    { id: 'c2', fromNodeId: 'node-1', toNodeId: 'node-end', label: 'Turn back', order: 2 },
  ],
}

function createParamsPromise(id: string) {
  return Promise.resolve({ id })
}

describe('ReaderPage', () => {
  beforeEach(() => {
    ;(useAppSelector as jest.Mock).mockReturnValue(false)
    ;(useGetProgressQuery as jest.Mock).mockReturnValue({ data: null })
    ;(useStartStoryMutation as jest.Mock).mockReturnValue([jest.fn()])
    ;(useMakeChoiceMutation as jest.Mock).mockReturnValue([jest.fn()])
  })

  it('renders node content and title', async () => {
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: mockGraph,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    await waitFor(() => {
      expect(screen.getByText('The Beginning')).toBeInTheDocument()
      expect(screen.getByText('You stand at a crossroads.')).toBeInTheDocument()
    })
  })

  it('renders choices as clickable buttons', async () => {
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: mockGraph,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    await waitFor(() => {
      expect(screen.getByText('Go into the forest')).toBeInTheDocument()
      expect(screen.getByText('Turn back')).toBeInTheDocument()
    })
  })

  it('navigates to next node when choice is clicked', async () => {
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: mockGraph,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    await waitFor(() => screen.getByText('Go into the forest'))

    await act(async () => {
      fireEvent.click(screen.getByText('Go into the forest'))
    })

    await waitFor(() => {
      expect(screen.getByText('The Forest')).toBeInTheDocument()
      expect(screen.getByText('You enter the dark forest.')).toBeInTheDocument()
    })
  })

  it('shows The End state on end node', async () => {
    const endOnlyGraph = {
      ...mockGraph,
      nodes: [{ id: 'node-end', title: 'Finale', content: 'Story over.', isStart: true, isEnd: true }],
      choices: [],
    }
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: endOnlyGraph,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    await waitFor(() => {
      expect(screen.getByText(/the end/i)).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /restart/i })).toBeInTheDocument()
      expect(screen.getByRole('link', { name: /back to stories/i })).toBeInTheDocument()
    })
  })

  it('shows loading skeleton while graph is loading', async () => {
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    // ReaderSkeleton renders animate-pulse elements
    const pulseEls = document.querySelectorAll('.animate-pulse')
    expect(pulseEls.length).toBeGreaterThan(0)
  })

  it('resumes from saved progress node', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    ;(useGetProgressQuery as jest.Mock).mockReturnValue({
      data: { currentNodeId: 'node-2', isCompleted: false },
    })
    ;(useGetStoryGraphQuery as jest.Mock).mockReturnValue({
      data: mockGraph,
      isLoading: false,
      isError: false,
    })

    await act(async () => {
      render(<ReaderPage params={createParamsPromise('story-1')} />)
    })

    await waitFor(() => {
      expect(screen.getByText('The Forest')).toBeInTheDocument()
    })
  })
})
