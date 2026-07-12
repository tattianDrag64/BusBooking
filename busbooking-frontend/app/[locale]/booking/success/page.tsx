import { Link } from "@/i18n/navigation";

export const metadata = {
  title: "Booking confirmed — Pometco",
};

export default async function BookingSuccessPage({
  searchParams,
}: {
  searchParams: Promise<{ orderId?: string }>;
}) {
  const { orderId } = await searchParams;

  return (
    <section className="mx-auto flex w-full max-w-md flex-1 flex-col items-center justify-center px-4 py-24 text-center sm:px-0">
      <span className="material-symbols-outlined text-5xl text-primary">
        check_circle
      </span>
      <h1 className="mt-4 font-headline text-2xl font-bold text-on-surface">
        Payment successful
      </h1>
      <p className="mt-4 text-on-surface-variant">
        Your booking is confirmed{orderId ? ` — order ${orderId}` : ""}. You
        can view it on your bookings page.
      </p>
      <Link
        href="/bookings"
        className="mt-8 rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary-container"
      >
        View my bookings
      </Link>
    </section>
  );
}
