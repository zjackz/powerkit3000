"use client";

import React, { useState, useEffect, useCallback } from 'react';

interface KickstarterProject {
  id: number;
  name: string;
  blurb: string;
  goal: number;
  pledged: number;
  state: string;
  country: string;
  currency: string;
  deadline: string;
  createdAt: string;
  launchedAt: string;
  backersCount: number;
  usdPledged: number;
  creator: {
    id: number;
    name: string;
  };
  category: {
    id: number;
    name: string;
    slug: string;
    parentId?: number;
    parentName?: string;
  };
  location?: {
    id: number;
    name: string;
    displayableName: string;
    country: string;
    state: string;
    type: string;
  };
  photo: string; // JSON string
  urls: string; // JSON string
  stateChangedAt: string;
  slug: string;
  countryDisplayableName: string;
  currencySymbol: string;
  currencyTrailingCode: boolean;
  isInPostCampaignPledgingPhase?: boolean;
  staffPick: boolean;
  isStarrable: boolean;
  disableCommunication: boolean;
  staticUsdRate: number;
  convertedPledgedAmount: number;
  fxRate: number;
  usdExchangeRate: number;
  currentCurrency: string;
  usdType: string;
  spotlight: boolean;
  percentFunded: number;
  isLiked: boolean;
  isDisliked: boolean;
  isLaunched: boolean;
  prelaunchActivated: boolean;
  sourceUrl: string;
}

interface QueryParams {
  state?: string;
  country?: string;
  categoryName?: string;
  projectName?: string;
  minGoal?: number;
  maxGoal?: number;
  minPledged?: number;
  maxPledged?: number;
  minBackersCount?: number;
  maxBackersCount?: number;
  pageNumber: number;
  pageSize: number;
}

const KickstarterQueryPage: React.FC = () => {
  const [projects, setProjects] = useState<KickstarterProject[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [queryParams, setQueryParams] = useState<QueryParams>({
    pageNumber: 1,
    pageSize: 10,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProjects = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const filteredQueryParams: Record<string, string> = {};
      for (const key in queryParams) {
        const value = queryParams[key as keyof QueryParams];
        if (value !== undefined && value !== null) {
          filteredQueryParams[key] = String(value);
        }
      }
      const queryString = new URLSearchParams(filteredQueryParams).toString();
      const response = await fetch(`/api/kickstarter/query?${queryString}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      setProjects(data.projects);
      setTotalCount(data.totalCount);
    } catch (e: unknown) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }, [queryParams]);

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setQueryParams((prev) => ({
      ...prev,
      [name]: type === 'number' ? (value === '' ? undefined : Number(value)) : value,
      pageNumber: 1, // Reset to first page on filter change
    }));
  };

  const handlePageChange = (newPageNumber: number) => {
    setQueryParams((prev) => ({
      ...prev,
      pageNumber: newPageNumber,
    }));
  };

  const totalPages = Math.ceil(totalCount / queryParams.pageSize);

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">Kickstarter Project Query</h1>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div>
          <label htmlFor="projectName" className="block text-sm font-medium text-gray-700">Project Name</label>
          <input
            type="text"
            name="projectName"
            id="projectName"
            value={queryParams.projectName || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="state" className="block text-sm font-medium text-gray-700">State</label>
          <input
            type="text"
            name="state"
            id="state"
            value={queryParams.state || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="country" className="block text-sm font-medium text-gray-700">Country</label>
          <input
            type="text"
            name="country"
            id="country"
            value={queryParams.country || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="categoryName" className="block text-sm font-medium text-gray-700">Category Name</label>
          <input
            type="text"
            name="categoryName"
            id="categoryName"
            value={queryParams.categoryName || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="minGoal" className="block text-sm font-medium text-gray-700">Min Goal</label>
          <input
            type="number"
            name="minGoal"
            id="minGoal"
            value={queryParams.minGoal || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="maxGoal" className="block text-sm font-medium text-gray-700">Max Goal</label>
          <input
            type="number"
            name="maxGoal"
            id="maxGoal"
            value={queryParams.maxGoal || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="minPledged" className="block text-sm font-medium text-gray-700">Min Pledged</label>
          <input
            type="number"
            name="minPledged"
            id="minPledged"
            value={queryParams.minPledged || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="maxPledged" className="block text-sm font-medium text-gray-700">Max Pledged</label>
          <input
            type="number"
            name="maxPledged"
            id="maxPledged"
            value={queryParams.maxPledged || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="minBackersCount" className="block text-sm font-medium text-gray-700">Min Backers</label>
          <input
            type="number"
            name="minBackersCount"
            id="minBackersCount"
            value={queryParams.minBackersCount || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
        <div>
          <label htmlFor="maxBackersCount" className="block text-sm font-medium text-gray-700">Max Backers</label>
          <input
            type="number"
            name="maxBackersCount"
            id="maxBackersCount"
            value={queryParams.maxBackersCount || ''}
            onChange={handleInputChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50"
          />
        </div>
      </div>

      {loading && <p>Loading projects...</p>}
      {error && <p className="text-red-500">Error: {error}</p>}

      {!loading && !error && (
        <>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">State</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Country</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Goal</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Pledged</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Backers</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {projects.map((project) => (
                  <tr key={project.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{project.id}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.name}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.state}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.country}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.category?.name}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.goal}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.pledged}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{project.backersCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <nav
            className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6"
            aria-label="Pagination"
          >
            <div className="hidden sm:block">
              <p className="text-sm text-gray-700">
                Showing <span className="font-medium">{(queryParams.pageNumber - 1) * queryParams.pageSize + 1}</span> to{' '}
                <span className="font-medium">{Math.min(queryParams.pageNumber * queryParams.pageSize, totalCount)}</span> of{' '}
                <span className="font-medium">{totalCount}</span> results
              </p>
            </div>
            <div className="flex flex-1 justify-between sm:justify-end">
              <button
                onClick={() => handlePageChange(queryParams.pageNumber - 1)}
                disabled={queryParams.pageNumber <= 1}
                className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <button
                onClick={() => handlePageChange(queryParams.pageNumber + 1)}
                disabled={queryParams.pageNumber >= totalPages}
                className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </nav>
        </>
      )}
    </div>
  );
};

export default KickstarterQueryPage;
