"use client";

import { useEffect } from "react";
import { Button } from "@/shared/ui/button";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main className="flex flex-col items-center justify-center min-h-svh gap-6 p-10">
      <h2 className="text-2xl font-semibold">Что-то пошло не так!</h2>
      <p className="text-gray-500">{error.message}</p>
      <Button onClick={reset}>Попробовать снова</Button>
    </main>
  );
}
