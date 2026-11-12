import { UseFormSetError } from "react-hook-form";
import { toast } from "sonner";
import { EnvelopeError } from "@/shared/api/errors";
import { CreateLocationFormValues } from "@/entities/location";

type ErrorFieldMap = Record<string, string>;

export function handleFormError(
  error: Error,
  setError: UseFormSetError<CreateLocationFormValues>,
  fieldMap: ErrorFieldMap,
) {
  if (!(error instanceof EnvelopeError)) {
    toast.error(error instanceof Error ? error.message : "Неизвестная ошибка");
    return;
  }

  const apiErrors = error.apiErrors;

  for (const apiError of apiErrors) {
    for (const message of apiError.messages) {
      const fieldName = fieldMap[message] as keyof CreateLocationFormValues;
      if (fieldName) {
        setError(fieldName, {
          type: "server",
          message,
        });
      } else {
        toast.error(message);
      }
    }
  }
}
