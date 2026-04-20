import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NewStoryPage from '../page'

jest.mock('@/store/api/storiesApi', () => ({ useCreateStoryMutation: jest.fn() }))
jest.mock('@/store/api/aiApi', () => ({ useGenerateStoryMutation: jest.fn() }))
jest.mock('@/store/hooks', () => ({
  useAppSelector: jest.fn(),
  useAppDispatch: jest.fn(() => jest.fn()),
}))
jest.mock('next/navigation', () => ({ useRouter: jest.fn(() => ({ push: jest.fn() })) }))
jest.mock('next/link', () => ({
  __esModule: true,
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...props}>{children}</a>
  ),
}))

import { useCreateStoryMutation } from '@/store/api/storiesApi'
import { useGenerateStoryMutation } from '@/store/api/aiApi'
import { useAppSelector } from '@/store/hooks'
import { useRouter } from 'next/navigation'

const mockPush = jest.fn()

beforeEach(() => {
  jest.clearAllMocks()
  ;(useRouter as jest.Mock).mockReturnValue({ push: mockPush })
  ;(useCreateStoryMutation as jest.Mock).mockReturnValue([jest.fn(), { isLoading: false }])
  ;(useGenerateStoryMutation as jest.Mock).mockReturnValue([jest.fn(), { isLoading: false }])
})

describe('NewStoryPage', () => {
  it('shows sign-in prompt when not authenticated', () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(false)
    render(<NewStoryPage />)
    expect(screen.getByText(/you need to be signed in/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /sign in/i })).toBeInTheDocument()
  })

  it('"Manual" tab is active by default', () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    render(<NewStoryPage />)
    const manualTab = screen.getByRole('button', { name: /^manual$/i })
    expect(manualTab.className).toMatch(/indigo/)
  })

  it('shows title and description fields in manual mode', () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    render(<NewStoryPage />)
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument()
  })

  it('shows validation errors on empty submit in manual mode', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const user = userEvent.setup()
    render(<NewStoryPage />)
    await user.click(screen.getByRole('button', { name: /create story/i }))
    expect(await screen.findByText(/at least 3 characters/i)).toBeInTheDocument()
    expect(await screen.findByText(/at least 10 characters/i)).toBeInTheDocument()
  })

  it('clicking "AI Generate" tab switches to AI mode', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const user = userEvent.setup()
    render(<NewStoryPage />)
    await user.click(screen.getByRole('button', { name: /ai generate/i }))
    expect(screen.getByLabelText(/describe your story/i)).toBeInTheDocument()
  })

  it('shows prompt textarea in AI mode', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const user = userEvent.setup()
    render(<NewStoryPage />)
    await user.click(screen.getByRole('button', { name: /ai generate/i }))
    expect(screen.getByRole('textbox', { name: /describe your story/i })).toBeInTheDocument()
  })

  it('shows validation error when prompt is too short in AI mode', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const user = userEvent.setup()
    render(<NewStoryPage />)
    await user.click(screen.getByRole('button', { name: /ai generate/i }))
    await user.type(screen.getByLabelText(/describe your story/i), 'short')
    await user.click(screen.getByRole('button', { name: /generate story/i }))
    expect(await screen.findByText(/at least 20 characters/i)).toBeInTheDocument()
  })

  it('calls createStory and navigates on successful manual creation', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const mockCreate = jest.fn().mockReturnValue({ unwrap: () => Promise.resolve('story-123') })
    ;(useCreateStoryMutation as jest.Mock).mockReturnValue([mockCreate, { isLoading: false }])

    const user = userEvent.setup()
    render(<NewStoryPage />)

    await user.type(screen.getByLabelText(/title/i), 'My Story Title')
    await user.type(screen.getByLabelText(/description/i), 'A story description that is long enough')
    await user.click(screen.getByRole('button', { name: /create story/i }))

    await act(async () => {})

    expect(mockCreate).toHaveBeenCalledWith({
      title: 'My Story Title',
      description: 'A story description that is long enough',
    })
    expect(mockPush).toHaveBeenCalledWith('/stories/story-123/edit')
  })

  it('calls generateStory and navigates on successful AI generation', async () => {
    ;(useAppSelector as jest.Mock).mockReturnValue(true)
    const mockGenerate = jest.fn().mockReturnValue({ unwrap: () => Promise.resolve('ai-story-456') })
    ;(useGenerateStoryMutation as jest.Mock).mockReturnValue([mockGenerate, { isLoading: false }])

    const user = userEvent.setup()
    render(<NewStoryPage />)

    await user.click(screen.getByRole('button', { name: /ai generate/i }))
    await user.type(
      screen.getByLabelText(/describe your story/i),
      'A pirate adventure where the hero must choose between loyalty and treasure'
    )
    await user.click(screen.getByRole('button', { name: /generate story/i }))

    await act(async () => {})

    expect(mockGenerate).toHaveBeenCalledWith({
      prompt: 'A pirate adventure where the hero must choose between loyalty and treasure',
    })
    expect(mockPush).toHaveBeenCalledWith('/stories/ai-story-456/edit')
  })
})
