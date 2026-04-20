import { apiSlice } from './apiSlice'

export interface CommentDto {
  id: string
  userId: string
  username: string
  body: string
  createdAt: string
  editedAt: string | null
}

export interface ListCommentsResult {
  comments: CommentDto[]
  totalCount: number
  page: number
  pageSize: number
}

export const commentsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    listComments: builder.query<ListCommentsResult, { storyId: string; page?: number }>({
      query: ({ storyId, page = 1 }) => ({
        url: `/api/stories/${storyId}/comments`,
        params: { page, pageSize: 20 },
      }),
      providesTags: (_result, _err, { storyId }) => [{ type: 'Comment', id: storyId }],
    }),
    addComment: builder.mutation<string, { storyId: string; body: string }>({
      query: ({ storyId, body }) => ({
        url: `/api/stories/${storyId}/comments`,
        method: 'POST',
        body: { body },
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Comment', id: storyId }],
    }),
    deleteComment: builder.mutation<void, { storyId: string; commentId: string }>({
      query: ({ storyId, commentId }) => ({
        url: `/api/stories/${storyId}/comments/${commentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _err, { storyId }) => [{ type: 'Comment', id: storyId }],
    }),
  }),
})

export const { useListCommentsQuery, useAddCommentMutation, useDeleteCommentMutation } = commentsApi
