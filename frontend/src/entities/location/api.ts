import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse } from "./types";
import { PagedResult } from "@/shared/api/types";

/**
 * Контракт POST /locations (DS-4):
 * { name, address: { city, street, building, roomNumber }, timezone }
 * В ответ приходит id созданной локации (Guid).
 */
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
    signal?: AbortSignal,
  ): Promise<PagedResult<LocationListItemResponse>> =>
    apiClient.get<PagedResult<LocationListItemResponse>>("/locations", {
      params: request,
      signal,
    }),

  // Возвращает id созданной локации; интерсептор уже развернул envelope
  createLocation: (request: CreateLocationRequest): Promise<string> =>
    apiClient.post<string>("/locations", request),
};
