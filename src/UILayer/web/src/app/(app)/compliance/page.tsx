"use client"

import { useRef } from 'react';
import NistComplianceDashboard from '@/components/widgets/NistCompliance/NistComplianceDashboard';
import { RoleGuard } from '@/components/auth/RoleGuard';
import { ExportMenu } from '@/components/shared/ExportMenu';

export default function CompliancePage() {
  const dashRef = useRef<HTMLDivElement>(null);

  return (
    <RoleGuard requiredRoles={['admin', 'compliance-officer', 'security']}>
      <div className="flex items-center justify-end mb-4">
        <ExportMenu filename="nist-compliance" targetRef={dashRef} />
      </div>
      <div ref={dashRef}>
        <NistComplianceDashboard />
      </div>
    </RoleGuard>
  );
}
