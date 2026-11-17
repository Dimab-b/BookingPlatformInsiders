import type {
  AuthResponse,
  BookingDto,
  CredentialsRequest,
  PagedResult,
  ProblemDetails,
  RoomDto
} from './types';

const baseUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5178').replace(/\/$/, '');

let accessToken: string | null = null;
let refreshPromise: Promise<AuthResponse> | null = null;
let sessionExpiredHandler: (() => void) | null = null;

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

export function setSessionExpiredHandler(handler: (() => void) | null): void {
  sessionExpiredHandler = handler;
}

export class ApiError extends Error {
  readonly status: number;
  readonly problem?: ProblemDetails;

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.title ?? `Request failed with status ${status}`);
    this.status = status;
    this.problem = problem;
  }
}

interface RequestOptions {
  auth?: boolean;
  retryOnUnauthorized?: boolean;
  includeCredentials?: boolean;
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  return await response.json().catch(() => undefined) as ProblemDetails | undefined;
}

async function rawRequest<T>(path: string, init: RequestInit, options: RequestOptions): Promise<T> {
  const headers = new Headers(init.headers);

  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (options.auth !== false && accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers,
    credentials: options.includeCredentials ? 'include' : 'same-origin'
  });

  if (response.status === 401 && options.auth !== false && options.retryOnUnauthorized !== false) {
    const refreshed = await tryRefreshSession();

    if (refreshed) {
      return rawRequest<T>(path, init, { ...options, retryOnUnauthorized: false });
    }
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return await response.json() as T;
}

async function performRefresh(): Promise<AuthResponse> {
  const response = await fetch(`${baseUrl}/api/auth/refresh`, {
    method: 'POST',
    credentials: 'include'
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }

  const result = await response.json() as AuthResponse;
  setAccessToken(result.accessToken);

  return result;
}

async function tryRefreshSession(): Promise<boolean> {
  try {
    await refreshSession();
    return true;
  } catch {
    setAccessToken(null);
    sessionExpiredHandler?.();
    return false;
  }
}

export function refreshSession(): Promise<AuthResponse> {
  if (!refreshPromise) {
    refreshPromise = performRefresh().finally(() => {
      refreshPromise = null;
    });
  }

  return refreshPromise;
}

export async function register(request: CredentialsRequest): Promise<AuthResponse> {
  const result = await rawRequest<AuthResponse>(
    '/api/auth/register',
    { method: 'POST', body: JSON.stringify(request) },
    { auth: false, includeCredentials: true });

  setAccessToken(result.accessToken);

  return result;
}

export async function login(request: CredentialsRequest): Promise<AuthResponse> {
  const result = await rawRequest<AuthResponse>(
    '/api/auth/login',
    { method: 'POST', body: JSON.stringify(request) },
    { auth: false, includeCredentials: true });

  setAccessToken(result.accessToken);

  return result;
}

export async function logout(): Promise<void> {
  try {
    await rawRequest<void>(
      '/api/auth/logout',
      { method: 'POST' },
      { auth: false, includeCredentials: true });
  } finally {
    setAccessToken(null);
  }
}

export async function logoutAll(): Promise<void> {
  try {
    await rawRequest<void>(
      '/api/auth/logout-all',
      { method: 'POST' },
      { auth: true, includeCredentials: true });
  } finally {
    setAccessToken(null);
  }
}

export interface CreateBookingRequest {
  roomId: string;
  title: string;
  startUtc: string;
  endUtc: string;
}

const authorized = { auth: true } as const;

export const api = {
  getRooms: () =>
    rawRequest<PagedResult<RoomDto>>('/api/rooms?pageNumber=1&pageSize=100', {}, authorized),

  getSchedule: (roomId: string, date: string) =>
    rawRequest<PagedResult<BookingDto>>(
      `/api/rooms/${roomId}/bookings?date=${date}&pageNumber=1&pageSize=100`,
      {},
      authorized),

  createBooking: (booking: CreateBookingRequest) =>
    rawRequest<BookingDto>(
      '/api/bookings',
      { method: 'POST', body: JSON.stringify(booking) },
      authorized),

  cancelBooking: (id: string) =>
    rawRequest<void>(`/api/bookings/${id}/cancel`, { method: 'PATCH' }, authorized)
};

export function describeError(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return error instanceof Error ? error.message : 'Unknown error';
  }

  const code = error.problem?.code;
  const fieldErrors = error.problem?.errors
    ? Object.entries(error.problem.errors)
        .flatMap(([field, messages]) => messages.map(message => `${field}: ${message}`))
    : [];

  const friendlyMessage = getFriendlyMessage(code);

  if (friendlyMessage) {
    return [friendlyMessage, ...fieldErrors].join(' ');
  }

  const parts = [code, error.problem?.title].filter(Boolean);

  return [...parts, ...fieldErrors].join(' | ') || error.message;
}

function getFriendlyMessage(code: string | undefined): string | null {
  switch (code) {
    case 'auth.invalid_credentials':
      return 'Invalid email or password.';
    case 'auth.email_taken':
      return 'This email is already registered. Try signing in instead.';
    case 'auth.invalid_token':
    case 'auth.session_expired':
      return 'Your session has expired. Please sign in again.';
    case 'booking.slot_taken':
      return 'That time slot overlaps an existing booking. Pick another time.';
    case 'booking.start_in_past':
      return 'The booking must start in the future.';
    case 'booking.cannot_cancel':
      return 'A completed booking cannot be cancelled.';
    case 'booking.not_owned':
      return 'You can only cancel your own bookings.';
    case 'booking.not_found':
      return 'That booking no longer exists.';
    case 'room.not_found':
      return 'That room no longer exists.';
    case 'validation.failed':
      return 'Please fix the highlighted fields.';
    default:
      return null;
  }
}
