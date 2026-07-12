"use client";

import { useRouter } from "@/i18n/navigation";
import { useState } from "react";
import { useAuth } from "@/auth/useAuth";
import type { SignUpPayload } from "@/types/auth";

const EMPTY_FORM: SignUpPayload = {
  firstName: "",
  lastName: "",
  username: "",
  phone: "",
  email: "",
  password: "",
  confirmPassword: "",
};

export function useSignupPageVM() {
  const router = useRouter();
  const { signUp } = useAuth();

  const [form, setForm] = useState<SignUpPayload>(EMPTY_FORM);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function setField<K extends keyof SignUpPayload>(
    field: K,
    value: SignUpPayload[K],
  ) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (form.password !== form.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await signUp(form);
      router.push("/");
    } catch {
      setError("Could not create an account. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return { form, setField, error, isSubmitting, handleSubmit };
}
