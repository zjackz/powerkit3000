'use client';

import { useState, useEffect } from 'react';
import { useSWRConfig } from 'swr';

export default function EditUserForm({ userId, closeModal }: { userId: string; closeModal: () => void }) {
  const { mutate } = useSWRConfig();
  const [email, setEmail] = useState('');
  const [roles, setRoles] = useState<string[]>([]);

  useEffect(() => {
    const fetchUser = async () => {
      const res = await fetch(`/api/admin/users/${userId}`);
      const user = await res.json();
      setEmail(user.email);
      setRoles(user.roles);
    };
    fetchUser();
  }, [userId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await fetch(`/api/admin/users/${userId}`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, roles }),
      }
    );
    mutate('/api/admin/users');
    closeModal();
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2 className="text-xl font-semibold">Edit User</h2>
      <div className="mt-4">
        <label className="block text-sm font-medium text-gray-700">Email</label>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        />
      </div>
      {/* Add role management UI here in the future */}
      <div className="mt-4 flex justify-end">
        <button type="button" onClick={closeModal} className="mr-2 px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300">
          Cancel
        </button>
        <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700">
          Save
        </button>
      </div>
    </form>
  );
}
