"use client"

import NistComplianceDashboard from '@/components/widgets/NistCompliance/NistComplianceDashboard';
import { RoleGuard } from '@/components/auth/RoleGuard';

export default function CompliancePage() {
  return (
    <RoleGuard requiredRoles={['admin', 'compliance-officer', 'security']}>
      <NistComplianceDashboard />
    </RoleGuard>
  );
}
