'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import Image from 'next/image'
import { BookOpen, ArrowLeft } from 'lucide-react'
import { useGetStoryQuery } from '@/store/api/storiesApi'
import { useGetRatingQuery, useRateStoryMutation } from '@/store/api/ratingsApi'
import { useListCommentsQuery, useAddCommentMutation, useDeleteCommentMutation } from '@/store/api/commentsApi'
import { useAppSelector } from '@/store/hooks'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Spinner } from '@/components/ui/Spinner'
import { cn } from '@/lib/cn'

// ---------------------------------------------------------------------------
// Star Rating
// ---------------------------------------------------------------------------

interface StarRatingProps {
  storyId: string
  isAuthenticated: boolean
}

function StarRating({ storyId, isAuthenticated }: StarRatingProps) {
  const { data: rating, isLoading } = useGetRatingQuery(storyId)
  const [rateStory, { isLoading: isRating }] = useRateStoryMutation()
  const [hoverStar, setHoverStar] = useState<number | null>(null)

  if (isLoading) return <p className="text-sm text-gray-400">Loading ratings…</p>

  const averageScore = rating?.averageScore ?? null
  const totalRatings = rating?.totalRatings ?? 0
  const userScore = rating?.userScore ?? null

  async function handleRate(score: number) {
    await rateStory({ storyId, score })
  }

  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold text-gray-900">Ratings</h2>
      <p className="text-sm text-gray-600">
        {averageScore !== null && totalRatings > 0
          ? `★ ${averageScore.toFixed(1)} (${totalRatings} rating${totalRatings !== 1 ? 's' : ''})`
          : 'No ratings yet'}
      </p>
      {isAuthenticated ? (
        <div className="flex items-center gap-1">
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onClick={() => handleRate(star)}
              disabled={isRating}
              className={cn(
                'text-2xl transition-colors',
                star <= (hoverStar ?? userScore ?? 0) ? 'text-yellow-400' : 'text-gray-300'
              )}
              onMouseEnter={() => setHoverStar(star)}
              onMouseLeave={() => setHoverStar(null)}
              aria-label={`Rate ${star} star${star !== 1 ? 's' : ''}`}
            >
              ★
            </button>
          ))}
          {userScore && (
            <span className="ml-2 text-xs text-gray-400">Your rating: {userScore}</span>
          )}
        </div>
      ) : (
        <p className="text-sm text-gray-500">
          <Link href="/login" className="text-indigo-600 hover:underline">
            Sign in
          </Link>{' '}
          to rate
        </p>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Comments Section
// ---------------------------------------------------------------------------

interface CommentsSectionProps {
  storyId: string
  isAuthenticated: boolean
  currentUserId: string | null
}

function CommentsSection({ storyId, isAuthenticated, currentUserId }: CommentsSectionProps) {
  const { data: commentsData, isLoading } = useListCommentsQuery({ storyId })
  const [addComment, { isLoading: isSubmitting }] = useAddCommentMutation()
  const [deleteComment] = useDeleteCommentMutation()

  const [body, setBody] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  const comments = commentsData?.comments ?? []
  const totalCount = commentsData?.totalCount ?? 0

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (body.length < 10) {
      setValidationError('Comment must be at least 10 characters.')
      return
    }
    if (body.length > 500) {
      setValidationError('Comment must be 500 characters or fewer.')
      return
    }
    setValidationError(null)
    await addComment({ storyId, body })
    setBody('')
  }

  function handleDelete(commentId: string) {
    deleteComment({ storyId, commentId })
  }

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold text-gray-900">
        Comments ({isLoading ? '…' : totalCount})
      </h2>

      {isLoading ? (
        <div className="flex justify-center py-4">
          <Spinner />
        </div>
      ) : comments.length === 0 ? (
        <p className="text-sm text-gray-500">No comments yet. Be the first!</p>
      ) : (
        <ul className="space-y-4">
          {comments.map((comment) => (
            <li key={comment.id} className="rounded-lg border border-gray-100 bg-gray-50 p-4">
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0">
                  <span className="font-semibold text-gray-900">{comment.username}</span>
                  <span className="ml-2 text-xs text-gray-400">
                    {new Date(comment.createdAt).toLocaleDateString()}
                  </span>
                  {comment.editedAt && (
                    <span className="ml-1 text-xs text-gray-400">(edited)</span>
                  )}
                  <p className="mt-1 text-sm text-gray-700">{comment.body}</p>
                </div>
                {currentUserId === comment.userId && (
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => handleDelete(comment.id)}
                    className="shrink-0"
                  >
                    Delete
                  </Button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}

      {isAuthenticated ? (
        <form onSubmit={handleSubmit} className="space-y-2">
          <textarea
            value={body}
            onChange={(e) => setBody(e.target.value)}
            placeholder="Write a comment… (10–500 characters)"
            rows={3}
            className="w-full rounded-lg border border-gray-300 p-3 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          />
          {validationError && (
            <p className="text-xs text-red-500">{validationError}</p>
          )}
          <Button type="submit" isLoading={isSubmitting} disabled={isSubmitting}>
            Post comment
          </Button>
        </form>
      ) : (
        <p className="text-sm text-gray-500">
          <Link href="/login" className="text-indigo-600 hover:underline">
            Sign in
          </Link>{' '}
          to leave a comment
        </p>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Story Detail Page
// ---------------------------------------------------------------------------

export default function StoryDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const [storyId, setStoryId] = useState<string | null>(null)

  useEffect(() => {
    params.then((p) => setStoryId(p.id))
  }, [params])

  const auth = useAppSelector((s) => s.auth)

  const { data: story, isLoading, isError } = useGetStoryQuery(storyId ?? '', { skip: !storyId })

  if (!storyId || isLoading) {
    return (
      <div className="space-y-6 animate-pulse">
        <div className="h-64 w-full rounded-xl bg-gray-200" />
        <div className="h-8 w-1/2 rounded bg-gray-200" />
        <div className="h-4 w-1/4 rounded bg-gray-200" />
        <div className="h-20 w-full rounded bg-gray-200" />
      </div>
    )
  }

  if (isError || !story) {
    return <p className="text-center text-red-500">Failed to load story.</p>
  }

  const isAuthor = auth.userId === story.authorId

  return (
    <div className="mx-auto max-w-3xl space-y-8">
      {/* Back link */}
      <Link
        href="/stories"
        className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-900"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to stories
      </Link>

      {/* Cover image */}
      {story.coverImageUrl ? (
        <div className="relative h-64 w-full overflow-hidden rounded-xl">
          <Image
            src={story.coverImageUrl}
            alt={story.title}
            fill
            className="object-cover"
            priority
          />
        </div>
      ) : (
        <div className="flex h-64 w-full items-center justify-center rounded-xl bg-indigo-50">
          <BookOpen className="h-16 w-16 text-indigo-300" />
        </div>
      )}

      {/* Title + meta */}
      <div className="space-y-2">
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-3xl font-bold text-gray-900">{story.title}</h1>
          {story.isPublished ? (
            <span className="rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-700">
              Published
            </span>
          ) : (
            <span className="rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-700">
              Draft
            </span>
          )}
        </div>
        <p className="text-sm text-gray-500">by {story.authorUsername}</p>
        <p className="text-gray-700">{story.description}</p>
      </div>

      {/* CTAs */}
      <div className="flex flex-wrap gap-3">
        <Link href={`/stories/${storyId}/read`}>
          <Button size="lg">Start Reading</Button>
        </Link>
        {isAuthor && (
          <Link href={`/stories/${storyId}/edit`}>
            <Button variant="secondary" size="lg">
              Edit Story
            </Button>
          </Link>
        )}
      </div>

      <hr className="border-gray-200" />

      {/* Star Rating */}
      <Card>
        <StarRating storyId={storyId} isAuthenticated={auth.isAuthenticated} />
      </Card>

      <hr className="border-gray-200" />

      {/* Comments */}
      <Card>
        <CommentsSection
          storyId={storyId}
          isAuthenticated={auth.isAuthenticated}
          currentUserId={auth.userId}
        />
      </Card>
    </div>
  )
}
