import { apiSlice } from './apiSlice'

export interface ProgressDto {
  progressId: string
  userId: string
  storyId: string
  currentNodeId: string
  isCompleted: boolean
  updatedAt: string
}

export interface MakeChoiceResult {
  newNodeId: string
  isEnd: boolean
}

export const progressApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProgress: builder.query<ProgressDto | null, string>({
      query: (storyId) => `/api/stories/${storyId}/progress`,
      providesTags: (_result, _err, storyId) => [{ type: 'Progress', id: storyId }],
    }),
    startStory: builder.mutation<string, string>({
      query: (storyId) => ({
        url: `/api/stories/${storyId}/progress/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _err, storyId) => [{ type: 'Progress', id: storyId }],
    }),
    makeChoice: builder.mutation<MakeChoiceResult, { storyId: string; choiceId: string }>({
      query: ({ storyId, choiceId }) => ({
        url: `/api/stories/${storyId}/progress/choose`,
        method: 'POST',
        body: { choiceId },
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Progress', id: storyId }],
    }),
  }),
})

export const { useGetProgressQuery, useStartStoryMutation, useMakeChoiceMutation } = progressApi
