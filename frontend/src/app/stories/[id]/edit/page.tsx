'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  addEdge,
  useNodesState,
  useEdgesState,
  type Node,
  type Edge,
  type Connection,
  type NodeTypes,
  Handle,
  Position,
  MarkerType,
  type NodeProps,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import {
  useGetStoryQuery,
  useGetStoryGraphQuery,
  useCreateNodeMutation,
  useUpdateNodeMutation,
  useDeleteNodeMutation,
  useCreateChoiceMutation,
  useUpdateChoiceMutation,
  useDeleteChoiceMutation,
  usePublishStoryMutation,
  type StoryGraphDto,
} from '@/store/api/storiesApi'
import { useGenerateStoryMutation, useSuggestNodeContentMutation } from '@/store/api/aiApi'
import { useAppSelector } from '@/store/hooks'
import { Button } from '@/components/ui/Button'
import { cn } from '@/lib/cn'

// ─── Types ───────────────────────────────────────────────────────────────────

interface StoryNodeData extends Record<string, unknown> {
  title: string
  content: string
  isStart: boolean
  isEnd: boolean
  dbId: string
}

// ─── Custom Node Component ────────────────────────────────────────────────────

function StoryNodeComponent({ data, selected }: NodeProps<Node<StoryNodeData>>) {
  return (
    <div
      className={cn(
        'rounded-xl border-2 bg-white p-3 shadow-md w-48 cursor-pointer',
        selected ? 'border-indigo-500' : 'border-gray-200',
        data.isStart && 'border-green-500',
        data.isEnd && 'border-red-400',
      )}
    >
      {data.isStart && (
        <span className="mb-1 block text-xs font-bold text-green-600">START</span>
      )}
      {data.isEnd && (
        <span className="mb-1 block text-xs font-bold text-red-500">END</span>
      )}
      <p className="text-sm font-semibold text-gray-900 truncate">{data.title || 'Untitled'}</p>
      <p className="mt-1 text-xs text-gray-500 line-clamp-2">{data.content || 'No content'}</p>
      <Handle type="target" position={Position.Top} className="!bg-indigo-400" />
      <Handle type="source" position={Position.Bottom} className="!bg-indigo-400" />
    </div>
  )
}

// ─── Graph Conversion ─────────────────────────────────────────────────────────

function graphToFlow(graph: StoryGraphDto): { nodes: Node<StoryNodeData>[]; edges: Edge[] } {
  const nodes = graph.nodes.map((n) => ({
    id: n.id,
    type: 'storyNode',
    position: { x: n.positionX, y: n.positionY },
    data: {
      title: n.title,
      content: n.content,
      isStart: n.isStart,
      isEnd: n.isEnd,
      dbId: n.id,
    },
  }))

  const edges = graph.choices.map((c) => ({
    id: c.id,
    source: c.fromNodeId,
    target: c.toNodeId,
    label: c.label,
    type: 'smoothstep',
    markerEnd: { type: MarkerType.ArrowClosed },
    data: { dbId: c.id, label: c.label, order: c.order },
  }))

  return { nodes, edges }
}

// ─── Editor Skeletons ─────────────────────────────────────────────────────────

function EditorSkeleton() {
  return (
    <div className="flex h-full animate-pulse flex-col">
      <div className="h-14 bg-gray-100 border-b border-gray-200" />
      <div className="flex flex-1">
        <div className="flex-1 bg-gray-50" />
        <div className="w-80 bg-white border-l border-gray-200" />
      </div>
    </div>
  )
}

// ─── Main Editor Page ─────────────────────────────────────────────────────────

function StoryEditor({ storyId }: { storyId: string }) {
  const router = useRouter()
  const isAuthenticated = useAppSelector((s) => s.auth.isAuthenticated)

  const { data: story, isLoading: storyLoading } = useGetStoryQuery(storyId, { skip: !storyId })
  const { data: graph, isLoading: graphLoading } = useGetStoryGraphQuery(storyId, {
    skip: !storyId,
  })

  const [nodes, setNodes, onNodesChange] = useNodesState<Node<StoryNodeData>>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([])
  const [selectedNode, setSelectedNode] = useState<Node<StoryNodeData> | null>(null)
  const [selectedEdge, setSelectedEdge] = useState<Edge | null>(null)
  const [showAiModal, setShowAiModal] = useState(false)
  const [aiPrompt, setAiPrompt] = useState('')
  const [connectingDialog, setConnectingDialog] = useState<{ connection: Connection } | null>(null)
  const [choiceLabel, setChoiceLabel] = useState('')

  // Sidebar local edit state
  const [nodeTitle, setNodeTitle] = useState('')
  const [nodeContent, setNodeContent] = useState('')
  const [nodeIsStart, setNodeIsStart] = useState(false)
  const [nodeIsEnd, setNodeIsEnd] = useState(false)
  const [edgeLabelEdit, setEdgeLabelEdit] = useState('')

  const [createNodeMutation] = useCreateNodeMutation()
  const [updateNodeMutation] = useUpdateNodeMutation()
  const [deleteNodeMutation] = useDeleteNodeMutation()
  const [createChoiceMutation] = useCreateChoiceMutation()
  const [updateChoiceMutation] = useUpdateChoiceMutation()
  const [deleteChoiceMutation] = useDeleteChoiceMutation()
  const [publishStoryMutation] = usePublishStoryMutation()
  const [generateStory, { isLoading: isGenerating }] = useGenerateStoryMutation()
  const [suggestNodeContent, { isLoading: isSuggesting }] = useSuggestNodeContentMutation()

  // Initialise flow when graph loads
  useEffect(() => {
    if (graph) {
      const { nodes: flowNodes, edges: flowEdges } = graphToFlow(graph)
      setNodes(flowNodes)
      setEdges(flowEdges)
    }
  }, [graph, setNodes, setEdges])

  // Sync sidebar fields when selection changes
  useEffect(() => {
    if (selectedNode) {
      setNodeTitle(selectedNode.data.title)
      setNodeContent(selectedNode.data.content)
      setNodeIsStart(selectedNode.data.isStart)
      setNodeIsEnd(selectedNode.data.isEnd)
    }
  }, [selectedNode])

  useEffect(() => {
    if (selectedEdge) {
      setEdgeLabelEdit((selectedEdge.data?.label as string) ?? (selectedEdge.label as string) ?? '')
    }
  }, [selectedEdge])

  const nodeTypes: NodeTypes = useMemo(() => ({ storyNode: StoryNodeComponent }), [])

  // ── Node drag stop: persist position ──────────────────────────────────────

  const onNodeDragStop = useCallback(
    (_: React.MouseEvent, node: Node<StoryNodeData>) => {
      updateNodeMutation({
        storyId,
        nodeId: node.data.dbId,
        title: node.data.title,
        content: node.data.content,
        isStart: node.data.isStart,
        isEnd: node.data.isEnd,
        positionX: node.position.x,
        positionY: node.position.y,
      })
    },
    [storyId, updateNodeMutation],
  )

  // ── New connection: ask for label ─────────────────────────────────────────

  const onConnect = useCallback((connection: Connection) => {
    setConnectingDialog({ connection })
    setChoiceLabel('')
  }, [])

  const confirmConnection = async () => {
    if (!connectingDialog) return
    const { connection } = connectingDialog

    try {
      const newChoiceId = await createChoiceMutation({
        storyId,
        fromNodeId: connection.source!,
        toNodeId: connection.target!,
        label: choiceLabel,
        order: edges.filter((e) => e.source === connection.source).length + 1,
      }).unwrap()

      setEdges((es) =>
        addEdge(
          {
            ...connection,
            id: newChoiceId,
            label: choiceLabel,
            type: 'smoothstep',
            markerEnd: { type: MarkerType.ArrowClosed },
            data: { dbId: newChoiceId, label: choiceLabel, order: 0 },
          },
          es,
        ),
      )
    } catch {
      // ignore – API error handled globally
    }

    setConnectingDialog(null)
    setChoiceLabel('')
  }

  // ── Add node ──────────────────────────────────────────────────────────────

  const handleAddNode = async () => {
    try {
      const newNodeId = await createNodeMutation({
        storyId,
        title: 'New Node',
        content: '',
        isStart: false,
        isEnd: false,
        positionX: Math.random() * 400 + 100,
        positionY: Math.random() * 300 + 100,
      }).unwrap()

      setNodes((ns) => [
        ...ns,
        {
          id: newNodeId,
          type: 'storyNode',
          position: { x: Math.random() * 400 + 100, y: Math.random() * 300 + 100 },
          data: {
            title: 'New Node',
            content: '',
            isStart: false,
            isEnd: false,
            dbId: newNodeId,
          },
        },
      ])
    } catch {
      // ignore
    }
  }

  // ── Update node from sidebar ──────────────────────────────────────────────

  const persistNodeUpdate = useCallback(
    (overrides?: Partial<Pick<StoryNodeData, 'title' | 'content' | 'isStart' | 'isEnd'>>) => {
      if (!selectedNode) return
      const updatedData = {
        title: nodeTitle,
        content: nodeContent,
        isStart: nodeIsStart,
        isEnd: nodeIsEnd,
        ...overrides,
      }
      updateNodeMutation({
        storyId,
        nodeId: selectedNode.data.dbId,
        ...updatedData,
        positionX: selectedNode.position.x,
        positionY: selectedNode.position.y,
      })
      setNodes((ns) =>
        ns.map((n) =>
          n.id === selectedNode.id ? { ...n, data: { ...n.data, ...updatedData } } : n,
        ),
      )
      setSelectedNode((prev) =>
        prev ? { ...prev, data: { ...prev.data, ...updatedData } } : prev,
      )
    },
    [selectedNode, nodeTitle, nodeContent, nodeIsStart, nodeIsEnd, storyId, updateNodeMutation, setNodes],
  )

  // ── Delete node ───────────────────────────────────────────────────────────

  const handleDeleteNode = async () => {
    if (!selectedNode) return
    try {
      await deleteNodeMutation({ storyId, nodeId: selectedNode.data.dbId }).unwrap()
      setNodes((ns) => ns.filter((n) => n.id !== selectedNode.id))
      setSelectedNode(null)
    } catch {
      // ignore
    }
  }

  // ── Suggest content ───────────────────────────────────────────────────────

  const handleSuggestContent = async () => {
    if (!selectedNode) return
    try {
      const suggested = await suggestNodeContent({
        storyId,
        nodeTitle: selectedNode.data.title,
        currentContent: selectedNode.data.content,
      }).unwrap()
      setNodeContent(suggested)
    } catch {
      // ignore
    }
  }

  // ── Update choice label ───────────────────────────────────────────────────

  const persistEdgeUpdate = useCallback(() => {
    if (!selectedEdge?.data?.dbId) return
    updateChoiceMutation({
      storyId,
      choiceId: selectedEdge.data.dbId as string,
      label: edgeLabelEdit,
      order: (selectedEdge.data.order as number) ?? 0,
    })
    setEdges((es) =>
      es.map((e) =>
        e.id === selectedEdge.id
          ? { ...e, label: edgeLabelEdit, data: { ...e.data, label: edgeLabelEdit } }
          : e,
      ),
    )
  }, [selectedEdge, edgeLabelEdit, storyId, updateChoiceMutation, setEdges])

  // ── Delete choice ─────────────────────────────────────────────────────────

  const handleDeleteEdge = async () => {
    if (!selectedEdge?.data?.dbId) return
    try {
      await deleteChoiceMutation({
        storyId,
        choiceId: selectedEdge.data.dbId as string,
      }).unwrap()
      setEdges((es) => es.filter((e) => e.id !== selectedEdge.id))
      setSelectedEdge(null)
    } catch {
      // ignore
    }
  }

  // ── Publish toggle ────────────────────────────────────────────────────────

  const handlePublish = async () => {
    const startNodes = nodes.filter((n) => n.data.isStart)
    if (startNodes.length !== 1) {
      alert('Story must have exactly one Start node before publishing.')
      return
    }
    try {
      await publishStoryMutation({ id: storyId, publish: !story?.isPublished }).unwrap()
    } catch {
      // ignore
    }
  }

  // ── AI Generate ───────────────────────────────────────────────────────────

  const handleAiGenerate = async () => {
    try {
      const newStoryId = await generateStory({ prompt: aiPrompt }).unwrap()
      setShowAiModal(false)
      setAiPrompt('')
      router.push(`/stories/${newStoryId}/edit`)
    } catch {
      // ignore
    }
  }

  // ─── Auth guard ───────────────────────────────────────────────────────────

  if (!isAuthenticated) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-lg font-medium text-gray-600">Not authorised</p>
      </div>
    )
  }

  if (storyLoading || graphLoading) return <EditorSkeleton />

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="flex flex-col" style={{ height: 'calc(100vh - 64px)' }}>
      {/* Toolbar */}
      <div className="flex items-center justify-between border-b border-gray-200 bg-white px-4 py-2 h-14 shrink-0">
        <div className="flex items-center gap-3 min-w-0">
          <Link
            href={`/stories/${storyId}`}
            className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-800 whitespace-nowrap"
          >
            ← Back
          </Link>
          <span className="text-sm font-semibold text-gray-800 truncate max-w-xs">
            {story?.title ?? '…'}
          </span>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <Button variant="secondary" size="sm" onClick={handleAddNode}>
            + Add Node
          </Button>
          <Button
            variant={story?.isPublished ? 'ghost' : 'primary'}
            size="sm"
            onClick={handlePublish}
          >
            {story?.isPublished ? 'Unpublish' : 'Publish'}
          </Button>
          <Button variant="secondary" size="sm" onClick={() => setShowAiModal(true)}>
            ✨ AI Generate
          </Button>
        </div>
      </div>

      {/* Canvas + Sidebar */}
      <div className="flex flex-1 overflow-hidden">
        {/* React Flow Canvas */}
        <div className="flex-1" style={{ height: 'calc(100vh - 120px)' }}>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onNodeClick={(_, node) => {
              setSelectedNode(node as Node<StoryNodeData>)
              setSelectedEdge(null)
            }}
            onEdgeClick={(_, edge) => {
              setSelectedEdge(edge)
              setSelectedNode(null)
            }}
            onPaneClick={() => {
              setSelectedNode(null)
              setSelectedEdge(null)
            }}
            onNodeDragStop={onNodeDragStop}
            nodeTypes={nodeTypes}
            fitView
          >
            <Background />
            <Controls />
            <MiniMap />
          </ReactFlow>
        </div>

        {/* Right Sidebar */}
        <div className="w-80 shrink-0 overflow-y-auto border-l border-gray-200 bg-white p-4">
          {!selectedNode && !selectedEdge && (
            <p className="text-sm text-gray-400">Select a node or connection to edit</p>
          )}

          {/* Node panel */}
          {selectedNode && (
            <div className="space-y-4">
              <h3 className="text-sm font-semibold text-gray-700">Edit Node</h3>

              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">Title</label>
                <input
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  value={nodeTitle}
                  onChange={(e) => setNodeTitle(e.target.value)}
                  onBlur={() => persistNodeUpdate()}
                />
              </div>

              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">Content</label>
                <textarea
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  rows={6}
                  value={nodeContent}
                  onChange={(e) => setNodeContent(e.target.value)}
                  onBlur={() => persistNodeUpdate()}
                />
              </div>

              <div className="flex flex-col gap-2">
                <label className="flex items-center gap-2 text-sm text-gray-700">
                  <input
                    type="checkbox"
                    checked={nodeIsStart}
                    onChange={(e) => {
                      setNodeIsStart(e.target.checked)
                      persistNodeUpdate({ isStart: e.target.checked })
                    }}
                  />
                  Start Node
                </label>
                <label className="flex items-center gap-2 text-sm text-gray-700">
                  <input
                    type="checkbox"
                    checked={nodeIsEnd}
                    onChange={(e) => {
                      setNodeIsEnd(e.target.checked)
                      persistNodeUpdate({ isEnd: e.target.checked })
                    }}
                  />
                  End Node
                </label>
              </div>

              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                isLoading={isSuggesting}
                onClick={handleSuggestContent}
              >
                ✨ Suggest Content
              </Button>

              <Button
                variant="danger"
                size="sm"
                className="w-full"
                onClick={handleDeleteNode}
              >
                Delete Node
              </Button>
            </div>
          )}

          {/* Edge panel */}
          {selectedEdge && (
            <div className="space-y-4">
              <h3 className="text-sm font-semibold text-gray-700">
                Choice:{' '}
                <span className="font-normal text-gray-500">
                  {(selectedEdge.data?.label as string) ?? (selectedEdge.label as string) ?? ''}
                </span>
              </h3>

              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">Label</label>
                <input
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  value={edgeLabelEdit}
                  onChange={(e) => setEdgeLabelEdit(e.target.value)}
                  onBlur={persistEdgeUpdate}
                />
              </div>

              <Button
                variant="danger"
                size="sm"
                className="w-full"
                onClick={handleDeleteEdge}
              >
                Delete Choice
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Connection dialog */}
      {connectingDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-80 rounded-xl bg-white p-6 shadow-xl">
            <h2 className="mb-4 text-base font-semibold text-gray-900">Add Choice Label</h2>
            <input
              autoFocus
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              placeholder="e.g. Go left"
              value={choiceLabel}
              onChange={(e) => setChoiceLabel(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && confirmConnection()}
            />
            <div className="mt-4 flex justify-end gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setConnectingDialog(null)}
              >
                Cancel
              </Button>
              <Button variant="primary" size="sm" onClick={confirmConnection}>
                Add
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* AI Generate modal */}
      {showAiModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-96 rounded-xl bg-white p-6 shadow-xl">
            <h2 className="mb-2 text-base font-semibold text-gray-900">AI Generate Story</h2>
            <p className="mb-4 text-xs text-amber-700 bg-amber-50 rounded-lg px-3 py-2">
              ⚠️ This will replace the current story content with AI-generated nodes and choices.
            </p>
            <textarea
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              rows={4}
              placeholder="Describe the story you want to generate…"
              value={aiPrompt}
              onChange={(e) => setAiPrompt(e.target.value)}
            />
            <div className="mt-4 flex justify-end gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setShowAiModal(false)
                  setAiPrompt('')
                }}
              >
                Cancel
              </Button>
              <Button
                variant="primary"
                size="sm"
                isLoading={isGenerating}
                onClick={handleAiGenerate}
              >
                Generate
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// ─── Page wrapper ─────────────────────────────────────────────────────────────

export default function EditPage({ params }: { params: Promise<{ id: string }> }) {
  const [storyId, setStoryId] = useState<string | null>(null)

  useEffect(() => {
    params.then((p) => setStoryId(p.id))
  }, [params])

  if (!storyId) return <EditorSkeleton />

  return <StoryEditor storyId={storyId} />
}
