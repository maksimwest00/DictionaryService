import { apiClient } from "@/shared/api/axios-instance";
import { PagedResult } from "@/shared/api/paged-result";
import { DepartmentListItemResponse } from "./types";
import { Envelope } from "@/shared/api/envelope";

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
  ): Promise<PagedResult<DepartmentListItemResponse>> =>
    apiClient
      .get<Envelope<PagedResult<DepartmentListItemResponse>>>("/departments", {
        params: request,
      })
      .then((envelope) => envelope.result!),
};
