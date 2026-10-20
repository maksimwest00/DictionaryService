import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Positions",
  description: "Positions Next App",
};

export default function Home() {
  return (
    <main className="flex flex-col gap-2 p-10">
      <h1 className="text-2xl font-semibold">Позиции</h1>
    </main>
  );
}
