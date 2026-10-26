export type DepartmentListItemResponse = {
  id: string;
  name: string;
  path: string;
  // Приходит с сервера как ISO-строка; для форматирования оборачиваем в new Date(...)
  createdAt: string;
};
