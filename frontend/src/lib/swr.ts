import useSWR from 'swr';

export const fetcher = (url: string) => {
  const token = localStorage.getItem('token');
  return fetch(url, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  }).then((res) => {
    if (res.status === 401) {
      // Handle unauthorized access, e.g., redirect to login
      window.location.href = '/login';
    }
    return res.json();
  });
};

export const useApi = (path: string) => {
  return useSWR(path, fetcher);
};