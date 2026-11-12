import { ROUTES } from "@/shared/config/routes";
import { MapPin, Briefcase, type LucideIcon } from "lucide-react";

export type NavItem = {
  name: string;
  href: (typeof ROUTES)[keyof typeof ROUTES];
  icon: LucideIcon;
};

export const NAV_ITEMS: NavItem[] = [
  { name: "Локации", href: ROUTES.LOCATIONS, icon: MapPin },
  { name: "Должности", href: ROUTES.POSITIONS, icon: Briefcase },
];
