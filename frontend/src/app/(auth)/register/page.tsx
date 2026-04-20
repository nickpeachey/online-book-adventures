'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import Link from 'next/link'
import { useRegisterMutation } from '@/store/api/authApi'
import { useAppDispatch } from '@/store/hooks'
import { setCredentials } from '@/store/slices/authSlice'

const schema = z.object({
  username: z.string().min(3, 'At least 3 characters').max(50).regex(/^[a-zA-Z0-9_]+$/, 'Letters, numbers, underscores only'),
  email: z.string().email('Enter a valid email'),
  password: z.string().min(8, 'At least 8 characters').max(100),
})

type FormValues = z.infer<typeof schema>

export default function RegisterPage() {
  const router = useRouter()
  const dispatch = useAppDispatch()
  const [register_, { isLoading, error }] = useRegisterMutation()

  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: FormValues) => {
    try {
      const result = await register_(data).unwrap()
      dispatch(setCredentials({
        userId: result.userId,
        username: result.username,
        token: result.accessToken,
      }))
      router.push('/stories')
    } catch {
      // error displayed below
    }
  }

  return (
    <div className="mx-auto max-w-sm">
      <h1 className="text-2xl font-bold text-gray-900">Create account</h1>
      <p className="mt-1 text-sm text-gray-500">
        Already have an account?{' '}
        <Link href="/login" className="text-indigo-600 hover:underline">Sign in</Link>
      </p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
        {(['username', 'email', 'password'] as const).map((field) => (
          <div key={field}>
            <label className="block text-sm font-medium text-gray-700 capitalize">{field}</label>
            <input
              {...register(field)}
              type={field === 'password' ? 'password' : field === 'email' ? 'email' : 'text'}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
            />
            {errors[field] && <p className="mt-1 text-xs text-red-500">{errors[field]?.message}</p>}
          </div>
        ))}

        {error && <p className="text-sm text-red-500">Registration failed. Try a different email or username.</p>}

        <button
          type="submit"
          disabled={isLoading}
          className="w-full rounded-lg bg-indigo-600 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          {isLoading ? 'Creating account…' : 'Create account'}
        </button>
      </form>
    </div>
  )
}
