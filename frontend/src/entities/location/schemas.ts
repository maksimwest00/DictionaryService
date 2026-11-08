import { z } from "zod";

export const createLocationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно быть не короче 3 символов")
    .max(120, "Название должно быть не длиннее 120 символов"),
  address: z.object({
    city: z.string().trim().min(1, "Укажите город"),
    street: z.string().trim().min(1, "Укажите улицу"),
    building: z.string().trim().min(1, "Укажите дом"),
    roomNumber: z.string().trim().min(1, "Укажите помещение"),
  }),
  timezone: z.string().trim().min(1, "Укажите часовой пояс"),
});

export type CreateLocationFormValues = z.infer<typeof createLocationSchema>;
