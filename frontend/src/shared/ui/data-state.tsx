"use client";

import { Skeleton } from "@/shared/ui/skeleton";
import { Button } from "@/shared/ui/button";

type DataStateProps = {
  isLoading: boolean;
  isError: boolean;
  errorMessage?: string;
  isEmpty: boolean;
  emptyText: string;
  errorTitle: string;
  onRetry: () => void;
  skeletonCount?: number;
  children: React.ReactNode;
};

/**
 * Единый набор состояний для списочных страниц:
 * loading → error → empty → data.
 * Компонент не знает ничего про backend/envelope — только про состояние запроса.
 */
export function DataState({
  isLoading,
  isError,
  errorMessage,
  isEmpty,
  emptyText,
  errorTitle,
  onRetry,
  skeletonCount = 6,
  children,
}: DataStateProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: skeletonCount }).map((_, index) => (
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
    );
  }

  if (isError) {
    return (
      <div className="flex flex-col items-start gap-4 rounded-xl border border-red-200 bg-red-50 p-6">
        <div className="flex flex-col gap-1">
          <h2 className="text-lg font-medium text-red-800">{errorTitle}</h2>
          {errorMessage && (
            <p className="text-sm text-red-700">{errorMessage}</p>
          )}
        </div>
        <Button variant="destructive" onClick={onRetry}>
          Повторить
        </Button>
      </div>
    );
  }

  if (isEmpty) {
    return <p className="text-sm text-gray-500">{emptyText}</p>;
  }

  return <>{children}</>;
}
