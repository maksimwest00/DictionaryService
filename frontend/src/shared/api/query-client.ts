import { QueryClient } from "@tanstack/react-query";
import { ApiError } from "./errors";

function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= 2) return false;

  const type = (error as Partial<ApiError> | null)?.type;
  if (type) return type === "FAILURE";

  return true;
}

export function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
        refetchOnWindowFocus: false,
        retry: shouldRetry,
      },
    },
  });
}

let browserQueryClient: QueryClient | undefined = undefined;

export function getQueryClient() {
  if (typeof window === "undefined") {
    return makeQueryClient();
  } else {
    if (!browserQueryClient) browserQueryClient = makeQueryClient();
    return browserQueryClient;
  }
}
