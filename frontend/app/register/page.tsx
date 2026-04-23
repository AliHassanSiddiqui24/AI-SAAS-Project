'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '../context/AuthContext';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2, Eye, EyeOff, Mail, Lock, User, Building } from 'lucide-react';
import Link from 'next/link';
import { NoSSR } from '../components/NoSSR';

// Validation schema
const registerSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .min(2, 'Name must be at least 2 characters'),
  companyName: z
    .string()
    .min(1, 'Company name is required')
    .min(2, 'Company name must be at least 2 characters'),
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address'),
  password: z
    .string()
    .min(1, 'Password is required')
    .min(8, 'Password must be at least 8 characters')
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/, 'Password must contain at least one uppercase letter, one lowercase letter, and one number'),
});

// Password strength indicator
const getPasswordStrength = (password: string) => {
  if (!password) return { score: 0, text: '', color: 'bg-gray-300' };
  
  let score = 0;
  const checks = {
    length: password.length >= 8,
    lowercase: /[a-z]/.test(password),
    uppercase: /[A-Z]/.test(password),
    number: /\d/.test(password),
    special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
  };
  
  Object.values(checks).forEach(check => {
    if (check) score += 20;
  });
  
  const percentage = (score / 100) * 100;
  
  if (percentage <= 40) return { score: percentage, text: 'Weak', color: 'bg-red-500' };
  if (percentage <= 60) return { score: percentage, text: 'Fair', color: 'bg-yellow-500' };
  if (percentage <= 80) return { score: percentage, text: 'Good', color: 'bg-blue-500' };
  return { score: percentage, text: 'Strong', color: 'bg-green-500' };
};

type RegisterFormData = z.infer<typeof registerSchema>;

export default function Register() {
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [authError, setAuthError] = useState('');
  const [passwordStrength, setPasswordStrength] = useState({ score: 0, text: '', color: 'bg-gray-300' });

  const { register: registerUser } = useAuth();
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      name: '',
      companyName: '',
      email: '',
      password: '',
    },
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsSubmitting(true);
    setAuthError('');

    // Log form data for debugging
    console.log('Registration attempt with data:', {
      name: data.name,
      email: data.email,
      companyName: data.companyName,
      passwordLength: data.password.length
    });

    try {
      const response = await registerUser({
        name: data.name,
        companyName: data.companyName,
        email: data.email,
        password: data.password,
      });
      
      console.log('Registration successful:', response);
      router.push('/dashboard');
    } catch (err: any) {
      console.error('Registration error:', err);
      
      // Enhanced error handling
      let errorMessage = 'Registration failed. Please try again.';
      
      if (err.response?.data) {
        if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        } else if (err.response.data.error) {
          errorMessage = err.response.data.error;
        } else if (err.response.data.message) {
          errorMessage = err.response.data.message;
        }
      } else if (err.message) {
        errorMessage = err.message;
      }
      
      setAuthError(errorMessage);
      reset({ password: '' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        {/* Header */}
        <div className="text-center">
          <h2 className="mt-6 text-3xl font-bold text-white">
            Create your account
          </h2>
          <p className="mt-2 text-sm text-gray-400">
            Get started with AI CRM Pro today
          </p>
        </div>

        {/* Register Form */}
        <NoSSR>
          <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)}>
          {/* Debug Information */}
          <div className="bg-gray-800 border border-gray-700 rounded-md p-4 mb-4">
            <h3 className="text-sm font-medium text-white mb-2">🔍 Debug Information</h3>
            <div className="text-xs text-gray-300 space-y-1">
              <p>• Backend API: http://localhost:5035/api/v1/auth/register</p>
              <p>• Password must have: 1 uppercase, 1 lowercase, 1 number, min 8 chars</p>
              <p>• Check browser console (F12) for detailed error messages</p>
              <p>• Example working data:</p>
              <pre className="bg-gray-900 p-2 rounded text-green-400 text-xs">
{`{
  "name": "Test User",
  "email": "test@example.com", 
  "password": "TestPass123",
  "companyName": "Test Company"
}`}
              </pre>
            </div>
          </div>

          {/* Auth Error */}
          {authError && (
            <div className="bg-red-900/50 border border-red-500 rounded-md p-4">
              <p className="text-sm text-red-200">❌ {authError}</p>
            </div>
          )}

          <div className="space-y-5">
            {/* Name Field */}
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-300 mb-2">
                  Full name
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    id="name"
                    type="text"
                    autoComplete="name"
                    {...register('name')}
                    className={`
                      block w-full pl-10 pr-3 py-3 bg-gray-800 border rounded-md text-white placeholder-gray-500
                      focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                      ${errors.name ? 'border-red-500' : 'border-gray-700'}
                    `}
                    placeholder="John Doe"
                  />
                </div>
                {errors.name && (
                  <p className="mt-2 text-sm text-red-400">{errors.name.message}</p>
                )}
              </div>

            {/* Company Name Field */}
            <div>
              <label htmlFor="companyName" className="block text-sm font-medium text-gray-300 mb-2">
                Company name
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Building className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="companyName"
                  type="text"
                  autoComplete="organization"
                  {...register('companyName')}
                  className={`
                    block w-full pl-10 pr-3 py-3 bg-gray-800 border rounded-md text-white placeholder-gray-500
                    focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                    ${errors.companyName ? 'border-red-500' : 'border-gray-700'}
                  `}
                  placeholder="Acme Corp"
                />
              </div>
              {errors.companyName && (
                <p className="mt-2 text-sm text-red-400">{errors.companyName.message}</p>
              )}
            </div>

            {/* Email Field */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-300 mb-2">
                Email address
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="email"
                  type="email"
                  autoComplete="email"
                  {...register('email')}
                  className={`
                    block w-full pl-10 pr-3 py-3 bg-gray-800 border rounded-md text-white placeholder-gray-500
                    focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                    ${errors.email ? 'border-red-500' : 'border-gray-700'}
                  `}
                  placeholder="you@example.com"
                />
              </div>
              {errors.email && (
                <p className="mt-2 text-sm text-red-400">{errors.email.message}</p>
              )}
            </div>

            {/* Password Field */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-300 mb-2">
                Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  {...register('password')}
                  onChange={(e) => {
                    const value = e.target.value;
                    const strength = getPasswordStrength(value);
                    setPasswordStrength(strength);
                  }}
                  className={`
                    block w-full pl-10 pr-10 py-3 bg-gray-800 border rounded-md text-white placeholder-gray-500
                    focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                    ${errors.password ? 'border-red-500' : 'border-gray-700'}
                  `}
                  placeholder="•••••••"
                />
                <button
                  type="button"
                  className="absolute inset-y-0 right-0 pr-3 flex items-center"
                  onClick={() => setShowPassword(!showPassword)}
                >
                  {showPassword ? (
                    <EyeOff className="h-5 w-5 text-gray-400 hover:text-gray-300" />
                  ) : (
                    <Eye className="h-5 w-5 text-gray-400 hover:text-gray-300" />
                  )}
                </button>
              </div>
              {errors.password && (
                <p className="mt-2 text-sm text-red-400">{errors.password.message}</p>
              )}
              <p className={`mt-2 text-xs ${passwordStrength.score > 0 ? passwordStrength.color : 'text-gray-500'}`}>
                {passwordStrength.score > 0 
                  ? `Password strength: ${passwordStrength.text} (${passwordStrength.score}%)`
                  : 'Must contain at least 8 characters, one uppercase, one lowercase, and one number'
                }
              </p>
            </div>
          </div>

          {/* Submit Button */}
          <div>
            <button
              type="submit"
              disabled={isSubmitting}
              className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 focus:ring-offset-gray-900 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Creating account...
                </>
              ) : (
                'Create account'
              )}
            </button>
          </div>

          {/* Login Link */}
          <div className="text-center">
            <p className="text-sm text-gray-400">
              Already have an account?{' '}
              <Link href="/login" className="font-medium text-blue-400 hover:text-blue-300">
                Sign in
              </Link>
            </p>
          </div>
        </form>
          </NoSSR>
      </div>
    </div>
  );
}
