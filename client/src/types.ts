export interface UserDto {
  id: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresInSeconds: number;
  user: UserDto;
}

export interface CredentialsRequest {
  email: string;
  password: string;
}

export interface RoomDto {
  id: string;
  name: string;
  capacity: number;
  location: string;
  description: string | null;
  isPaid: boolean;
}

export interface BookingDto {
  id: string;
  roomId: string;
  title: string;
  startUtc: string;
  endUtc: string;
  status: string;
  isOwnedByCurrentUser: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ProblemDetails {
  title?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}
