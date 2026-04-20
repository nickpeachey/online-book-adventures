'use client'

import Link from 'next/link'
import { useAppSelector, useAppDispatch } from '@/store/hooks'
import { clearCredentials } from '@/store/slices/authSlice'
import { BookOpen, PlusCircle, LogIn, LogOut, User } from 'lucide-react'

export function Navigation() {
  const { isAuthenticated, username } = useAppSelector((s) => s.auth)
  const dispatch = useAppDispatch()

  return (
    <nav className="border-b border-gray-200 bg-white shadow-sm">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        <Link href="/" className="flex items-center gap-2 text-xl font-bold text-indigo-600 hover:text-indigo-700">
          <BookOpen className="h-6 w-6" />
          Book Adventures
        </Link>

        <div className="flex items-center gap-4">
          <Link href="/stories" className="text-sm font-medium text-gray-600 hover:text-gray-900">
            Browse
          </Link>

          {isAuthenticated ? (
            <>
              <Link href="/stories/new" className="flex items-center gap-1 text-sm font-medium text-gray-600 hover:text-gray-900">
                <PlusCircle className="h-4 w-4" />
                Create
              </Link>
              <span className="flex items-center gap-1 text-sm text-gray-500">
                <User className="h-4 w-4" />
                {username}
              </span>
              <button
                onClick={() => dispatch(clearCredentials())}
                className="flex items-center gap-1 text-sm font-medium text-red-500 hover:text-red-700"
              >
                <LogOut className="h-4 w-4" />
                Sign out
              </button>
            </>
          ) : (
            <Link href="/login" className="flex items-center gap-1 text-sm font-medium text-indigo-600 hover:text-indigo-700">
              <LogIn className="h-4 w-4" />
              Sign in
            </Link>
          )}
        </div>
      </div>
    </nav>
  )
}
