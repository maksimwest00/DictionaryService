"use client";

import { NAV_ITEMS, type NavItem } from "@/widgets/sidebar/config";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
  useSidebar,
} from "@/shared/ui/sidebar";
import Link from "next/link";
import { usePathname } from "next/navigation";

export default function SideBar() {
  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="px-4 py-3 text-lg font-semibold">
        <span className="truncate group-data-[collapsible=icon]:hidden">
          Directory Service
        </span>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {NAV_ITEMS.map((item) => (
                <MyLink key={item.href} item={item} />
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  );
}

function MyLink({ item }: { item: NavItem }) {
  const pathName = usePathname();
  const { isMobile, setOpenMobile } = useSidebar();
  const isActive =
    pathName === item.href || pathName.startsWith(`${item.href}/`);
  const Icon = item.icon;

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        isActive={isActive}
        tooltip={item.name}
        render={
          <Link
            href={item.href}
            aria-current={isActive ? "page" : undefined}
            onClick={() => {
              if (isMobile) setOpenMobile(false);
            }}
          />
        }
      >
        {Icon ? <Icon /> : null}
        <span>{item.name}</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  );
}
