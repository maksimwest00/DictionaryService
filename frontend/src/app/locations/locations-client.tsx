"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { locationQueries } from "@/entities/location/queries";
import { DataState } from "@/shared/ui/data-state";
import { PaginationBar } from "@/shared/ui/pagination-bar";

const PAGE_SIZE = 12;

export default function LocationsClient() {
  const [page, setPage] = useState(1);

  const { data, isPending, isError, isSuccess, error, refetch } = useQuery(
    locationQueries.list({ page, pageSize: PAGE_SIZE }),
  );

  const locations = data?.data ?? [];
  const totalCount = data?.totalCount ?? 0;

  return (
    <main className="flex flex-col gap-6 p-10">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold">Локации</h1>
        <p className="text-sm text-gray-500">
          Список доступных локаций: города, регионы и страны.
        </p>
      </div>

      <DataState
        isLoading={isPending}
        isError={isError}
        errorMessage={error?.message}
        errorTitle="Не удалось загрузить локации"
        isEmpty={isSuccess && locations.length === 0}
        emptyText="Локации не найдены."
        onRetry={() => refetch()}
      >
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
      </DataState>

      <PaginationBar
        page={page}
        pageSize={PAGE_SIZE}
        totalCount={totalCount}
        onPageChange={setPage}
      />
    </main>
  );
}
