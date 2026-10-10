import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { request } from './client'

// These types mirror the C# contracts in src/Modules/Todos/Contracts/TodoContracts.cs.
// The API also publishes an OpenAPI document at /openapi/v1.json, which can generate them instead.
export interface Todo {
  id: string
  title: string
  isDone: boolean
  createdAt: string
  completedAt: string | null
}

export interface UpdateTodo {
  title?: string
  isDone?: boolean
}

const todosKey = ['todos'] as const

export const todosApi = {
  list: () => request<Todo[]>('/api/todos'),
  create: (title: string) =>
    request<Todo>('/api/todos', { method: 'POST', body: JSON.stringify({ title }) }),
  update: (id: string, changes: UpdateTodo) =>
    request<Todo>(`/api/todos/${id}`, { method: 'PATCH', body: JSON.stringify(changes) }),
  remove: (id: string) => request<void>(`/api/todos/${id}`, { method: 'DELETE' }),
}

export function useTodos() {
  return useQuery({ queryKey: todosKey, queryFn: todosApi.list })
}

export function useCreateTodo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: todosApi.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: todosKey }),
  })
}

export function useUpdateTodo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, changes }: { id: string; changes: UpdateTodo }) => todosApi.update(id, changes),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: todosKey }),
  })
}

export function useDeleteTodo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: todosApi.remove,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: todosKey }),
  })
}
