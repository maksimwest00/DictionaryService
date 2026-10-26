"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { departmentQueries } from "@/entities/department/queries";
import { DataState } from "@/shared/ui/data-state";
import { PaginationBar } from "@/shared/ui/pagination-bar";

const PAGE_SIZE = 12;

export default function DepartmentsClient() {
  const [page, setPage] = useState(1);

  const { data, isPending, isError, isSuccess, error, refetch } = useQuery(
    departmentQueries.list({ page, pageSize: PAGE_SIZE }),
  );

  const departments = data?.data ?? [];
  const totalCount = data?.totalCount ?? 0;

  return (
    <main className="flex flex-col gap-6 p-10">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold">Подразделения</h1>
        <p className="text-sm text-gray-500">
          Список подразделений справочника.
        </p>
      </div>

      <DataState
        isLoading={isPending}
        isError={isError}
        errorMessage={error?.message}
        errorTitle="Не удалось загрузить подразделения"
        isEmpty={isSuccess && departments.length === 0}
        emptyText="Подразделения не найдены."
        onRetry={() => refetch()}
      >
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {departments.map((department) => (
            <article
              key={department.id}
              className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md"
            >
              <h2 className="text-lg font-medium">{department.name}</h2>

              <p className="text-sm text-gray-600">{department.path}</p>

              <div className="mt-auto flex items-center justify-between text-xs text-gray-500">
                <span>
                  Создано:{" "}
                  {new Date(department.createdAt).toLocaleDateString("ru-RU")}
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
