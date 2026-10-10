import { TodoPage } from './features/todos/TodoPage'

export function App() {
  return (
    <div className="app">
      <header className="app-header">
        <h1>Base</h1>
        <p className="muted">.NET 10 · React · PostgreSQL · Valkey · Docker</p>
      </header>
      <main>
        <TodoPage />
      </main>
    </div>
  )
}
