import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

interface AuthState {
  userId: string | null
  username: string | null
  token: string | null
  isAuthenticated: boolean
}

const initialState: AuthState = {
  userId: null,
  username: null,
  token: null,
  isAuthenticated: false,
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials(
      state,
      action: PayloadAction<{ userId: string; username: string; token: string }>
    ) {
      state.userId = action.payload.userId
      state.username = action.payload.username
      state.token = action.payload.token
      state.isAuthenticated = true
    },
    clearCredentials(state) {
      state.userId = null
      state.username = null
      state.token = null
      state.isAuthenticated = false
    },
  },
})

export const { setCredentials, clearCredentials } = authSlice.actions
export const authReducer = authSlice.reducer
