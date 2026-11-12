import { queryOptions } from "@tanstack/react-query";
import { locationsApi, GetLocationsByFiltersQuery } from "./api";

export const locationQueries = {
  list: (filters: GetLocationsByFiltersQuery = {}) =>
    queryOptions({
      queryKey: ["locations", "list", filters] as const,
      queryFn: () => locationsApi.getLocations(filters),
    }),
};
