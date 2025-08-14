'use client';

import UserList from '../components/UserList';
import CreateUserForm from '../components/CreateUserForm';
import RoleManager from '../components/RoleManager';
import withAuth from '../components/withAuth';

function AdminPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Admin Dashboard</h1>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div>
          <UserList />
        </div>
        <div>
          <CreateUserForm />
          <RoleManager />
        </div>
      </div>
    </div>
  );
}

export default withAuth(AdminPage);
