import { authFetch } from "@/api/authFetch";
import type { AdminUser } from "@/types/adminUser";

export function getUsers(): Promise<AdminUser[]> {
  return authFetch<AdminUser[]>("/api/users");
}
