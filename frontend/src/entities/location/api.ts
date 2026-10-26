import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse } from "./types";
import { Envelope, PagedResult } from "@/shared/api/types";

export type GetLocationsByFiltersQuery = {
  search?: string;
  minDepartmentCount?: number;
  sortBy?: string;
  sortDir?: string;
  page?: number;
  pageSize?: number;
};

export const locationsApi = {
  getLocations: async (
    request: GetLocationsByFiltersQuery,
    signal?: AbortSignal,
  ): Promise<LocationListItemResponse[]> => {
    const response = await apiClient.get<
      Envelope<PagedResult<LocationListItemResponse>>
    >("/locations", {
      params: request,
      signal,
    });

    return response.data.result?.data || [];
  },
};
