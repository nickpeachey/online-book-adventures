import { apiSlice } from './apiSlice'

interface RegisterRequest {
  username: string
  email: string
  password: string
}

interface LoginRequest {
  email: string
  password: string
}

interface AuthResult {
  userId: string
  username: string
  accessToken: string
}

export const authApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    register: builder.mutation<AuthResult, RegisterRequest>({
      query: (body) => ({ url: '/api/auth/register', method: 'POST', body }),
    }),
    login: builder.mutation<AuthResult, LoginRequest>({
      query: (body) => ({ url: '/api/auth/login', method: 'POST', body }),
    }),
  }),
})

export const { useRegisterMutation, useLoginMutation } = authApi
