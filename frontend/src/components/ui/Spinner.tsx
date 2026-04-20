import { cn } from '@/lib/cn'

interface SpinnerProps {
  className?: string
}

/** Animated loading spinner. */
export function Spinner({ className }: SpinnerProps) {
  return (
    <div
      role="status"
      aria-label="Loading"
      className={cn('h-6 w-6 animate-spin rounded-full border-2 border-gray-200 border-t-indigo-600', className)}
    />
  )
}
