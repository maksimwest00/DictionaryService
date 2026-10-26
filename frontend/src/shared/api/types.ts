export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface Envelope<T> {
  result?: T | null;
  // Backend сериализует Errors через ErrorsJsonConverter как массив
  errors: ApiError[];
  timeGenerated: Date;
}

export interface ApiError {
  code: string;
  messages: string[];
  type: ErrorType;
  invalidField?: string | null;
}

export type ErrorType = "VALIDATION" | "NOT_FOUND" | "FAILURE" | "CONFLICT";
