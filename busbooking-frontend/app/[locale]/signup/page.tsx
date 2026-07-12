"use client";

import { Link } from "@/i18n/navigation";
import { useSignupPageVM } from "./useSignupPageVM";

export default function SignupPage() {
  const { form, setField, error, isSubmitting, handleSubmit } =
    useSignupPageVM();

  return (
    <section className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-4 py-24 sm:px-0">
      <h1 className="font-headline text-3xl font-bold text-on-surface">
        Create an account
      </h1>
      <p className="mt-2 text-on-surface-variant">
        Sign up to book seats and manage your trips.
      </p>

      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-6">
        <div className="grid grid-cols-2 gap-4">
          <label className="flex flex-col gap-2">
            <span className="text-sm font-semibold text-on-surface">
              First name
            </span>
            <input
              required
              value={form.firstName}
              onChange={(event) => setField("firstName", event.target.value)}
              className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
            />
          </label>
          <label className="flex flex-col gap-2">
            <span className="text-sm font-semibold text-on-surface">
              Last name
            </span>
            <input
              required
              value={form.lastName}
              onChange={(event) => setField("lastName", event.target.value)}
              className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
            />
          </label>
        </div>

        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Username
          </span>
          <input
            required
            value={form.username}
            onChange={(event) => setField("username", event.target.value)}
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Phone
          </span>
          <input
            type="tel"
            required
            value={form.phone}
            onChange={(event) => setField("phone", event.target.value)}
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Email
          </span>
          <input
            type="email"
            required
            autoComplete="email"
            value={form.email}
            onChange={(event) => setField("email", event.target.value)}
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
            minLength={8}
            autoComplete="new-password"
            value={form.password}
            onChange={(event) => setField("password", event.target.value)}
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        <label className="flex flex-col gap-2">
          <span className="text-sm font-semibold text-on-surface">
            Confirm password
          </span>
          <input
            type="password"
            required
            minLength={8}
            autoComplete="new-password"
            value={form.confirmPassword}
            onChange={(event) =>
              setField("confirmPassword", event.target.value)
            }
            className="rounded-lg border border-outline-variant px-4 py-3 outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>

        {error && <p className="text-sm text-error">{error}</p>}

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-primary py-3 font-semibold text-white transition-soft hover:bg-primary-container active:scale-95 disabled:opacity-60"
        >
          {isSubmitting ? "Creating account…" : "Create account"}
        </button>
      </form>

      <p className="mt-6 text-sm text-on-surface-variant">
        Already have an account?{" "}
        <Link href="/login" className="font-semibold text-primary">
          Sign in
        </Link>
      </p>
    </section>
  );
}
