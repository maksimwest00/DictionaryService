import { queryOptions } from "@tanstack/react-query";
import { departmentsApi, GetDepartmentsByFiltersQuery } from "./api";

export const departmentQueries = {
  list: (filters: GetDepartmentsByFiltersQuery = {}) =>
    queryOptions({
      queryKey: ["departments", "list", filters] as const,
      queryFn: () => departmentsApi.getDepartments(filters),
    }),
};
