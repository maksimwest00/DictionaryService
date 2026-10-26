export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface Envelope<T> {
  result?: T | null;
  errors: Errors;
  timeGenerated: Date;
}

export interface Errors {
  items: Error[];
}

export interface ApiError {
  code: string;
  messages: string[];
  type: ErrorType;
  invalidField?: string | null;
}

export type ErrorType = "VALIDATION" | "NOT_FOUND" | "FAILURE" | "CONFLICT";
