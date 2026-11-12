// Matches backend contract: DictionaryService.Contracts.Shared.PagedResult<T>
export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}
