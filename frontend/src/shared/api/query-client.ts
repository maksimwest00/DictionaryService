import { QueryClient } from "@tanstack/react-query";
import type { ApiError } from "./types";

// Повторяем только временные сбои (FAILURE / сеть). Бизнес-ошибки
// (VALIDATION, NOT_FOUND, CONFLICT) повторять бессмысленно — ответ не изменится.
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= 2) return false;

  const type = (error as Partial<ApiError> | null)?.type;
  if (type) return type === "FAILURE";

  // Ошибка без типа — скорее всего сетевая (нет ответа от backend'а)
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
