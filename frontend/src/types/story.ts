export interface Story {
  id: string
  title: string
  description: string
  authorUsername: string
  isPublished: boolean
  startNodeId: string
  coverImageUrl?: string
  nodeCount: number
  averageRating: number
  createdAt: string
}

export interface StoryNode {
  id: string
  title: string
  content: string
  isEndNode: boolean
  choices: Choice[]
}

export interface Choice {
  id: string
  text: string
  toNodeId: string
}

export interface StoryGraph {
  storyId: string
  startNodeId: string
  nodes: StoryNode[]
}

export interface Progress {
  storyId: string
  currentNodeId: string
  isCompleted: boolean
}
