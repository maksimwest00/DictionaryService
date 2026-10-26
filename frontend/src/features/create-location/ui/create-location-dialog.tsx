"use client";

import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2Icon } from "lucide-react";

import { Button } from "@/shared/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/ui/dialog";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import {
  createLocationSchema,
  type CreateLocationFormValues,
} from "@/entities/location/schemas";
import { useCreateLocation } from "../model/use-create-location";

const TIMEZONES = [
  "Europe/Moscow",
  "Europe/Kaliningrad",
  "Europe/Samara",
  "Asia/Yekaterinburg",
  "Asia/Novosibirsk",
  "Asia/Krasnoyarsk",
  "Asia/Irkutsk",
  "Asia/Yakutsk",
  "Asia/Vladivostok",
];

const EMPTY_VALUES: CreateLocationFormValues = {
  name: "",
  address: { city: "", street: "", building: "", roomNumber: "" },
  timezone: "Europe/Moscow",
};

/** Поле формы с подписью и ошибкой валидации. */
function Field({
  label,
  htmlFor,
  error,
  children,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1.5">
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

/**
 * Диалог создания локации.
 * UI не знает про HTTP: собирает валидные данные и вызывает мутацию.
 */
export function CreateLocationDialog() {
  const [open, setOpen] = useState(false);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateLocationFormValues>({
    resolver: zodResolver(createLocationSchema),
    defaultValues: EMPTY_VALUES,
  });

  const mutation = useCreateLocation({
    onSuccess: () => {
      setOpen(false);
    },
  });

  // При любом закрытии форма возвращается к пустому состоянию
  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (!next) {
      reset(EMPTY_VALUES);
      mutation.reset();
    }
  };

  const onSubmit = handleSubmit((values) => {
    mutation.mutate(values);
  });

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={<Button />}>Создать локацию</DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Новая локация</DialogTitle>
          <DialogDescription>
            Заполните название и адрес — поля обязательны.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={onSubmit} className="flex flex-col gap-4" noValidate>
          <Controller
            control={control}
            name="name"
            render={({ field }) => (
              <Field
                label="Название"
                htmlFor="location-name"
                error={errors.name?.message}
              >
                <Input
                  id="location-name"
                  placeholder="Например, Москва"
                  aria-invalid={!!errors.name}
                  {...field}
                />
              </Field>
            )}
          />

          <div className="grid grid-cols-2 gap-3">
            <Controller
              control={control}
              name="address.city"
              render={({ field }) => (
                <Field
                  label="Город"
                  htmlFor="location-city"
                  error={errors.address?.city?.message}
                >
                  <Input
                    id="location-city"
                    aria-invalid={!!errors.address?.city}
                    {...field}
                  />
                </Field>
              )}
            />

            <Controller
              control={control}
              name="address.street"
              render={({ field }) => (
                <Field
                  label="Улица"
                  htmlFor="location-street"
                  error={errors.address?.street?.message}
                >
                  <Input
                    id="location-street"
                    aria-invalid={!!errors.address?.street}
                    {...field}
                  />
                </Field>
              )}
            />

            <Controller
              control={control}
              name="address.building"
              render={({ field }) => (
                <Field
                  label="Дом"
                  htmlFor="location-building"
                  error={errors.address?.building?.message}
                >
                  <Input
                    id="location-building"
                    aria-invalid={!!errors.address?.building}
                    {...field}
                  />
                </Field>
              )}
            />

            <Controller
              control={control}
              name="address.roomNumber"
              render={({ field }) => (
                <Field
                  label="Помещение"
                  htmlFor="location-room"
                  error={errors.address?.roomNumber?.message}
                >
                  <Input
                    id="location-room"
                    aria-invalid={!!errors.address?.roomNumber}
                    {...field}
                  />
                </Field>
              )}
            />
          </div>

          <Controller
            control={control}
            name="timezone"
            render={({ field }) => (
              <Field
                label="Часовой пояс"
                htmlFor="location-timezone"
                error={errors.timezone?.message}
              >
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="location-timezone" className="w-full">
                    <SelectValue placeholder="Выберите часовой пояс" />
                  </SelectTrigger>
                  <SelectContent>
                    {TIMEZONES.map((timezone) => (
                      <SelectItem key={timezone} value={timezone}>
                        {timezone}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          <DialogFooter className="-mx-4 -mb-4 mt-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              disabled={mutation.isPending}
            >
              Отмена
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending && (
                <Loader2Icon
                  className="animate-spin"
                  data-icon="inline-start"
                />
              )}
              {mutation.isPending ? "Создаём…" : "Создать"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
