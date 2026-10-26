export type LocationListItemResponse = {
  id: string;
  name: string;
  createdAt: Date;
  departmentCount: number;
  address: AddressDto;
};

export type AddressDto = {
  city: string;
  street: string;
  building: string;
  roomNumber: string;
};
