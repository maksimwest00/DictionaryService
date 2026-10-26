import axios from "axios";
import type { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import type { ApiError, Envelope, ErrorType } from "./types";

// Базовый URL берём из окружения; фолбэк — локальный backend для разработки
const baseURL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5202/api";

/**
 * Ошибка, которую кидает HTTP-клиент, когда backend вернул бизнес-ошибку
 * или пустой result. Компоненты и TanStack Query работают именно с ней.
 */
export class HttpError extends Error implements ApiError {
  code: string;
  messages: string[];
  type: ErrorType;
  invalidField?: string | null;

  constructor(error: ApiError) {
    super(error.messages.join("; ") || error.code);
    this.name = "HttpError";
    this.code = error.code;
    this.messages = error.messages;
    this.type = error.type;
    this.invalidField = error.invalidField;
  }
}

/** Пробуем достать ошибку из тела ответа, даже если статус не 2xx. */
function extractError(body: unknown): ApiError | null {
  const envelope = body as Partial<Envelope<unknown>> | undefined;
  // backend отдаёт errors массивом (ErrorsJsonConverter), но на всякий
  // случай поддерживаем и объект с items
  const errors = envelope?.errors as unknown;
  const list = Array.isArray(errors)
    ? errors
    : ((errors as { items?: ApiError[] } | undefined)?.items ?? []);
  const first = list[0];

  if (!first) return null;

  return {
    code: first.code,
    messages: first.messages ?? [],
    type: first.type ?? "FAILURE",
    invalidField: first.invalidField ?? null,
  };
}

const instance = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

// Успешный ответ: разворачиваем Envelope и отдаём сразу result.
// Пустой result — ошибка контракта, кидаем FAILURE, чтобы было видно в UI.
instance.interceptors.response.use(
  (response: AxiosResponse<Envelope<unknown>>) => {
    const envelope = response.data;

    const apiError = extractError(envelope);
    if (apiError) {
      return Promise.reject(new HttpError(apiError));
    }

    if (envelope?.result === null || envelope?.result === undefined) {
      return Promise.reject(
        new HttpError({
          code: "empty.result",
          messages: ["Ответ сервера не содержит данных"],
          type: "FAILURE",
        }),
      );
    }

    // Подменяем data — наружу уходит чистый result, без envelope
    response.data = envelope.result as Envelope<unknown>;
    return response;
  },
  (error: AxiosError) => {
    const apiError = extractError(error.response?.data);

    if (apiError) {
      return Promise.reject(new HttpError(apiError));
    }

    // Нет ответа от backend'а — сетевой сбой. type не ставим,
    // query-client ретраит такие ошибки.
    return Promise.reject(error);
  },
);

/**
 * Типизированные хелперы: интерсептор уже развернул envelope,
 * поэтому T — это сам полезный результат (например, PagedResult<...>).
 */
export const apiClient = {
  get: async <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    const response = await instance.get<T>(url, config);
    return response.data;
  },
  post: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.post<T>(url, data, config);
    return response.data;
  },
  put: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.put<T>(url, data, config);
    return response.data;
  },
  patch: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> => {
    const response = await instance.patch<T>(url, data, config);
    return response.data;
  },
  delete: async <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    const response = await instance.delete<T>(url, config);
    return response.data;
  },
};
