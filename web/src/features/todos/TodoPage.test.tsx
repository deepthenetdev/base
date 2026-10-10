import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { Todo } from '../../api/todos'
import { TodoPage } from './TodoPage'

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <TodoPage />
    </QueryClientProvider>,
  )
}

const sample: Todo = {
  id: '0199c1a2-0000-7000-8000-000000000001',
  title: 'Write the README',
  isDone: false,
  createdAt: '2026-10-09T12:00:00Z',
  completedAt: null,
}

describe('TodoPage', () => {
  it('shows todos from the API', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => jsonResponse([sample])))

    renderPage()

    expect(await screen.findByText('Write the README')).toBeInTheDocument()
  })

  it('shows the server validation message when a todo is rejected', async () => {
    const fetchMock = vi.fn(async (_url: string, init?: RequestInit) => {
      if (init?.method === 'POST') {
        return jsonResponse({ title: 'One or more validation errors occurred.', errors: { title: ['Title is required.'] } }, 400)
      }
      return jsonResponse([])
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText(/Nothing here yet/)
    await user.type(screen.getByLabelText('New todo'), '   ')
    await user.click(screen.getByRole('button', { name: 'Add' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Title is required.')
  })
})
