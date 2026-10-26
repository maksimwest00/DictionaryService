import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { locationsApi } from "@/entities/location/api";
import type { CreateLocationFormValues } from "@/entities/location/schemas";
import { locationQueries } from "@/entities/location/queries";

/**
 * Мутация создания локации.
 * Здесь и только здесь живут детали: вызов API, инвалидация кэша и тосты.
 * UI-форма просто вызывает mutate(values).
 */
export function useCreateLocation(options?: { onSuccess?: () => void }) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: CreateLocationFormValues) =>
      locationsApi.createLocation(values),
    onSuccess: async () => {
      // Инвалидируем все списки локаций, чтобы новая появилась без перезагрузки
      await queryClient.invalidateQueries({
        queryKey: locationQueries.list().queryKey.slice(0, 2),
      });
      toast.success("Локация создана");
      options?.onSuccess?.();
    },
    onError: (error) => {
      // Не показываем пользователю "Error: undefined" — только осмысленный текст
      const message =
        error instanceof Error && error.message
          ? error.message
          : "Не удалось создать локацию";
      toast.error(message);
    },
  });
}
