'use client';

import React from 'react';

interface Session {
  sessionId: string;
  title: string;
  status: 'scheduled' | 'active' | 'completed' | 'cancelled';
  startedAt: string;
  participants: number;
}

interface SessionTimelineProps {
  /** Sessions to display in the timeline. */
  sessions: Session[];
}

function getStatusColor(status: Session['status']): string {
  switch (status) {
    case 'active':
      return 'bg-green-500';
    case 'completed':
      return 'bg-blue-500';
    case 'cancelled':
      return 'bg-red-500';
    case 'scheduled':
    default:
      return 'bg-gray-500';
  }
}

function getStatusLabel(status: Session['status']): string {
  switch (status) {
    case 'active':
      return 'Active';
    case 'completed':
      return 'Completed';
    case 'cancelled':
      return 'Cancelled';
    case 'scheduled':
    default:
      return 'Scheduled';
  }
}

/**
 * Timeline component showing convener session history.
 */
export default function SessionTimeline({ sessions }: SessionTimelineProps) {
  if (sessions.length === 0) {
    return <p className="text-sm text-gray-500">No sessions recorded yet.</p>;
  }

  return (
    <div className="relative space-y-0" role="list" aria-label="Session timeline">
      {/* Vertical line */}
      <div className="absolute left-[7px] top-2 bottom-2 w-px bg-white/10" />

      {sessions.map((session) => (
        <div key={session.sessionId} className="relative flex gap-4 py-3" role="listitem">
          {/* Dot */}
          <div className={`relative z-10 mt-1 h-4 w-4 rounded-full border-2 border-black/80 ${getStatusColor(session.status)}`} />

          {/* Content */}
          <div className="flex-1">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-200">{session.title}</span>
              <span className={`inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[10px] font-medium ${
                session.status === 'active'
                  ? 'bg-green-500/20 text-green-400'
                  : session.status === 'completed'
                    ? 'bg-blue-500/20 text-blue-400'
                    : 'bg-gray-500/20 text-gray-400'
              }`}>
                {getStatusLabel(session.status)}
              </span>
            </div>
            <div className="mt-0.5 flex gap-3 text-xs text-gray-500">
              <span>{new Date(session.startedAt).toLocaleString()}</span>
              <span>{session.participants} participant{session.participants !== 1 ? 's' : ''}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
