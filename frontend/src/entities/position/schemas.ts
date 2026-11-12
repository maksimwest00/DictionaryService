import { z } from "zod";

export const positionListItemResponseSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().nullable(),
  createdAt: z.string(),
  updatedAt: z.string(),
  isActive: z.boolean(),
  departmentCount: z.number(),
});

export type PositionListItemResponse = z.infer<
  typeof positionListItemResponseSchema
>;
