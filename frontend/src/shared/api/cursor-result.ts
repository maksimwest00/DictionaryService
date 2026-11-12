// Matches backend contract: DictionaryService.Contracts.Shared.CursorResult<T>
export interface CursorResult<T> {
  data: T[];
  nextCursor: string | null;
}
