"use client";

import { useQuery } from "@tanstack/react-query";
import { getUsers } from "@/api/adminUsers";

export default function AdminUsersPage() {
  const usersQuery = useQuery({ queryKey: ["admin", "users"], queryFn: getUsers });
  const users = usersQuery.data ?? [];

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Users</h1>
        <p className="mt-1 text-sm text-on-surface-variant">Read-only — no role management yet.</p>
      </div>

      {usersQuery.isPending ? (
        <div className="h-40 animate-pulse rounded-2xl bg-surface-light" />
      ) : usersQuery.isError ? (
        <p className="text-sm text-error">Could not load users.</p>
      ) : (
        <div className="overflow-x-auto rounded-2xl border border-outline-variant/30">
          <table className="w-full text-left text-sm">
            <thead className="bg-surface-light text-on-surface-variant">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Username</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Phone</th>
                <th className="px-4 py-3">Role</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id} className="border-t border-outline-variant/30">
                  <td className="px-4 py-3">
                    {user.firstName} {user.lastName}
                  </td>
                  <td className="px-4 py-3">{user.username}</td>
                  <td className="px-4 py-3">{user.email}</td>
                  <td className="px-4 py-3">{user.phone ?? "—"}</td>
                  <td className="px-4 py-3">{user.role}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
