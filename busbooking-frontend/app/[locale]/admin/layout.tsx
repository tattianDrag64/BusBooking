"use client";

import { Link } from "@/i18n/navigation";
import { usePathname } from "@/i18n/navigation";
import { AdminGuard } from "@/auth/AdminGuard";

const ADMIN_LINKS = [
  { href: "/admin/routes", label: "Routes" },
  { href: "/admin/buses", label: "Buses" },
  { href: "/admin/schedules", label: "Schedules" },
  { href: "/admin/trips", label: "Trips" },
  { href: "/admin/users", label: "Users" },
  { href: "/admin/bookings", label: "Bookings" },
];

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();

  return (
    <AdminGuard>
      <div className="mx-auto flex max-w-[1280px] flex-col gap-8 px-4 py-12 sm:px-12 lg:flex-row">
        <nav className="flex gap-2 overflow-x-auto lg:w-56 lg:flex-none lg:flex-col lg:gap-1">
          {ADMIN_LINKS.map((link) => {
            const active = pathname?.startsWith(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                className={`whitespace-nowrap rounded-lg px-3 py-2 text-sm font-semibold ${
                  active
                    ? "bg-surface-light text-primary"
                    : "text-on-surface-variant hover:text-primary"
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>
        <div className="flex-1">{children}</div>
      </div>
    </AdminGuard>
  );
}
