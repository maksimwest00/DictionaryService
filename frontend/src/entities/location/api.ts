import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse } from "./types";
import { PagedResult } from "@/shared/api/types";

export type GetLocationsByFiltersQuery = {
  search?: string;
  minDepartmentCount?: number;
  sortBy?: string;
  sortDir?: string;
  page?: number;
  pageSize?: number;
};

export const locationsApi = {
  getLocations: (
    request: GetLocationsByFiltersQuery,
    signal?: AbortSignal,
  ): Promise<PagedResult<LocationListItemResponse>> =>
    apiClient.get<PagedResult<LocationListItemResponse>>("/locations", {
      params: request,
      signal,
    }),
};
