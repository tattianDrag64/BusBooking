"use client";

import { use } from "react";
import { useCheckoutPageVM } from "./useCheckoutPageVM";

export default function CheckoutPage({
  params,
}: {
  params: Promise<{ orderId: string }>;
}) {
  const { orderId } = use(params);
  const { error } = useCheckoutPageVM(orderId);

  return (
    <section className="mx-auto flex w-full max-w-md flex-1 flex-col items-center justify-center px-4 py-24 text-center sm:px-0">
      {error ? (
        <>
          <h1 className="font-headline text-2xl font-bold text-on-surface">
            Checkout unavailable
          </h1>
          <p className="mt-4 text-on-surface-variant">{error}</p>
        </>
      ) : (
        <>
          <h1 className="font-headline text-2xl font-bold text-on-surface">
            Redirecting to payment…
          </h1>
          <p className="mt-4 text-on-surface-variant">
            Please wait, you&rsquo;ll be redirected to our payment provider.
          </p>
        </>
      )}
    </section>
  );
}
