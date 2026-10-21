import { ROUTES } from "@/shared/config/routes";
import { Briefcase, Building2, MapPin, type LucideIcon } from "lucide-react";

export type NavItem = {
  name: string;
  href: (typeof ROUTES)[keyof typeof ROUTES];
  icon: LucideIcon;
};

export const NAV_ITEMS: NavItem[] = [
  { name: "Локации", href: ROUTES.LOCATIONS, icon: MapPin },
  { name: "Подразделения", href: ROUTES.DEPARTMENTS, icon: Building2 },
  { name: "Позиции", href: ROUTES.POSITIONS, icon: Briefcase },
];
