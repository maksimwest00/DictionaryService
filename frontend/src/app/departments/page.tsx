import type { Metadata } from "next";
import DepartmentsClient from "./departments-client";

export const metadata: Metadata = {
  title: "Подразделения",
  description: "Список подразделений справочника",
};

export default function Page() {
  return <DepartmentsClient />;
}
