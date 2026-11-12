import type { Metadata } from "next";
import PositionsClient from "./positions-client";

export const metadata: Metadata = {
  title: "Должности",
  description: "Список доступных должностей справочника",
};

export default function Page() {
  return <PositionsClient />;
}
