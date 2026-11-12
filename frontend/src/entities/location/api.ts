import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse, CreateLocationRequest } from "./schemas";
import { PagedResult } from "@/shared/api/paged-result";

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
  ): Promise<PagedResult<LocationListItemResponse>> => {
    return apiClient.get<PagedResult<LocationListItemResponse>>("/locations", {
      params: request,
    });
  },

  createLocation: (request: CreateLocationRequest): Promise<string> => {
    return apiClient.post<string>("/locations", request);
  },
};
