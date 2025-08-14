'use client';

import { useApi } from '../lib/swr';

export default function RoleManager() {
  const { data: roles, error } = useApi('/api/admin/roles');

  if (error) return <div>Failed to load roles</div>;
  if (!roles) return <div>Loading...</div>;

  return (
    <div className="mt-8">
      <h2 className="text-xl font-semibold">Roles</h2>
      <ul className="mt-4 space-y-2">
        {roles.map((role: string) => (
          <li key={role} className="px-4 py-2 bg-gray-100 rounded-md">{role}</li>
        ))}
      </ul>
    </div>
  );
}