'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { jwtDecode } from 'jwt-decode';

const withAuth = (WrappedComponent: React.ComponentType) => {
  const AuthComponent = (props: any) => {
    const router = useRouter();
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    useEffect(() => {
      const token = localStorage.getItem('token');
      if (!token) {
        router.push('/login');
        return;
      }

      try {
        const decodedToken: { exp: number, role: string | string[] } = jwtDecode(token);
        if (decodedToken.exp * 1000 < Date.now()) {
          router.push('/login');
          return;
        }
        
        const roles = Array.isArray(decodedToken.role) ? decodedToken.role : [decodedToken.role];
        if (!roles.includes('Admin')) {
            router.push('/'); // Or a dedicated unauthorized page
            return;
        }

        setIsAuthenticated(true);
      } catch (error) {
        router.push('/login');
      }
    }, [router]);

    if (!isAuthenticated) {
      return <div>Loading...</div>; // Or a proper loader
    }

    return <WrappedComponent {...props} />;
  };

  return AuthComponent;
};

export default withAuth;
