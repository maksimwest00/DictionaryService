import axios from "axios";

// Базовый URL берём из окружения; фолбэк — локальный backend для разработки
const baseURL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5202/api";

export const apiClient = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});
