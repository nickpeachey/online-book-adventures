import { cn } from '@/lib/cn'

interface CardProps {
  className?: string
  children: React.ReactNode
}

/** A simple card container with border, shadow, and rounded corners. */
export function Card({ className, children }: CardProps) {
  return (
    <div className={cn('rounded-xl border border-gray-200 bg-white p-6 shadow-sm', className)}>
      {children}
    </div>
  )
}
