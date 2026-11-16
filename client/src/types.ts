export interface RoomDto {
  id: string;
  name: string;
  capacity: number;
  location: string;
  description: string | null;
}

export interface BookingDto {
  id: string;
  roomId: string;
  title: string;
  startUtc: string;
  endUtc: string;
  status: string;
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
