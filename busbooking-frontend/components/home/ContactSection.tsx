"use client";

import { useState } from "react";

const CONTACT_EMAIL = "info@pometco.com";

export function ContactSection() {
  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [sent, setSent] = useState(false);

  function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!name.trim() || !phone.trim() || !message.trim()) {
      setError("Please fill in every field before sending.");
      setSent(false);
      return;
    }

    setError(null);
    const subject = encodeURIComponent(`Website inquiry from ${name}`);
    const body = encodeURIComponent(`${message}\n\nPhone: ${phone}`);
    window.location.href = `mailto:${CONTACT_EMAIL}?subject=${subject}&body=${body}`;
    setSent(true);
  }

  return (
    <section className="relative overflow-hidden py-24">
      <div className="absolute inset-0 -z-10 bg-gradient-to-br from-primary via-primary-container to-deep-navy" />
      <div className="absolute right-0 top-0 -z-10 h-96 w-96 rounded-full bg-secondary opacity-20 blur-[120px]" />

      <div className="mx-auto flex max-w-[1280px] flex-col items-center gap-16 px-4 sm:px-12 lg:flex-row">
        <div className="flex-1 text-white">
          <h2 className="font-headline text-4xl font-bold">
            Let&rsquo;s Get In Touch
          </h2>
          <p className="mt-6 max-w-md text-lg text-white/80">
            Have questions about routes, schedules, or special bookings? Our
            team is here to assist you 24/7.
          </p>
          <div className="mt-8 space-y-4">
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white/10">
                <span className="material-symbols-outlined">call</span>
              </div>
              <span className="font-headline text-lg">
                +373 (79) 222-444
              </span>
            </div>
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white/10">
                <span className="material-symbols-outlined">mail</span>
              </div>
              <span className="font-headline text-lg">{CONTACT_EMAIL}</span>
            </div>
          </div>
        </div>

        <div className="w-full flex-1 max-w-xl">
          <form
            onSubmit={handleSubmit}
            className="rounded-card bg-white p-8 shadow-2xl md:p-12"
          >
            <div className="grid grid-cols-1 gap-6">
              <label className="flex flex-col gap-2">
                <span className="text-sm font-semibold text-on-surface">
                  Full Name
                </span>
                <input
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  placeholder="Your Name"
                  className="rounded-lg border-none bg-surface-container-low px-4 py-4 outline-none focus:ring-1 focus:ring-primary"
                />
              </label>

              <label className="flex flex-col gap-2">
                <span className="text-sm font-semibold text-on-surface">
                  Phone Number
                </span>
                <input
                  value={phone}
                  onChange={(event) => setPhone(event.target.value)}
                  placeholder="+373"
                  type="tel"
                  className="rounded-lg border-none bg-surface-container-low px-4 py-4 outline-none focus:ring-1 focus:ring-primary"
                />
              </label>

              <label className="flex flex-col gap-2">
                <span className="text-sm font-semibold text-on-surface">
                  Message
                </span>
                <textarea
                  value={message}
                  onChange={(event) => setMessage(event.target.value)}
                  placeholder="How can we help you?"
                  rows={4}
                  className="resize-none rounded-lg border-none bg-surface-container-low px-4 py-4 outline-none focus:ring-1 focus:ring-primary"
                />
              </label>

              {error && <p className="text-sm text-error">{error}</p>}
              {sent && (
                <p className="text-sm text-primary">
                  Opening your email client to send the message…
                </p>
              )}

              <button
                type="submit"
                className="mt-2 rounded-lg bg-primary py-4 font-semibold text-white shadow-lg transition-soft hover:bg-on-primary-fixed-variant active:scale-95"
              >
                Send message
              </button>
            </div>
          </form>
        </div>
      </div>
    </section>
  );
}
