'use client';

import { useQuery } from '@tanstack/react-query';
import axios from '../../lib/axios';
import { useAuth } from '../context/AuthContext';
import { Users, TrendingUp, DollarSign, Trophy } from 'lucide-react';

interface DashboardStats {
  totalClients: number;
  activeDeals: number;
  pipelineValue: number;
  dealsWonThisMonth: number;
}

const fetchDashboardStats = async (): Promise<DashboardStats> => {
  try {
    const [clientsResponse, dealsResponse] = await Promise.all([
      axios.get('/clients'),
      axios.get('/deals')
    ]);

    const clients = clientsResponse.data?.data || clientsResponse.data || [];
    const deals = dealsResponse.data?.data || dealsResponse.data || [];

  const totalClients = clients.length;
  const activeDeals = deals.filter((deal: any) => deal.status !== 'won' && deal.status !== 'lost').length;
  const pipelineValue = deals
    .filter((deal: any) => deal.status !== 'won' && deal.status !== 'lost')
    .reduce((sum: number, deal: any) => sum + (deal.value || 0), 0);
  
  const currentMonth = new Date().getMonth();
  const currentYear = new Date().getFullYear();
  const dealsWonThisMonth = deals
    .filter((deal: any) => {
      const dealDate = new Date(deal.createdAt);
      return deal.status === 'won' && 
             dealDate.getMonth() === currentMonth && 
             dealDate.getFullYear() === currentYear;
    })
    .reduce((sum: number, deal: any) => sum + (deal.value || 0), 0);

  return {
    totalClients,
    activeDeals,
    pipelineValue,
    dealsWonThisMonth
  };
  } catch (error) {
    console.error('Dashboard data fetch error:', error);
    // Return default values on error
    return {
      totalClients: 0,
      activeDeals: 0,
      pipelineValue: 0,
      dealsWonThisMonth: 0
    };
  }
};

const StatCard = ({ 
  title, 
  value, 
  icon: Icon, 
  trend, 
  isLoading 
}: { 
  title: string; 
  value: string | number; 
  icon: any; 
  trend?: string; 
  isLoading: boolean;
}) => {
  return (
    <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-gray-400 text-sm font-medium">{title}</p>
          <div className="text-2xl font-bold text-white mt-2">
            {isLoading ? (
              <div className="h-8 w-24 bg-gray-700 rounded animate-pulse"></div>
            ) : (
              value
            )}
          </div>
          {trend && !isLoading && (
            <p className="text-green-400 text-sm mt-1">{trend}</p>
          )}
        </div>
        <div className="bg-gray-700 p-3 rounded-lg">
          <Icon className="h-6 w-6 text-gray-300" />
        </div>
      </div>
    </div>
  );
};

export default function Dashboard() {
  const { user, isLoading: authLoading } = useAuth();
  
  const { data: stats, isLoading, error } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: fetchDashboardStats,
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!user, // Only run query when user is authenticated
  });

  // Show loading state while auth is initializing
  if (authLoading) {
    return (
      <div className="min-h-screen bg-gray-900 p-8">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold text-white mb-8">Dashboard</h1>
          <div className="text-gray-400">Loading authentication...</div>
        </div>
      </div>
    );
  }

  // Show error if not authenticated
  if (!user) {
    return (
      <div className="min-h-screen bg-gray-900 p-8">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold text-white mb-8">Dashboard</h1>
          <div className="bg-yellow-900 border border-yellow-700 text-yellow-200 px-4 py-3 rounded">
            Please log in to access the dashboard.
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    console.error('Dashboard query error:', error);
    return (
      <div className="min-h-screen bg-gray-900 p-8">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold text-white mb-8">Dashboard</h1>
          <div className="bg-red-900 border border-red-700 text-red-200 px-4 py-3 rounded">
            Error loading dashboard data: {error.message || 'Please try again later.'}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 p-8">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-white mb-8">Dashboard</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard
            title="Total Clients"
            value={stats?.totalClients || 0}
            icon={Users}
            isLoading={isLoading}
          />
          <StatCard
            title="Active Deals"
            value={stats?.activeDeals || 0}
            icon={TrendingUp}
            isLoading={isLoading}
          />
          <StatCard
            title="Pipeline Value"
            value={`$${(stats?.pipelineValue || 0).toLocaleString()}`}
            icon={DollarSign}
            isLoading={isLoading}
          />
          <StatCard
            title="Deals Won This Month"
            value={`$${(stats?.dealsWonThisMonth || 0).toLocaleString()}`}
            icon={Trophy}
            isLoading={isLoading}
          />
        </div>

        {/* Recent Activity Section */}
        <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
          <h2 className="text-xl font-semibold text-white mb-4">Quick Actions</h2>
          <div className="flex flex-wrap gap-4">
            <a 
              href="/dashboard/clients" 
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-colors"
            >
              View All Clients
            </a>
            <a 
              href="/dashboard/deals" 
              className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg transition-colors"
            >
              Manage Deals
            </a>
            <button className="bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded-lg transition-colors">
              Generate Report
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
