"use client";

import { Link } from "@/i18n/navigation";
import { Suspense } from "react";
import { useLoginPageVM } from "./useLoginPageVM";

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}

function LoginForm() {
  const {
    email,
    setEmail,
    password,
    setPassword,
    error,
    isSubmitting,
    handleSubmit,
  } = useLoginPageVM();

  return (
    <section className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-4 py-24 sm:px-0">
      <h1 className="font-headline text-3xl font-bold text-on-surface">
        Sign in
      </h1>
      <p className="mt-2 text-on-surface-variant">
        Welcome back — sign in to manage your bookings.
      </p>

      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-6">
        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Email
          </span>
          <input
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Password
          </span>
          <input
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        {error && <p className="text-sm text-error">{error}</p>}

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-primary py-3 font-semibold text-white transition-soft hover:bg-primary-container active:scale-95 disabled:opacity-60"
        >
          {isSubmitting ? "Signing in…" : "Sign in"}
        </button>
      </form>

      <p className="mt-6 text-sm text-on-surface-variant">
        Don&rsquo;t have an account?{" "}
        <Link href="/signup" className="font-semibold text-primary">
          Create one
        </Link>
      </p>
    </section>
  );
}
