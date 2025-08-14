'use client';

import { useState } from 'react';
import { useApi, fetcher } from '../lib/swr';
import EditUserForm from './EditUserForm';
import Modal from './Modal';

export default function UserList() {
  const { data: users, error, mutate } = useApi('/api/admin/users');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

  const openModal = (userId: string) => {
    setSelectedUserId(userId);
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setSelectedUserId(null);
    setIsModalOpen(false);
  };

  const handleDelete = async (userId: string) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      await fetcher(`/api/admin/users/${userId}`, { method: 'DELETE' });
      mutate();
    }
  };

  if (error) return <div>Failed to load users</div>;
  if (!users) return <div>Loading...</div>;

  return (
    <div className="mt-8">
      <h2 className="text-xl font-semibold">Users</h2>
      <table className="min-w-full divide-y divide-gray-200 mt-4">
        <thead className="bg-gray-50">
          <tr>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Username</th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Roles</th>
            <th scope="col" className="relative px-6 py-3">
              <span className="sr-only">Actions</span>
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {users.map((user: any) => (
            <tr key={user.id}>
              <td className="px-6 py-4 whitespace-nowrap">{user.userName}</td>
              <td className="px-6 py-4 whitespace-nowrap">{user.email}</td>
              <td className="px-6 py-4 whitespace-nowrap">{user.roles.join(', ')}</td>
              <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button onClick={() => openModal(user.id)} className="text-indigo-600 hover:text-indigo-900">Edit</button>
                <button onClick={() => handleDelete(user.id)} className="ml-4 text-red-600 hover:text-red-900">Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <Modal isOpen={isModalOpen} onClose={closeModal}>
        {selectedUserId && <EditUserForm userId={selectedUserId} closeModal={closeModal} />}
      </Modal>
    </div>
  );
}
