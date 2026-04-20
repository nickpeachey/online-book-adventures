export function StoriesSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <div key={i} className="animate-pulse rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          <div className="h-40 rounded-lg bg-gray-200" />
          <div className="mt-4 h-5 w-3/4 rounded bg-gray-200" />
          <div className="mt-2 h-4 w-full rounded bg-gray-200" />
          <div className="mt-1 h-4 w-5/6 rounded bg-gray-200" />
          <div className="mt-4 flex items-center justify-between">
            <div className="h-3 w-1/3 rounded bg-gray-200" />
            <div className="h-3 w-1/4 rounded bg-gray-200" />
          </div>
        </div>
      ))}
    </div>
  )
}
