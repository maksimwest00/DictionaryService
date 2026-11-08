import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { locationsApi } from "@/entities/location/api";
import type { CreateLocationFormValues } from "@/entities/location/schemas";
import { locationQueries } from "@/entities/location/queries";

export function useCreateLocation(options?: { onSuccess?: () => void }) {
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
      toast.error(error.message);
    },
  });
}
