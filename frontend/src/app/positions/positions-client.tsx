"use client";

import { useRef, useEffect, useCallback, useMemo } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { positionsInfiniteQueryOptions } from "@/entities/position/queries";
import { PositionListItemResponse } from "@/entities/position/schemas";

const PAGE_SIZE = 20;

export default function PositionsClient() {
  const {
    data,
    isPending,
    isError,
    error,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    refetch,
  } = useInfiniteQuery(positionsInfiniteQueryOptions({ pageSize: PAGE_SIZE }));

  const observerRef = useRef<IntersectionObserver | null>(null);
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  const allPositions = data?.pages.flatMap((page) => page.data) ?? [];

  const uniquePositions = useMemo(() => {
    const seen = new Set<string>();
    return allPositions.filter((position) => {
      if (seen.has(position.id)) {
        return false;
      }
      seen.add(position.id);
      return true;
    });
  }, [allPositions]);

  const isFirstLoad = isPending && !isFetchingNextPage;
  const isEmpty = !isPending && !isFetchingNextPage && uniquePositions.length === 0;
  const isEnd = !hasNextPage && uniquePositions.length > 0;

  const handleRetryNextPage = useCallback(() => {
    fetchNextPage();
  }, [fetchNextPage]);

  useEffect(() => {
    if (sentinelRef.current) {
      observerRef.current = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        { threshold: 0.1 }
      );

      observerRef.current.observe(sentinelRef.current);
    }

    return () => {
      observerRef.current?.disconnect();
    };
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  return (
    <main className="flex flex-col gap-6 p-10">
      <div className="flex items-start justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold">Должности</h1>
          <p className="text-sm text-gray-500">
            Список доступных должностей организации.
          </p>
        </div>
      </div>

      {isFirstLoad && (
        <div className="flex items-center justify-center py-20">
          <div className="text-sm text-gray-500">Загрузка должностей...</div>
        </div>
      )}

      {isError && !isFetchingNextPage && (
        <div className="flex flex-col items-center gap-4 py-20">
          <div className="text-sm text-red-600">
            Не удалось загрузить должности: {error?.message}
          </div>
          <button
            onClick={() => refetch()}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
          >
            Повторить
          </button>
        </div>
      )}

      {isEmpty && (
        <div className="flex items-center justify-center py-20">
          <div className="text-sm text-gray-500">Должности не найдены.</div>
        </div>
      )}

      {!isFirstLoad && !isEmpty && (
        <>
          <Data positions={uniquePositions} />

          <div ref={sentinelRef} className="h-4" />

          {isFetchingNextPage && (
            <div className="flex items-center justify-center py-4">
              <div className="text-sm text-gray-500">Загрузка...</div>
            </div>
          )}

          {isEnd && (
            <div className="flex items-center justify-center py-4">
              <div className="text-sm text-gray-500">
                Конец списка должностей
              </div>
            </div>
          )}

          {isError && isFetchingNextPage && (
            <div className="flex items-center justify-center gap-4 py-4">
              <div className="text-sm text-red-600">
                Ошибка загрузки следующей страницы
              </div>
              <button
                onClick={handleRetryNextPage}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
              >
                Повторить
              </button>
            </div>
          )}
        </>
      )}
    </main>
  );
}

const Data = ({ positions }: { positions: PositionListItemResponse[] }) => {
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {positions.map((position) => (
        <article
          key={position.id}
          className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md"
        >
          <h2 className="text-lg font-medium">{position.name}</h2>

          {position.description && (
            <p className="text-sm text-gray-600">{position.description}</p>
          )}

          <div className="mt-auto flex items-center justify-between text-xs text-gray-500">
            <span>Отделов: {position.departmentCount}</span>
            <span>
              Создано:{" "}
              {new Date(position.createdAt).toLocaleDateString("ru-RU")}
            </span>
          </div>
        </article>
      ))}
    </div>
  );
};
