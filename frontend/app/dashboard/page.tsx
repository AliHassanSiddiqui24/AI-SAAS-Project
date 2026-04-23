'use client';

import { useAuth } from '../context/AuthContext';
import { Users, TrendingUp, Calendar, DollarSign } from 'lucide-react';

export default function Dashboard() {
  const { user } = useAuth();

  const stats = [
    {
      name: 'Total Contacts',
      value: '1,234',
      change: '+12%',
      changeType: 'positive',
      icon: Users,
    },
    {
      name: 'Active Deals',
      value: '89',
      change: '+4%',
      changeType: 'positive',
      icon: DollarSign,
    },
    {
      name: 'Meetings Today',
      value: '5',
      change: '-2',
      changeType: 'negative',
      icon: Calendar,
    },
    {
      name: 'Conversion Rate',
      value: '23%',
      change: '+2%',
      changeType: 'positive',
      icon: TrendingUp,
    },
  ];

  return (
    <div className="p-6">
      {/* Welcome message */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">
          Welcome back, {user?.firstName}!
        </h1>
        <p className="text-gray-600 mt-1">
          Here's what's happening with your business today.
        </p>
      </div>

      {/* Stats grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div key={stat.name} className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">{stat.name}</p>
                  <p className="text-2xl font-semibold text-gray-900 mt-1">{stat.value}</p>
                </div>
                <div className="p-3 bg-blue-50 rounded-lg">
                  <Icon className="w-6 h-6 text-blue-600" />
                </div>
              </div>
              <div className="mt-4">
                <span className={`text-sm font-medium ${
                  stat.changeType === 'positive' ? 'text-green-600' : 'text-red-600'
                }`}>
                  {stat.change}
                </span>
                <span className="text-sm text-gray-500 ml-1">from last month</span>
              </div>
            </div>
          );
        })}
      </div>

      {/* Recent activity and quick actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent activity */}
        <div className="bg-white rounded-lg shadow">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">Recent Activity</h2>
          </div>
          <div className="p-6">
            <div className="space-y-4">
              {[
                { time: '2 hours ago', action: 'New contact added: John Doe', type: 'contact' },
                { time: '4 hours ago', action: 'Meeting scheduled with ABC Corp', type: 'meeting' },
                { time: '6 hours ago', action: 'Deal closed: $50,000', type: 'deal' },
                { time: '1 day ago', action: 'Follow-up email sent to Jane Smith', type: 'email' },
              ].map((activity, index) => (
                <div key={index} className="flex items-start space-x-3">
                  <div className="w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
                  <div className="flex-1">
                    <p className="text-sm text-gray-900">{activity.action}</p>
                    <p className="text-xs text-gray-500 mt-1">{activity.time}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Quick actions */}
        <div className="bg-white rounded-lg shadow">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">Quick Actions</h2>
          </div>
          <div className="p-6">
            <div className="grid grid-cols-2 gap-4">
              {[
                { name: 'Add Contact', href: '/dashboard/contacts/new' },
                { name: 'Schedule Meeting', href: '/dashboard/calendar/new' },
                { name: 'Create Deal', href: '/dashboard/deals/new' },
                { name: 'Send Email', href: '/dashboard/emails/new' },
              ].map((action) => (
                <a
                  key={action.name}
                  href={action.href}
                  className="p-4 text-center border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <p className="text-sm font-medium text-gray-900">{action.name}</p>
                </a>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
