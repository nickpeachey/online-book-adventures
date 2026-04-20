export function ReaderSkeleton() {
  return (
    <div className="mx-auto max-w-2xl animate-pulse space-y-6">
      <div className="h-8 w-2/3 rounded bg-gray-200" />
      <div className="space-y-3 rounded-xl border border-gray-200 bg-white p-6">
        <div className="h-4 w-full rounded bg-gray-200" />
        <div className="h-4 w-full rounded bg-gray-200" />
        <div className="h-4 w-5/6 rounded bg-gray-200" />
        <div className="h-4 w-4/5 rounded bg-gray-200" />
      </div>
      <div className="space-y-3">
        <div className="h-10 w-full rounded-lg bg-gray-200" />
        <div className="h-10 w-full rounded-lg bg-gray-200" />
        <div className="h-10 w-3/4 rounded-lg bg-gray-200" />
      </div>
    </div>
  )
}
