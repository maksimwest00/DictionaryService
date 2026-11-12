export const LOCATION_ERROR_FIELD_MAP: Record<string, string> = {
  "Name must be not null": "name",
  "Name must be between 3 and 120 characters": "name",
  "Address City required": "address.city",
  "Address Street required": "address.street",
  "Address Building required": "address.building",
  "Address RoomNumber required": "address.roomNumber",
  "value is empty": "timezone",
} as const;
