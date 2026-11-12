import { z } from "zod";

export const addressDtoSchema = z.object({
  city: z.string(),
  street: z.string(),
  building: z.string(),
  roomNumber: z.string(),
});

export const locationListItemResponseSchema = z.object({
  id: z.string(),
  name: z.string(),
  createdAt: z.string(),
  departmentCount: z.number(),
  address: addressDtoSchema,
});

export const createLocationRequestSchema = z.object({
  name: z.string().min(3).max(120),
  address: z.object({
    city: z.string().min(1),
    street: z.string().min(1),
    building: z.string().min(1),
    roomNumber: z.string(),
  }),
  timezone: z.string().min(1),
});

export const createLocationFormSchema = createLocationRequestSchema.extend({
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

export type AddressDto = z.infer<typeof addressDtoSchema>;

export type LocationListItemResponse = z.infer<
  typeof locationListItemResponseSchema
>;

export type CreateLocationRequest = z.infer<typeof createLocationRequestSchema>;

export type CreateLocationFormValues = z.infer<typeof createLocationFormSchema>;
