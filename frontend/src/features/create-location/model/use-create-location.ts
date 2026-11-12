import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { locationsApi } from "@/entities/location/api";
import type { CreateLocationFormValues } from "@/entities/location/schemas";
import { locationQueries } from "@/entities/location/queries";
import { handleFormError } from "@/shared/lib/handle-form-error";
import { LOCATION_ERROR_FIELD_MAP } from "../error-field-map";
import { UseFormSetError } from "react-hook-form";

export function useCreateLocation(options?: {
  onSuccess?: () => void;
  setError?: UseFormSetError<CreateLocationFormValues>;
}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: CreateLocationFormValues) =>
      locationsApi.createLocation(values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: locationQueries.list().queryKey.slice(0, 2),
      });
      toast.success("Локация создана");
      options?.onSuccess?.();
    },
    onError: (error) => {
      if (options?.setError) {
        handleFormError(error, options.setError, LOCATION_ERROR_FIELD_MAP);
      } else {
        toast.error(
          error instanceof Error ? error.message : "Неизвестная ошибка",
        );
      }
    },
  });
}
