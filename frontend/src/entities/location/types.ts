export type LocationListItemResponse = {
  id: string;
  name: string;
  // Приходит с сервера как ISO-строка; для форматирования оборачиваем в new Date(...)
  createdAt: string;
  departmentCount: number;
  address: AddressDto;
};

export type AddressDto = {
  city: string;
  street: string;
  building: string;
  roomNumber?: string | null;
};
