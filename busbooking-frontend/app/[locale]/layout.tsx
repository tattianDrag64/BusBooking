import type { Metadata } from "next";
import { Fira_Sans, Inter } from "next/font/google";
import { NextIntlClientProvider } from "next-intl";
import { setRequestLocale } from "next-intl/server";
import { notFound } from "next/navigation";
import { routing } from "@/i18n/routing";
import { Header } from "@/components/ui/Header";
import { Footer } from "@/components/ui/Footer";
import { AuthProvider } from "@/auth/AuthProvider";
import { CurrencyProvider } from "@/currency/CurrencyProvider";
import { QueryProvider } from "@/lib/QueryProvider";
import "../globals.css";

const firaSans = Fira_Sans({
  variable: "--font-headline",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700", "800"],
});

const inter = Inter({
  variable: "--font-body",
  subsets: ["latin"],
  weight: ["400", "500", "600"],
});

export const metadata: Metadata = {
  title: "Pometco — Bus Tickets",
  description: "Order a bus ticket and travel with comfort across Europe.",
};

export function generateStaticParams() {
  return routing.locales.map((locale) => ({ locale }));
}

export default async function RootLayout({
  children,
  params,
}: Readonly<{
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}>) {
  const { locale } = await params;
  if (!routing.locales.includes(locale as (typeof routing.locales)[number])) {
    notFound();
  }

  // Required for static rendering of this layout across locales — tells
  // next-intl which locale is being rendered on the server (see next-intl
  // docs, "Static rendering").
  setRequestLocale(locale);

  return (
    <html
      lang={locale}
      className={`${firaSans.variable} ${inter.variable} h-full antialiased`}
    >
      <head>
        <link
          href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap"
          rel="stylesheet"
        />
      </head>
      <body className="flex min-h-full flex-col font-body">
        <NextIntlClientProvider>
          <QueryProvider>
            <AuthProvider>
              <CurrencyProvider>
                <Header />
                <main className="flex flex-1 flex-col">{children}</main>
                <Footer />
              </CurrencyProvider>
            </AuthProvider>
          </QueryProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
