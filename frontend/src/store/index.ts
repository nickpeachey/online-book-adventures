import { configureStore } from '@reduxjs/toolkit'
import { apiSlice } from './api/apiSlice'
import { authReducer } from './slices/authSlice'

const AUTH_STORAGE_KEY = 'cyoa_auth'

function loadAuthState() {
  try {
    const raw = typeof window !== 'undefined' ? localStorage.getItem(AUTH_STORAGE_KEY) : null
    if (!raw) return undefined
    return { auth: JSON.parse(raw) }
  } catch {
    return undefined
  }
}

export const store = configureStore({
  reducer: {
    auth: authReducer,
    [apiSlice.reducerPath]: apiSlice.reducer,
  },
  preloadedState: loadAuthState(),
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(apiSlice.middleware),
})

store.subscribe(() => {
  try {
    const { auth } = store.getState()
    if (typeof window !== 'undefined') {
      if (auth.isAuthenticated) {
        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth))
      } else {
        localStorage.removeItem(AUTH_STORAGE_KEY)
      }
    }
  } catch {
    // ignore write errors
  }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
