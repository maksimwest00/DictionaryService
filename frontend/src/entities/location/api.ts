import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse } from "./types";
import { PagedResult } from "@/shared/api/paged-result";
import { Envelope } from "@/shared/api/envelope";

export type CreateLocationRequest = {
  name: string;
  address: {
    city: string;
    street: string;
    building: string;
    roomNumber: string;
  };
  timezone: string;
};

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
    return apiClient
      .get<Envelope<PagedResult<LocationListItemResponse>>>("/locations", {
        params: request,
      })
      .then((envelope) => envelope.result!);
  },

  createLocation: (
    request: CreateLocationRequest,
  ): Promise<Envelope<string>> => {
    return apiClient.post<Envelope<string>>("/locations", request);
  },
};
