import { QueryClient } from "@tanstack/react-query";
import { ApiError } from "./errors";

function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= 2) return false;

  const type = (error as Partial<ApiError> | null)?.type;
  if (type) return type === "FAILURE";

  return true;
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      refetchOnWindowFocus: false,
      retry: shouldRetry,
    },
  },
});
