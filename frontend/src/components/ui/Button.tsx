import { cn } from '@/lib/cn'
import type { ButtonHTMLAttributes } from 'react'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  isLoading?: boolean
}

const variants = {
  primary: 'bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50',
  secondary: 'border border-indigo-600 text-indigo-600 hover:bg-indigo-50',
  danger: 'bg-red-600 text-white hover:bg-red-700',
  ghost: 'text-gray-600 hover:text-gray-900 hover:bg-gray-100',
}

const sizes = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-4 py-2 text-sm',
  lg: 'px-6 py-3 text-base',
}

/** Reusable button component with variant and size support. */
export function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  className,
  children,
  disabled,
  ...props
}: ButtonProps) {
  return (
    <button
      disabled={disabled || isLoading}
      className={cn(
        'rounded-lg font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-1',
        variants[variant],
        sizes[size],
        className
      )}
      {...props}
    >
      {isLoading ? 'Loading…' : children}
    </button>
  )
}
