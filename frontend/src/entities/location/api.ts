import { apiClient } from "@/shared/api/axios-instance";
import { LocationListItemResponse } from "./types";
import { PagedResult } from "@/shared/api/paged-result";
import axios from "axios";
import { Envelope } from "@/shared/api/envelope";
import { EnvelopeError } from "@/shared/api/errors";

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
  ): Promise<PagedResult<LocationListItemResponse>> => {
    try {
      return apiClient.get<PagedResult<LocationListItemResponse>>(
        "/locations",
        {
          params: request,
          signal,
        },
      );
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data) {
        const envelope = error.response.data as Envelope;
        if (envelope?.errors.length > 0) {
          throw new EnvelopeError(envelope.errors);
        }
      }
      throw error;
    }
  },

  createLocation: (
    request: CreateLocationRequest,
  ): Promise<Envelope<string>> => {
    try {
      return apiClient.post<Envelope<string>>("/locations", request);
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data) {
        const envelope = error.response.data as Envelope;
        if (envelope?.errors.length > 0) {
          throw new EnvelopeError(envelope.errors);
        }
      }
      throw error;
    }
  },
};
