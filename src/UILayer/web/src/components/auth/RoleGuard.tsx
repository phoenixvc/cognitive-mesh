'use client';

import React from 'react';
import { useAuth } from '@/contexts/AuthContext';

interface RoleGuardProps {
  /** Roles that are allowed to view the content. User needs at least one. */
  requiredRoles: string[];
  /** Content rendered when the user has the required role. */
  children: React.ReactNode;
  /** Optional custom fallback. Defaults to an "Access Denied" message. */
  fallback?: React.ReactNode;
}

/**
 * FE-023: Role-based access guard component.
 *
 * Reads the authenticated user's roles from AuthContext and renders children
 * only if the user has at least one of the required roles.  Otherwise, a
 * configurable fallback (defaulting to an "Access Denied" message) is shown.
 */
export function RoleGuard({ requiredRoles, children, fallback }: RoleGuardProps) {
  const { user, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-12" aria-busy="true">
        <div className="h-6 w-6 animate-spin rounded-full border-2 border-cyan-500 border-t-transparent" />
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return (
      fallback ?? (
        <div className="rounded-lg border border-yellow-500/30 bg-yellow-500/10 p-6 text-center" role="alert">
          <h2 className="text-lg font-semibold text-yellow-400">Authentication Required</h2>
          <p className="mt-1 text-sm text-yellow-300">
            Please sign in to access this content.
          </p>
        </div>
      )
    );
  }

  const hasRole = requiredRoles.length === 0 || requiredRoles.some((role) => user.roles.includes(role));

  if (!hasRole) {
    return (
      fallback ?? (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6 text-center" role="alert">
          <h2 className="text-lg font-semibold text-red-400">Access Denied</h2>
          <p className="mt-1 text-sm text-red-300">
            You do not have the required permissions to view this page.
          </p>
          <p className="mt-2 text-xs text-gray-500">
            Required role: {requiredRoles.join(' or ')}
          </p>
        </div>
      )
    );
  }

  return <>{children}</>;
}
