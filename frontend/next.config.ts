import type { NextConfig } from 'next'

// API_URL is a server-side-only env var used by Next.js rewrites (e.g. http://backend:8080 in Docker).
// Falls back to NEXT_PUBLIC_API_URL (baked into the client bundle at build time).
const apiUrl = process.env.API_URL ?? process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8080'

const nextConfig: NextConfig = {
  output: 'standalone',
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${apiUrl}/api/:path*`,
      },
    ]
  },
  images: {
    remotePatterns: [
      // Local dev
      { protocol: 'http', hostname: 'localhost', port: '9000' },
      // Docker Compose network
      { protocol: 'http', hostname: 'minio', port: '9000' },
    ],
  },
}

export default nextConfig
