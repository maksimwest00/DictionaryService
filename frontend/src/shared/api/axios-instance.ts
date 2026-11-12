import axios, { AxiosRequestConfig } from "axios";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

const baseURL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5202/api";

const instance = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

function checkEnvelope<T>(data: Envelope<T>): T {
  if (data.errors && data.errors.length > 0) {
    throw new EnvelopeError(data.errors);
  }
  return data.result as T;
}

export const apiClient = {
  get: async <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    const response = await instance.get<Envelope<T>>(url, config);
    return checkEnvelope(response.data);
  },
  post: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.post<Envelope<T>>(url, data, config);
    return checkEnvelope(response.data);
  },
  put: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.put<Envelope<T>>(url, data, config);
    return checkEnvelope(response.data);
  },
  patch: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.patch<Envelope<T>>(url, data, config);
    return checkEnvelope(response.data);
  },
  delete: async <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    const response = await instance.delete<Envelope<T>>(url, config);
    return checkEnvelope(response.data);
  },
};
