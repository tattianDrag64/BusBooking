import { Link } from "@/i18n/navigation";

export const metadata = {
  title: "Booking cancelled — Pometco",
};

export default async function BookingCancelPage({
  searchParams,
}: {
  searchParams: Promise<{ orderId?: string }>;
}) {
  const { orderId } = await searchParams;

  return (
    <section className="mx-auto flex w-full max-w-md flex-1 flex-col items-center justify-center px-4 py-24 text-center sm:px-0">
      <span className="material-symbols-outlined text-5xl text-on-surface-variant">
        cancel
      </span>
      <h1 className="mt-4 font-headline text-2xl font-bold text-on-surface">
        Payment cancelled
      </h1>
      <p className="mt-4 text-on-surface-variant">
        {orderId ? `Order ${orderId} was not paid.` : "Your payment was not completed."}{" "}
        You can try again from the search page.
      </p>
      <Link
        href="/trips"
        className="mt-8 rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary-container"
      >
        Back to search
      </Link>
    </section>
  );
}
