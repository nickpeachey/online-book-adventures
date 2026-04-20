'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import Link from 'next/link'
import { useCreateStoryMutation } from '@/store/api/storiesApi'
import { useGenerateStoryMutation } from '@/store/api/aiApi'
import { useAppSelector } from '@/store/hooks'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'

const manualSchema = z.object({
  title: z.string().min(3, 'At least 3 characters').max(100),
  description: z.string().min(10, 'At least 10 characters').max(500),
})

type ManualFormValues = z.infer<typeof manualSchema>

export default function NewStoryPage() {
  const router = useRouter()
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated)
  const [mode, setMode] = useState<'manual' | 'ai'>('manual')

  const [createStory, { isLoading: isCreating, error: createError }] = useCreateStoryMutation()
  const [generateStory, { isLoading: isGenerating }] = useGenerateStoryMutation()

  const { register, handleSubmit, formState: { errors } } = useForm<ManualFormValues>({
    resolver: zodResolver(manualSchema),
  })

  const [prompt, setPrompt] = useState('')
  const [promptError, setPromptError] = useState<string | null>(null)
  const [aiError, setAiError] = useState<string | null>(null)

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-sm py-16 text-center">
        <p className="text-gray-600">You need to be signed in to create a story.</p>
        <Link
          href="/login"
          className="mt-4 inline-block rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
        >
          Sign in
        </Link>
      </div>
    )
  }

  const onManualSubmit = async (data: ManualFormValues) => {
    try {
      const storyId = await createStory({ title: data.title, description: data.description }).unwrap()
      router.push(`/stories/${storyId}/edit`)
    } catch {
      // error displayed below
    }
  }

  const onAiSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setAiError(null)

    if (prompt.trim().length < 20) {
      setPromptError('Prompt must be at least 20 characters.')
      return
    }
    setPromptError(null)

    try {
      const storyId = await generateStory({ prompt }).unwrap()
      router.push(`/stories/${storyId}/edit`)
    } catch {
      setAiError('AI generation failed. Try again or switch to manual mode.')
    }
  }

  return (
    <div className="mx-auto max-w-xl py-10">
      <h1 className="text-2xl font-bold text-gray-900">Create a New Story</h1>
      <p className="mt-1 text-sm text-gray-500">Choose how you want to start your adventure.</p>

      {/* Mode tabs */}
      <div className="mt-6 flex gap-1 border-b border-gray-200">
        <button
          type="button"
          onClick={() => setMode('manual')}
          className={`px-4 py-2 text-sm font-medium transition-colors focus:outline-none ${
            mode === 'manual'
              ? 'border-b-2 border-indigo-600 text-indigo-600'
              : 'text-gray-500 hover:text-gray-700'
          }`}
        >
          Manual
        </button>
        <button
          type="button"
          onClick={() => setMode('ai')}
          className={`px-4 py-2 text-sm font-medium transition-colors focus:outline-none ${
            mode === 'ai'
              ? 'border-b-2 border-indigo-600 text-indigo-600'
              : 'text-gray-500 hover:text-gray-700'
          }`}
        >
          AI Generate
        </button>
      </div>

      <Card className="mt-6">
        {mode === 'manual' ? (
          <form onSubmit={handleSubmit(onManualSubmit)} className="space-y-4">
            <div>
              <label htmlFor="title" className="block text-sm font-medium text-gray-700">
                Title
              </label>
              <input
                id="title"
                {...register('title')}
                type="text"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
              />
              {errors.title && (
                <p className="mt-1 text-xs text-red-500">{errors.title.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                Description
              </label>
              <textarea
                id="description"
                {...register('description')}
                rows={4}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
              />
              {errors.description && (
                <p className="mt-1 text-xs text-red-500">{errors.description.message}</p>
              )}
            </div>

            {createError && (
              <p className="text-sm text-red-500">Failed to create story. Please try again.</p>
            )}

            <Button type="submit" variant="primary" isLoading={isCreating} className="w-full">
              Create Story
            </Button>
          </form>
        ) : (
          <form onSubmit={onAiSubmit} className="space-y-4">
            <div>
              <label htmlFor="prompt" className="block text-sm font-medium text-gray-700">
                Describe your story
              </label>
              <textarea
                id="prompt"
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                rows={5}
                placeholder="e.g. A pirate adventure where the hero must choose between loyalty and treasure, with multiple endings based on moral choices..."
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
              />
              {promptError && (
                <p className="mt-1 text-xs text-red-500">{promptError}</p>
              )}
              <p className="mt-1 text-xs text-gray-500">
                AI will generate a complete story with 5–10 nodes and branching choices.
              </p>
            </div>

            {aiError && (
              <p className="text-sm text-red-500">{aiError}</p>
            )}

            <Button type="submit" variant="primary" isLoading={isGenerating} className="w-full">
              Generate Story ✨
            </Button>
          </form>
        )}
      </Card>
    </div>
  )
}
