import { clearSession, getToken } from '../auth/token'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

export class ApiError extends Error {
  status: number
  rawMessage: string

  constructor(status: number, message: string) {
    super(message)
    this.status = status
    this.rawMessage = message
  }
}

interface ProblemBody {
  detail?: string
  title?: string
  message?: string
  errors?: Array<{ property?: string; errors?: string[] } | string>
}

function extractErrorMessage(body: ProblemBody, fallback: string): string {
  const fromErrors: string[] = []
  if (Array.isArray(body.errors)) {
    for (const item of body.errors) {
      if (typeof item === 'string') {
        fromErrors.push(item)
        continue
      }
      if (item?.errors?.length) fromErrors.push(...item.errors)
    }
  }

  if (fromErrors.length > 0) return fromErrors.join(' ')
  return body.detail ?? body.title ?? body.message ?? fallback
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers)
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json')
  }

  const token = getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  if (response.status === 401) {
    clearSession()
    const path = window.location.pathname
    if (!path.startsWith('/login') && !path.startsWith('/register')) {
      window.location.assign('/login')
    }
    throw new ApiError(401, 'Unauthorized')
  }

  if (!response.ok) {
    let message = response.statusText
    try {
      const body = (await response.json()) as ProblemBody
      message = extractErrorMessage(body, message)
    } catch {
      // ignore parse errors
    }
    throw new ApiError(response.status, message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export interface DownloadedFile {
  blob: Blob
  fileName: string
  contentType: string
}

/** Authenticated binary download (PDF and future export formats). */
export async function apiDownload(path: string, options: RequestInit = {}): Promise<DownloadedFile> {
  const headers = new Headers(options.headers)
  const token = getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  if (response.status === 401) {
    clearSession()
    const currentPath = window.location.pathname
    if (!currentPath.startsWith('/login') && !currentPath.startsWith('/register')) {
      window.location.assign('/login')
    }
    throw new ApiError(401, 'Unauthorized')
  }

  if (!response.ok) {
    let message = response.statusText
    try {
      const body = (await response.json()) as ProblemBody
      message = extractErrorMessage(body, message)
    } catch {
      // ignore parse errors
    }
    throw new ApiError(response.status, message)
  }

  const blob = await response.blob()
  const disposition = response.headers.get('Content-Disposition') ?? ''
  const match = /filename\*?=(?:UTF-8''|")?([^\";]+)/i.exec(disposition)
  const fileName = match?.[1] ? decodeURIComponent(match[1].replace(/"/g, '')) : 'download'
  const contentType = response.headers.get('Content-Type') ?? blob.type

  return { blob, fileName, contentType }
}

/** Multipart upload — do not set Content-Type (browser sets boundary). */
export async function apiUpload<T>(path: string, formData: FormData): Promise<T> {
  const headers = new Headers()
  const token = getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers,
    body: formData,
  })

  if (response.status === 401) {
    clearSession()
    const currentPath = window.location.pathname
    if (!currentPath.startsWith('/login') && !currentPath.startsWith('/register')) {
      window.location.assign('/login')
    }
    throw new ApiError(401, 'Unauthorized')
  }

  if (!response.ok) {
    let message = response.statusText
    try {
      const body = (await response.json()) as ProblemBody
      message = extractErrorMessage(body, message)
    } catch {
      // ignore parse errors
    }
    throw new ApiError(response.status, message)
  }

  return (await response.json()) as T
}
