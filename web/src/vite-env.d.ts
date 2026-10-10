/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Optional absolute API URL. Leave unset to call the same origin (recommended). */
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
