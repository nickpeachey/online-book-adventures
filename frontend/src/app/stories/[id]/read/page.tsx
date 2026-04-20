'use client'

import { Suspense, useState, useTransition, useEffect } from 'react'
import Link from 'next/link'
import ReactMarkdown from 'react-markdown'
import { useGetStoryGraphQuery } from '@/store/api/storiesApi'
import { useGetProgressQuery, useStartStoryMutation, useMakeChoiceMutation } from '@/store/api/progressApi'
import { useAppSelector } from '@/store/hooks'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { ReaderSkeleton } from '@/components/skeletons/ReaderSkeleton'
import { BookOpen, RotateCcw, ArrowLeft } from 'lucide-react'

interface ReaderProps {
  storyId: string
}

function StoryReader({ storyId }: ReaderProps) {
  const isAuthenticated = useAppSelector((s) => s.auth.isAuthenticated)
  const { data: graph, isLoading: graphLoading, isError: graphError } = useGetStoryGraphQuery(storyId)
  const { data: progress } = useGetProgressQuery(storyId, { skip: !isAuthenticated })
  const [startStory] = useStartStoryMutation()
  const [makeChoice] = useMakeChoiceMutation()

  const [currentNodeId, setCurrentNodeId] = useState<string | null>(null)
  const [isPending, startTransition] = useTransition()

  // Set initial node from progress or story start
  useEffect(() => {
    if (currentNodeId) return
    if (progress?.currentNodeId) {
      setCurrentNodeId(progress.currentNodeId)
    } else if (graph) {
      const startNode = graph.nodes.find((n) => n.isStart)
      setCurrentNodeId(startNode?.id ?? graph.nodes[0]?.id ?? null)
      if (isAuthenticated) {
        startStory(storyId)
      }
    }
  }, [graph, progress, currentNodeId, isAuthenticated, storyId, startStory])

  if (graphLoading || !currentNodeId) return <ReaderSkeleton />
  if (graphError || !graph) return <p className="text-center text-red-500">Failed to load story.</p>

  const currentNode = graph.nodes.find((n) => n.id === currentNodeId)
  if (!currentNode) return <p className="text-center text-red-500">Node not found.</p>

  const nodeChoices = graph.choices.filter((c) => c.fromNodeId === currentNodeId)
  const isEndNode = currentNode.isEnd || nodeChoices.length === 0

  function handleChoice(choiceId: string, toNodeId: string) {
    startTransition(async () => {
      if (isAuthenticated) {
        await makeChoice({ storyId, choiceId })
      }
      setCurrentNodeId(toNodeId)
    })
  }

  function handleRestart() {
    startTransition(() => {
      const startNode = graph!.nodes.find((n) => n.isStart)
      setCurrentNodeId(startNode?.id ?? graph!.nodes[0]?.id ?? null)
      if (isAuthenticated) {
        startStory(storyId)
      }
    })
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{graph.story.title}</h1>
        <span className="text-sm text-gray-400">by {graph.story.authorUsername}</span>
      </div>

      <Card className={isPending ? 'opacity-60 transition-opacity' : ''}>
        {currentNode.title && (
          <h2 className="mb-4 text-xl font-semibold text-gray-800">{currentNode.title}</h2>
        )}
        <div className="prose prose-sm max-w-none text-gray-700">
          <ReactMarkdown>{currentNode.content}</ReactMarkdown>
        </div>
      </Card>

      {isEndNode ? (
        <div className="space-y-3 text-center">
          <p className="text-lg font-semibold text-indigo-600">✦ The End ✦</p>
          <div className="flex justify-center gap-3">
            <Button variant="secondary" onClick={handleRestart} isLoading={isPending}>
              <RotateCcw className="mr-1 inline h-4 w-4" />
              Restart
            </Button>
            <Link href="/stories">
              <Button variant="ghost">
                <ArrowLeft className="mr-1 inline h-4 w-4" />
                Back to stories
              </Button>
            </Link>
          </div>
        </div>
      ) : (
        <div className="space-y-3">
          {nodeChoices.map((choice) => (
            <button
              key={choice.id}
              onClick={() => handleChoice(choice.id, choice.toNodeId)}
              disabled={isPending}
              className="w-full rounded-lg border border-indigo-200 bg-white px-5 py-3 text-left text-sm font-medium text-indigo-700 transition-colors hover:bg-indigo-50 hover:border-indigo-400 disabled:opacity-50"
            >
              {choice.label}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

export default function ReaderPage({ params }: { params: Promise<{ id: string }> }) {
  const [storyId, setStoryId] = useState<string | null>(null)

  useEffect(() => {
    params.then((p) => setStoryId(p.id))
  }, [params])

  if (!storyId) return <ReaderSkeleton />

  return (
    <Suspense fallback={<ReaderSkeleton />}>
      <StoryReader storyId={storyId} />
    </Suspense>
  )
}
