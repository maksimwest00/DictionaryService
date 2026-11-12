import { redirect } from "next/navigation";
import { ROUTES } from "@/shared/config/routes";

export const instant = false;

export default function Home() {
  redirect(ROUTES.LOCATIONS);
}
