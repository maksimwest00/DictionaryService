import { infiniteQueryOptions } from "@tanstack/react-query";
import { positionsApi, GetPositionsQuery } from "./api";

export const positionsQueryKey = ["positions"] as const;

export const positionsInfiniteQueryOptions = (filters: GetPositionsQuery = {}) =>
  infiniteQueryOptions({
    queryKey: [...positionsQueryKey, "list", filters] as const,
    queryFn: ({ pageParam }) =>
      positionsApi.getPositions({ ...filters, cursor: pageParam }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  });
