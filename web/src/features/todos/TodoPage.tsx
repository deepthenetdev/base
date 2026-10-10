import { useState, type FormEvent } from 'react'
import { ApiError } from '../../api/client'
import { useCreateTodo, useDeleteTodo, useTodos, useUpdateTodo } from '../../api/todos'

export function TodoPage() {
  const todos = useTodos()
  const createTodo = useCreateTodo()
  const updateTodo = useUpdateTodo()
  const deleteTodo = useDeleteTodo()
  const [title, setTitle] = useState('')

  const titleError =
    createTodo.error instanceof ApiError ? createTodo.error.fieldError('title') : undefined

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    createTodo.mutate(title, { onSuccess: () => setTitle('') })
  }

  return (
    <section className="card" aria-labelledby="todos-heading">
      <h2 id="todos-heading">Todos</h2>
      <p className="muted">
        A sample feature that exercises the full stack: React → .NET API → Valkey cache → PostgreSQL.
      </p>

      <form className="add-form" onSubmit={handleSubmit} noValidate>
        <label htmlFor="new-todo" className="visually-hidden">
          New todo
        </label>
        <input
          id="new-todo"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="What needs doing?"
          maxLength={200}
          aria-invalid={titleError ? true : undefined}
          aria-describedby={titleError ? 'new-todo-error' : undefined}
        />
        <button type="submit" disabled={createTodo.isPending}>
          {createTodo.isPending ? 'Adding…' : 'Add'}
        </button>
      </form>
      {titleError && (
        <p id="new-todo-error" className="error" role="alert">
          {titleError}
        </p>
      )}

      {todos.isPending && <p className="muted">Loading…</p>}

      {todos.isError && (
        <p className="error" role="alert">
          Couldn&apos;t load todos. Is the API running? ({todos.error.message})
        </p>
      )}

      {todos.isSuccess && todos.data.length === 0 && <p className="muted">Nothing here yet. Add your first todo.</p>}

      {todos.isSuccess && todos.data.length > 0 && (
        <ul className="todo-list">
          {todos.data.map((todo) => (
            <li key={todo.id} className={todo.isDone ? 'done' : undefined}>
              <label>
                <input
                  type="checkbox"
                  checked={todo.isDone}
                  onChange={() => updateTodo.mutate({ id: todo.id, changes: { isDone: !todo.isDone } })}
                />
                <span>{todo.title}</span>
              </label>
              <button
                type="button"
                className="ghost"
                onClick={() => deleteTodo.mutate(todo.id)}
                aria-label={`Delete ${todo.title}`}
              >
                Delete
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
