import { apiClient } from "@/shared/api/axios-instance";
import { PagedResult } from "@/shared/api/types";
import { DepartmentListItemResponse } from "./types";

export type GetDepartmentsByFiltersQuery = {
  search?: string;
  sortBy?: string;
  sortDir?: string;
  page?: number;
  pageSize?: number;
};

export const departmentsApi = {
  getDepartments: (
    request: GetDepartmentsByFiltersQuery,
    signal?: AbortSignal,
  ): Promise<PagedResult<DepartmentListItemResponse>> =>
    apiClient.get<PagedResult<DepartmentListItemResponse>>("/departments", {
      params: request,
      signal,
    }),
};
