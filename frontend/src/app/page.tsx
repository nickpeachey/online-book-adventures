import Link from 'next/link'
import { BookOpen, Pencil, BarChart2 } from 'lucide-react'

export default function HomePage() {
  return (
    <div className="py-16 text-center">
      <h1 className="text-4xl font-extrabold tracking-tight text-gray-900 sm:text-5xl">
        Choose Your Own <span className="text-indigo-600">Adventure</span>
      </h1>
      <p className="mx-auto mt-4 max-w-xl text-lg text-gray-500">
        Read interactive stories where every decision shapes the outcome — or build your own with our visual editor.
      </p>

      <div className="mt-10 flex justify-center gap-4">
        <Link
          href="/stories"
          className="rounded-lg bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow hover:bg-indigo-700 transition-colors"
        >
          Browse Stories
        </Link>
        <Link
          href="/register"
          className="rounded-lg border border-indigo-600 px-6 py-3 text-sm font-semibold text-indigo-600 hover:bg-indigo-50 transition-colors"
        >
          Start Creating
        </Link>
      </div>

      <div className="mt-20 grid grid-cols-1 gap-8 sm:grid-cols-3">
        {[
          { icon: BookOpen, title: 'Read Stories', desc: 'Explore hundreds of branching stories. Every choice matters.' },
          { icon: Pencil, title: 'Build Stories', desc: 'Use our visual node editor to craft your own CYOA adventures.' },
          { icon: BarChart2, title: 'Track Progress', desc: 'Resume where you left off and track your reading history.' },
        ].map(({ icon: Icon, title, desc }) => (
          <div key={title} className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <Icon className="mx-auto h-8 w-8 text-indigo-500" />
            <h3 className="mt-3 text-base font-semibold text-gray-900">{title}</h3>
            <p className="mt-1 text-sm text-gray-500">{desc}</p>
          </div>
        ))}
      </div>
    </div>
  )
}
