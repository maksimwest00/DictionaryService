import type { Metadata } from "next";
import LocationsClient from "./locations-client";

export const metadata: Metadata = {
  title: "Локации",
  description: "Список доступных локаций справочника",
};

export default function Page() {
  return <LocationsClient />;
}
