import { ApiError } from "./errors";

export interface Envelope<T = unknown> {
  result?: T | null;
  errors: ApiError[];
  timeGenerated: Date;
}
