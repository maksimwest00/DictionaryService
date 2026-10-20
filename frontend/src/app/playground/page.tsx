import PlayGround from "./playground";

export default function Home() {
  return (
    <main className="flex flex-col gap-2 p-10">
      <h1 className="text-2xl font-semibold">Todo</h1>
      <PlayGround />
    </main>
  );
}
