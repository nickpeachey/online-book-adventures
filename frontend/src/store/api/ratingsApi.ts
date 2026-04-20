import { apiSlice } from './apiSlice'

export interface StoryRatingDto {
  storyId: string
  averageScore: number | null
  totalRatings: number
  userScore: number | null
}

export const ratingsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRating: builder.query<StoryRatingDto, string>({
      query: (storyId) => `/api/stories/${storyId}/ratings`,
      providesTags: (_result, _err, storyId) => [{ type: 'Rating', id: storyId }],
    }),
    rateStory: builder.mutation<void, { storyId: string; score: number }>({
      query: ({ storyId, score }) => ({
        url: `/api/stories/${storyId}/ratings`,
        method: 'POST',
        body: { score },
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Rating', id: storyId }],
    }),
  }),
})

export const { useGetRatingQuery, useRateStoryMutation } = ratingsApi
