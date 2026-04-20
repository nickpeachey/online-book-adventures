import { apiSlice } from './apiSlice'

export const aiApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    generateStory: builder.mutation<string, { prompt: string }>({
      query: (body) => ({
        url: '/api/ai/generate-story',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Story'],
    }),
    suggestNodeContent: builder.mutation<string, { storyId: string; nodeTitle: string; currentContent?: string }>({
      query: ({ storyId, nodeTitle, currentContent }) => ({
        url: `/api/ai/stories/${storyId}/suggest-node`,
        method: 'POST',
        body: { nodeTitle, currentContent: currentContent ?? '' },
      }),
    }),
  }),
})

export const { useGenerateStoryMutation, useSuggestNodeContentMutation } = aiApi
