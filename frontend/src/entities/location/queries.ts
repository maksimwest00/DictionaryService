import { keepPreviousData, queryOptions } from "@tanstack/react-query";
import { locationsApi, GetLocationsByFiltersQuery } from "./api";

// queryKey — часть контракта: инвалидация должна использовать тот же ключ.
export const locationQueries = {
  list: (filters: GetLocationsByFiltersQuery = {}) =>
    queryOptions({
      queryKey: ["locations", "list", filters] as const,
      queryFn: ({ signal }) => locationsApi.getLocations(filters, signal),
      // При смене страницы показываем предыдущие данные, а не скелетон
      placeholderData: keepPreviousData,
    }),
};
