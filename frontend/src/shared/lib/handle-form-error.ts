import {FieldValues, UseFormSetError, Path} from "react-hook-form";
import { toast } from "sonner";
import { EnvelopeError } from "@/shared/api/errors";

type ErrorFieldMap = Record<string, string>;

export function handleFormError<T extends FieldValues>(
  error: Error,
  setError: UseFormSetError<T>,
  fieldMap: ErrorFieldMap,
) {
  if (!(error instanceof EnvelopeError)) {
    toast.error(error instanceof Error ? error.message : "Неизвестная ошибка");
    return;
  }

  const apiErrors = error.apiErrors;

  for (const apiError of apiErrors) {
    for (const message of apiError.messages) {
      const fieldName = fieldMap[message] as Path<T>;
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
