import { apiSlice } from './apiSlice'

export interface CreateNodeRequest {
  title: string
  content: string
  isStart?: boolean
  isEnd?: boolean
  positionX?: number
  positionY?: number
}

export interface UpdateNodeRequest {
  title: string
  content: string
  isStart: boolean
  isEnd: boolean
  positionX: number
  positionY: number
}

export interface CreateChoiceRequest {
  fromNodeId: string
  toNodeId: string
  label: string
  order: number
}

export interface UpdateChoiceRequest {
  label: string
  order: number
}

export interface StoryDto {
  id: string
  authorId: string
  authorUsername: string
  title: string
  description: string
  coverImageUrl: string | null
  isPublished: boolean
  createdAt: string
  updatedAt: string
}

export interface ListStoriesResult {
  stories: StoryDto[]
  totalCount: number
  page: number
  pageSize: number
}

export interface StoryGraphDto {
  story: StoryDto
  nodes: NodeDto[]
  choices: ChoiceDto[]
}

export interface NodeDto {
  id: string
  storyId: string
  title: string
  content: string
  isStart: boolean
  isEnd: boolean
  positionX: number
  positionY: number
}

export interface ChoiceDto {
  id: string
  fromNodeId: string
  toNodeId: string
  label: string
  order: number
}

export const storiesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    listStories: builder.query<ListStoriesResult, { page?: number; pageSize?: number; search?: string }>({
      query: ({ page = 1, pageSize = 20, search } = {}) => ({
        url: '/api/stories',
        params: { page, pageSize, ...(search ? { search } : {}) },
      }),
      providesTags: ['Story'],
    }),
    getStory: builder.query<StoryDto, string>({
      query: (id) => `/api/stories/${id}`,
      providesTags: (_result, _err, id) => [{ type: 'Story', id }],
    }),
    getStoryGraph: builder.query<StoryGraphDto, string>({
      query: (id) => `/api/stories/${id}/graph`,
      providesTags: (_result, _err, id) => [{ type: 'Story', id }],
    }),
    createStory: builder.mutation<string, { title: string; description: string }>({
      query: (body) => ({ url: '/api/stories', method: 'POST', body }),
      invalidatesTags: ['Story'],
    }),
    updateStory: builder.mutation<void, { id: string; title: string; description: string }>({
      query: ({ id, title, description }) => ({
        url: `/api/stories/${id}`,
        method: 'PUT',
        body: { title, description },
      }),
      invalidatesTags: (_result, _err, { id }) => [{ type: 'Story', id }],
    }),
    deleteStory: builder.mutation<void, string>({
      query: (id) => ({ url: `/api/stories/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Story'],
    }),
    publishStory: builder.mutation<void, { id: string; publish: boolean }>({
      query: ({ id, publish }) => ({
        url: `/api/stories/${id}/publish`,
        method: 'PATCH',
        body: { publish },
      }),
      invalidatesTags: (_result, _err, { id }) => [{ type: 'Story', id }],
    }),
    uploadCoverImage: builder.mutation<string, { storyId: string; file: File }>({
      query: ({ storyId, file }) => {
        const formData = new FormData()
        formData.append('file', file)
        return {
          url: `/api/stories/${storyId}/cover-image`,
          method: 'POST',
          body: formData,
        }
      },
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    createNode: builder.mutation<string, { storyId: string } & CreateNodeRequest>({
      query: ({ storyId, ...body }) => ({
        url: `/api/stories/${storyId}/nodes`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    updateNode: builder.mutation<void, { storyId: string; nodeId: string } & UpdateNodeRequest>({
      query: ({ storyId, nodeId, ...body }) => ({
        url: `/api/stories/${storyId}/nodes/${nodeId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    deleteNode: builder.mutation<void, { storyId: string; nodeId: string }>({
      query: ({ storyId, nodeId }) => ({
        url: `/api/stories/${storyId}/nodes/${nodeId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    createChoice: builder.mutation<string, { storyId: string } & CreateChoiceRequest>({
      query: ({ storyId, ...body }) => ({
        url: `/api/stories/${storyId}/choices`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    updateChoice: builder.mutation<void, { storyId: string; choiceId: string } & UpdateChoiceRequest>({
      query: ({ storyId, choiceId, ...body }) => ({
        url: `/api/stories/${storyId}/choices/${choiceId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
    deleteChoice: builder.mutation<void, { storyId: string; choiceId: string }>({
      query: ({ storyId, choiceId }) => ({
        url: `/api/stories/${storyId}/choices/${choiceId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Story', id: storyId }],
    }),
  }),
})

export const {
  useListStoriesQuery,
  useGetStoryQuery,
  useGetStoryGraphQuery,
  useCreateStoryMutation,
  useUpdateStoryMutation,
  useDeleteStoryMutation,
  usePublishStoryMutation,
  useUploadCoverImageMutation,
  useCreateNodeMutation,
  useUpdateNodeMutation,
  useDeleteNodeMutation,
  useCreateChoiceMutation,
  useUpdateChoiceMutation,
  useDeleteChoiceMutation,
} = storiesApi
