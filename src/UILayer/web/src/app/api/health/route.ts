import { NextResponse } from 'next/server';

/**
 * GET /api/health
 *
 * Health check endpoint used by Docker HEALTHCHECK and Kubernetes probes.
 * Returns 200 with a JSON body when the frontend is healthy.
 */
export async function GET() {
  return NextResponse.json(
    {
      status: 'healthy',
      timestamp: new Date().toISOString(),
    },
    { status: 200 },
  );
}
