"use client";

import { locationsApi } from "@/entities/location/api";
import { LocationListItemResponse } from "@/entities/location/types";
import { Skeleton } from "@/shared/ui/skeleton";
import { useEffect, useState } from "react";

export default function Home() {
  const [locations, setLocations] = useState<LocationListItemResponse[]>([]);

  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    locationsApi
      .getLocations({}, controller.signal)
      .then((data) => setLocations(data))
      .catch((error) => {
        if (controller.signal.aborted) return;
        setError(error.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false);
      });

    return () => controller.abort();
  }, [reloadKey]);

  const handleRetry = () => {
    setIsLoading(true);
    setError(null);
    setReloadKey((key) => key + 1);
  };

  if (isLoading) {
    return (
      <main className="flex flex-col gap-6 p-10">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold">Локации</h1>
          <p className="text-sm text-gray-500">
            Список доступных локаций: города, регионы и страны.
          </p>
        </div>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <div
              key={index}
              className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-5 shadow-sm"
            >
              <Skeleton className="h-6 w-2/3" />
              <Skeleton className="h-4 w-full" />
              <div className="mt-auto flex items-center justify-between">
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-3 w-24" />
              </div>
            </div>
          ))}
        </div>
      </main>
    );
  }

  if (error) {
    return (
      <main className="flex flex-col gap-6 p-10">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold">Локации</h1>
          <p className="text-sm text-gray-500">
            Список доступных локаций: города, регионы и страны.
          </p>
        </div>
        <div className="flex flex-col items-start gap-4 rounded-xl border border-red-200 bg-red-50 p-6">
          <div className="flex flex-col gap-1">
            <h2 className="text-lg font-medium text-red-800">
              Не удалось загрузить локации
            </h2>
            <p className="text-sm text-red-700">{error}</p>
          </div>
          <button
            type="button"
            onClick={handleRetry}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700"
          >
            Повторить
          </button>
        </div>
      </main>
    );
  }

  return (
    <main className="flex flex-col gap-6 p-10">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold">Локации</h1>
        <p className="text-sm text-gray-500">
          Список доступных локаций: города, регионы и страны.
        </p>
      </div>
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {locations.map((location) => (
          <article
            key={location.id}
            className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md"
          >
            <h2 className="text-lg font-medium">{location.name}</h2>

            <p className="text-sm text-gray-600">
              {location.address.city}, {location.address.street}, д.{" "}
              {location.address.building}
              {location.address.roomNumber
                ? `, пом. ${location.address.roomNumber}`
                : ""}
            </p>

            <div className="mt-auto flex items-center justify-between text-xs text-gray-500">
              <span>Отделов: {location.departmentCount}</span>
              <span>
                Создано:{" "}
                {new Date(location.createdAt).toLocaleDateString("ru-RU")}
              </span>
            </div>
          </article>
        ))}
      </div>
      {locations.length === 0 && (
        <p className="text-sm text-gray-500">Локации не найдены.</p>
      )}
    </main>
  );
}
