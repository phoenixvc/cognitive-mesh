"use client"

import { useRef } from 'react';
import ImpactMetricsDashboard from '@/components/widgets/ImpactMetrics/ImpactMetricsDashboard';
import { ExportMenu } from '@/components/shared/ExportMenu';

export default function ImpactPage() {
  const dashRef = useRef<HTMLDivElement>(null);

  return (
    <>
      <div className="flex items-center justify-end mb-4">
        <ExportMenu filename="impact-metrics" targetRef={dashRef} />
      </div>
      <div ref={dashRef}>
        <ImpactMetricsDashboard />
      </div>
    </>
  );
}
