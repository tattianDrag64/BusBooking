"use client";

import { use } from "react";
import { useSeatsPageVM } from "./useSeatsPageVM";

export default function SeatsPage({
  params,
}: {
  params: Promise<{ tripId: string }>;
}) {
  const { tripId } = use(params);
  const {
    seats,
    isLoading,
    error,
    selectedSeatIds,
    toggleSeat,
    isBooking,
    confirmBooking,
  } = useSeatsPageVM(tripId);

  return (
    <section className="mx-auto w-full max-w-[1280px] px-4 py-24 sm:px-12">
      <h1 className="font-headline text-3xl font-bold text-on-surface">
        Select your seats
      </h1>

      {error && <p className="mt-4 text-sm text-error">{error}</p>}

      {isLoading ? (
        <p className="mt-8 text-on-surface-variant">Loading seats…</p>
      ) : (
        <>
          <div className="mt-8 grid grid-cols-4 gap-3 sm:grid-cols-6 lg:grid-cols-8">
            {seats.map((seat) => {
              const isSelected = selectedSeatIds.includes(seat.id);
              return (
                <button
                  key={seat.id}
                  type="button"
                  disabled={seat.isOccupied}
                  onClick={() => toggleSeat(seat.id)}
                  className={`rounded-lg border px-3 py-3 text-sm font-semibold transition-colors ${
                    seat.isOccupied
                      ? "cursor-not-allowed border-outline-variant/30 bg-surface-light text-on-surface-variant/50"
                      : isSelected
                        ? "border-primary bg-primary text-white"
                        : "border-outline-variant text-on-surface hover:border-primary"
                  }`}
                >
                  {seat.seatNumber}
                </button>
              );
            })}
          </div>

          <button
            type="button"
            disabled={selectedSeatIds.length === 0 || isBooking}
            onClick={confirmBooking}
            className="mt-8 rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary-container disabled:opacity-50"
          >
            {isBooking
              ? "Booking…"
              : `Book ${selectedSeatIds.length} seat${selectedSeatIds.length === 1 ? "" : "s"}`}
          </button>
        </>
      )}
    </section>
  );
}
