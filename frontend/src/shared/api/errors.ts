export interface ApiError {
  code: string;
  messages: string[];
  type: ErrorType;
  invalidField?: string | null;
}

export type ErrorType = "VALIDATION" | "NOT_FOUND" | "FAILURE" | "CONFLICT";

export class EnvelopeError extends Error {
  public readonly apiErrors: ApiError[];

  constructor(errors: ApiError[]) {
    super(errors[0].messages[0]);

    this.apiErrors = errors;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }
}
