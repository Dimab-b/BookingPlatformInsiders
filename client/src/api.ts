import type { BookingDto, PagedResult, ProblemDetails, RoomDto } from './types';

const baseUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5178').replace(/\/$/, '');

export class ApiError extends Error {
  readonly status: number;
  readonly problem?: ProblemDetails;

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.title ?? `Request failed with status ${status}`);
    this.status = status;
    this.problem = problem;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {})
    }
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => undefined) as ProblemDetails | undefined;
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return await response.json() as T;
}

export interface CreateBookingRequest {
  roomId: string;
  title: string;
  startUtc: string;
  endUtc: string;
}

export const api = {
  getRooms: () => request<PagedResult<RoomDto>>('/api/rooms?pageNumber=1&pageSize=100'),

  getSchedule: (roomId: string, date: string) =>
    request<PagedResult<BookingDto>>(
      `/api/rooms/${roomId}/bookings?date=${date}&pageNumber=1&pageSize=100`),

  createBooking: (booking: CreateBookingRequest) =>
    request<BookingDto>('/api/bookings', {
      method: 'POST',
      body: JSON.stringify(booking)
    }),

  cancelBooking: (id: string) =>
    request<void>(`/api/bookings/${id}/cancel`, { method: 'PATCH' })
};

export function describeError(error: unknown): string {
  if (error instanceof ApiError) {
    const parts = [error.problem?.code, error.problem?.title].filter(Boolean);
    const fieldErrors = error.problem?.errors
      ? Object.entries(error.problem.errors)
          .flatMap(([field, messages]) => messages.map(message => `${field}: ${message}`))
      : [];

    return [...parts, ...fieldErrors].join(' | ') || error.message;
  }

  return error instanceof Error ? error.message : 'Unknown error';
}
