"use client";

import { SidebarInset, SidebarProvider } from "@/shared/ui/sidebar";
import { cn } from "@/shared/lib/cn";
import Header from "@/widgets/header/header";
import SideBar from "@/widgets/sidebar/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/shared/api/query-client";

type AppShellProps = {
  children: React.ReactNode;
  contentClassName?: string;
};

export default function AppShell({
  children,
  contentClassName,
}: AppShellProps) {
  return (
    <>
      <QueryClientProvider client={queryClient}>
        <SidebarProvider>
          <SideBar />
          <SidebarInset>
            <Header />
            <div className={cn("flex-1 p-10", contentClassName)}>
              {children}
            </div>
          </SidebarInset>
        </SidebarProvider>
      </QueryClientProvider>
    </>
  );
}
