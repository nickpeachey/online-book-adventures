'use client'

import { Suspense, useTransition, useState } from 'react'
import Link from 'next/link'
import Image from 'next/image'
import { useListStoriesQuery } from '@/store/api/storiesApi'
import { useAppSelector } from '@/store/hooks'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { StoriesSkeleton } from '@/components/skeletons/StoriesSkeleton'
import { BookOpen, Star, Search, ChevronLeft, ChevronRight } from 'lucide-react'

function StoriesGrid({ search, page }: { search: string; page: number }) {
  const { data, isLoading, isError } = useListStoriesQuery({ page, pageSize: 12, search })

  if (isLoading) return <StoriesSkeleton />
  if (isError) return <p className="text-center text-red-500">Failed to load stories.</p>
  if (!data || data.stories.length === 0) {
    return (
      <div className="py-16 text-center">
        <BookOpen className="mx-auto h-12 w-12 text-gray-300" />
        <p className="mt-4 text-gray-500">No stories found{search ? ` for "${search}"` : ''}.</p>
      </div>
    )
  }

  const published = data.stories.filter((s) => s.isPublished)
  const totalPages = Math.ceil(data.totalCount / 12)

  return (
    <>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {published.map((story) => (
          <Link key={story.id} href={`/stories/${story.id}`} className="group">
            <Card className="flex h-full flex-col transition-shadow hover:shadow-md">
              {story.coverImageUrl ? (
                <div className="relative mb-4 h-40 w-full overflow-hidden rounded-lg">
                  <Image
                    src={story.coverImageUrl}
                    alt={story.title}
                    fill
                    className="object-cover transition-transform group-hover:scale-105"
                  />
                </div>
              ) : (
                <div className="mb-4 flex h-40 w-full items-center justify-center rounded-lg bg-indigo-50">
                  <BookOpen className="h-12 w-12 text-indigo-300" />
                </div>
              )}
              <h2 className="text-base font-semibold text-gray-900 group-hover:text-indigo-600">
                {story.title}
              </h2>
              <p className="mt-1 flex-1 text-sm text-gray-500 line-clamp-2">{story.description}</p>
              <div className="mt-4 flex items-center justify-between text-xs text-gray-400">
                <span>by {story.authorUsername}</span>
                <span className="flex items-center gap-1">
                  <Star className="h-3 w-3" />
                  {'averageRating' in story && (story as { averageRating?: number }).averageRating
                    ? ((story as { averageRating: number }).averageRating).toFixed(1)
                    : '—'}
                </span>
              </div>
            </Card>
          </Link>
        ))}
      </div>

      {totalPages > 1 && (
        <div data-testid="pagination" className="mt-8 flex items-center justify-center gap-4">
          <PageControls page={page} totalPages={totalPages} />
        </div>
      )}
    </>
  )
}

function PageControls({ page, totalPages }: { page: number; totalPages: number }) {
  return (
    <>
      <Link
        href={`?page=${page - 1}`}
        aria-disabled={page <= 1}
        className={page <= 1 ? 'pointer-events-none opacity-40' : ''}
      >
        <ChevronLeft className="h-5 w-5" />
      </Link>
      <span className="text-sm text-gray-600">
        Page {page} of {totalPages}
      </span>
      <Link
        href={`?page=${page + 1}`}
        aria-disabled={page >= totalPages}
        className={page >= totalPages ? 'pointer-events-none opacity-40' : ''}
      >
        <ChevronRight className="h-5 w-5" />
      </Link>
    </>
  )
}

export default function StoriesPage({
  searchParams,
}: {
  searchParams?: Promise<{ page?: string; search?: string }>
}) {
  const isAuthenticated = useAppSelector((s) => s.auth.isAuthenticated)
  const [inputValue, setInputValue] = useState('')
  const [search, setSearch] = useState('')
  const [page] = useState(1)
  const [isPending, startTransition] = useTransition()

  function handleSearch(value: string) {
    setInputValue(value)
    startTransition(() => {
      setSearch(value)
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Browse Stories</h1>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search stories…"
              value={inputValue}
              onChange={(e) => handleSearch(e.target.value)}
              className="rounded-lg border border-gray-300 py-2 pl-9 pr-4 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
          </div>
          {isAuthenticated && (
            <Link href="/stories/new">
              <Button size="sm">Create Story</Button>
            </Link>
          )}
        </div>
      </div>

      <Suspense fallback={<StoriesSkeleton />}>
        <div className={isPending ? 'opacity-60 transition-opacity' : ''}>
          <StoriesGrid search={search} page={page} />
        </div>
      </Suspense>
    </div>
  )
}
