"use client";

import { useTranslations } from "next-intl";
import { Link, usePathname } from "@/i18n/navigation";
import { Suspense, useEffect, useState } from "react";
import { useAuth } from "@/auth/useAuth";
import { LocaleSwitcher } from "./LocaleSwitcher";
import { CurrencySwitcher } from "./CurrencySwitcher";

export function Header() {
  const t = useTranslations("Nav");
  const pathname = usePathname();
  const { user, isAuthenticated, signOut } = useAuth();
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [prevPathname, setPrevPathname] = useState(pathname);

  const navLinks = [
    { href: "/about", label: t("aboutUs") },
    { href: "/routes", label: t("schedules") },
  ];

  if (pathname !== prevPathname) {
    setPrevPathname(pathname);
    setMenuOpen(false);
  }

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 50);
    onScroll();
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <header
      className={`sticky top-0 z-50 w-full bg-surface transition-[height,box-shadow] duration-300 ${
        scrolled ? "h-16 shadow-md" : "h-20 shadow-sm"
      }`}
    >
      <div className="mx-auto flex h-full max-w-[1280px] items-center justify-between px-4 sm:px-12">
        <div className="flex items-center gap-8">
          <Link href="/" className="flex items-center gap-2">
            <span
              className="material-symbols-outlined text-3xl text-primary"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              call_to_action
            </span>
            <span className="font-headline text-xl font-bold text-primary">
              Pometco
            </span>
          </Link>

          <nav className="hidden gap-6 md:flex">
            {navLinks.map((link) => {
              const active = pathname === link.href;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={`pb-1 text-sm font-semibold transition-colors ${
                    active
                      ? "border-b-2 border-primary text-primary"
                      : "text-on-surface-variant hover:text-primary"
                  }`}
                >
                  {link.label}
                </Link>
              );
            })}
          </nav>
        </div>

        <div className="flex items-center gap-4 sm:gap-6">
          <div className="hidden flex-col items-end lg:flex">
            <span className="text-sm font-bold text-primary">
              +373 (79) 222-444
            </span>
            <span className="text-xs text-on-surface-variant">
              {t("callCenter")}
            </span>
          </div>

          <Suspense fallback={null}>
            <LocaleSwitcher className="hidden md:flex" />
          </Suspense>
          <CurrencySwitcher className="hidden md:flex" />

          <Link
            href="/routes"
            className="hidden rounded-full bg-primary px-5 py-2 text-sm font-semibold text-white transition-colors hover:bg-primary-container sm:inline-block"
          >
            {t("tickets")}
          </Link>

          {isAuthenticated ? (
            <>
              {user?.role === "Admin" && (
                <Link
                  href="/admin/routes"
                  className="hidden text-sm font-semibold text-on-surface-variant transition-colors hover:text-primary sm:inline-block"
                >
                  {t("admin")}
                </Link>
              )}
              <Link
                href="/bookings"
                className="hidden text-sm font-semibold text-on-surface-variant transition-colors hover:text-primary sm:inline-block"
              >
                {t("myBookings")}
              </Link>
              <button
                type="button"
                onClick={() => void signOut()}
                className="hidden text-sm font-semibold text-on-surface-variant transition-colors hover:text-primary sm:inline-block"
                title={user?.email}
              >
                {t("signOut")}
              </button>
            </>
          ) : (
            <Link
              href="/login"
              className="hidden text-sm font-semibold text-on-surface-variant transition-colors hover:text-primary sm:inline-block"
            >
              {t("signIn")}
            </Link>
          )}

          <button
            type="button"
            aria-label="Toggle menu"
            aria-expanded={menuOpen}
            className="-mr-2.5 flex h-11 w-11 items-center justify-center md:hidden"
            onClick={() => setMenuOpen((open) => !open)}
          >
            <span className="material-symbols-outlined text-on-surface">
              {menuOpen ? "close" : "menu"}
            </span>
          </button>
        </div>
      </div>

      {menuOpen && (
        <nav className="flex flex-col gap-1 border-t border-outline-variant/30 bg-surface px-4 py-4 md:hidden">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={`flex min-h-11 items-center rounded-lg px-3 py-2 text-sm font-semibold ${
                pathname === link.href
                  ? "bg-surface-light text-primary"
                  : "text-on-surface-variant"
              }`}
            >
              {link.label}
            </Link>
          ))}
          <Link
            href="/routes"
            className="mt-2 flex min-h-11 items-center justify-center rounded-full bg-primary px-4 py-2 text-center text-sm font-semibold text-white"
          >
            {t("tickets")}
          </Link>
          {isAuthenticated ? (
            <>
              {user?.role === "Admin" && (
                <Link
                  href="/admin/routes"
                  className="flex min-h-11 items-center rounded-lg px-3 py-2 text-sm font-semibold text-on-surface-variant"
                >
                  {t("admin")}
                </Link>
              )}
              <Link
                href="/bookings"
                className="flex min-h-11 items-center rounded-lg px-3 py-2 text-sm font-semibold text-on-surface-variant"
              >
                {t("myBookings")}
              </Link>
              <button
                type="button"
                onClick={() => void signOut()}
                className="flex min-h-11 items-center rounded-lg px-3 py-2 text-left text-sm font-semibold text-on-surface-variant"
              >
                {t("signOut")}
              </button>
            </>
          ) : (
            <Link
              href="/login"
              className="flex min-h-11 items-center rounded-lg px-3 py-2 text-sm font-semibold text-on-surface-variant"
            >
              {t("signIn")}
            </Link>
          )}
          <Suspense fallback={null}>
            <LocaleSwitcher className="px-3 pt-2" />
          </Suspense>
          <CurrencySwitcher className="px-3" />
        </nav>
      )}
    </header>
  );
}
