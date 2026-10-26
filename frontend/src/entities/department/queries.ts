import { keepPreviousData, queryOptions } from "@tanstack/react-query";
import { departmentsApi, GetDepartmentsByFiltersQuery } from "./api";

// queryKey — часть контракта: инвалидация должна использовать тот же ключ.
export const departmentQueries = {
  list: (filters: GetDepartmentsByFiltersQuery = {}) =>
    queryOptions({
      queryKey: ["departments", "list", filters] as const,
      queryFn: ({ signal }) => departmentsApi.getDepartments(filters, signal),
      // При смене страницы показываем предыдущие данные, а не скелетон
      placeholderData: keepPreviousData,
    }),
};
