import { useState } from 'react';
import type { UserDto } from '../types';

interface TopBarProps {
  user: UserDto;
  onLogout: () => Promise<void>;
  onLogoutAll: () => Promise<void>;
}

export function TopBar({ user, onLogout, onLogoutAll }: TopBarProps) {
  const [busy, setBusy] = useState<'logout' | 'logout-all' | null>(null);

  async function handleLogout() {
    setBusy('logout');

    try {
      await onLogout();
    } finally {
      setBusy(null);
    }
  }

  async function handleLogoutAll() {
    if (!window.confirm('Sign out of all sessions and devices?')) {
      return;
    }

    setBusy('logout-all');

    try {
      await onLogoutAll();
    } finally {
      setBusy(null);
    }
  }

  const initials = user.email.slice(0, 1).toUpperCase();

  return (
    <header className="topbar">
      <div className="topbar-brand">
        <span className="brand-mark brand-mark-small" aria-hidden="true">MR</span>
        <div>
          <strong>Meeting Rooms</strong>
          <span className="topbar-subtitle">Booking console</span>
        </div>
      </div>

      <div className="topbar-user">
        <span className="avatar" aria-hidden="true">{initials}</span>
        <div className="topbar-identity">
          <strong>{user.email}</strong>
          <span className="role-badge">{user.role}</span>
        </div>
        <div className="topbar-actions">
          <button
            className="btn btn-secondary btn-small"
            type="button"
            onClick={() => void handleLogout()}
            disabled={busy !== null}
          >
            {busy === 'logout' ? 'Signing out…' : 'Sign out'}
          </button>
          <button
            className="btn btn-ghost btn-small"
            type="button"
            onClick={() => void handleLogoutAll()}
            disabled={busy !== null}
          >
            {busy === 'logout-all' ? 'Signing out everywhere…' : 'Sign out everywhere'}
          </button>
        </div>
      </div>
    </header>
  );
}
