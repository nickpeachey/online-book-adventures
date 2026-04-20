import type { Metadata } from 'next'
import { Geist } from 'next/font/google'
import './globals.css'
import { Providers } from '@/components/providers/Providers'
import { Navigation } from '@/components/layout/Navigation'

const geist = Geist({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'Online Book Adventures',
  description: 'Choose Your Own Adventure — read and create interactive stories',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={`${geist.className} min-h-screen bg-gray-50 text-gray-900`}>
        <Providers>
          <Navigation />
          <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
            {children}
          </main>
        </Providers>
      </body>
    </html>
  )
}
