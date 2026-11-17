import { useEffect, useState } from 'react';
import { api, describeError } from './api';
import type { CreateBookingRequest } from './api';
import { useAuth } from './auth';
import type { UserDto } from './types';
import { BookingForm } from './components/BookingForm';
import { ErrorBanner } from './components/ErrorBanner';
import { LoginScreen } from './components/LoginScreen';
import { RoomToolbar } from './components/RoomToolbar';
import { ScheduleTable } from './components/ScheduleTable';
import { TopBar } from './components/TopBar';
import type { BookingDto, RoomDto } from './types';
import { todayUtc } from './utils/date';

export default function App() {
  const { user, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return (
      <div className="splash">
        <span className="spinner spinner-large" aria-hidden="true" />
        <p>Restoring your session…</p>
      </div>
    );
  }

  if (!user) {
    return <LoginScreen />;
  }

  return <MainScreen user={user} />;
}

function MainScreen({ user }: { user: UserDto }) {
  const { logout, logoutAll } = useAuth();
  const [rooms, setRooms] = useState<RoomDto[]>([]);
  const [roomsLoading, setRoomsLoading] = useState(true);
  const [roomId, setRoomId] = useState('');
  const [date, setDate] = useState(todayUtc);
  const [bookings, setBookings] = useState<BookingDto[]>([]);
  const [scheduleLoading, setScheduleLoading] = useState(false);
  const [cancellingId, setCancellingId] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    setRoomsLoading(true);

    api.getRooms()
      .then(result => {
        if (!active) {
          return;
        }

        setRooms(result.items);
        setRoomId(current => current || result.items[0]?.id || '');
      })
      .catch(err => {
        if (active) {
          setError(describeError(err));
        }
      })
      .finally(() => {
        if (active) {
          setRoomsLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!roomId || !date) {
      setBookings([]);
      return;
    }

    let active = true;
    setScheduleLoading(true);

    api.getSchedule(roomId, date)
      .then(result => {
        if (active) {
          setBookings(result.items);
        }
      })
      .catch(err => {
        if (active) {
          setError(describeError(err));
        }
      })
      .finally(() => {
        if (active) {
          setScheduleLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [roomId, date, reloadToken]);

  async function handleCreate(request: CreateBookingRequest) {
    await api.createBooking(request);
    setReloadToken(value => value + 1);
  }

  async function handleCancel(id: string) {
    setError(null);
    setCancellingId(id);

    try {
      await api.cancelBooking(id);
      setReloadToken(value => value + 1);
    } catch (err) {
      setError(describeError(err));
    } finally {
      setCancellingId(null);
    }
  }

  async function handleLogout() {
    try {
      await logout();
    } catch {
      // Session state is cleared regardless; the login screen will be shown.
    }
  }

  async function handleLogoutAll() {
    try {
      await logoutAll();
    } catch {
      // Session state is cleared regardless; the login screen will be shown.
    }
  }

  return (
    <div className="app-shell">
      <TopBar user={user} onLogout={handleLogout} onLogoutAll={handleLogoutAll} />

      <main className="content">
        <ErrorBanner message={error} onDismiss={() => setError(null)} />

        <RoomToolbar
          rooms={rooms}
          roomId={roomId}
          date={date}
          loading={roomsLoading}
          onRoomChange={value => {
            setError(null);
            setRoomId(value);
          }}
          onDateChange={value => {
            setError(null);
            setDate(value);
          }}
        />

        <div className="workspace">
          <ScheduleTable
            bookings={bookings}
            loading={scheduleLoading}
            cancellingId={cancellingId}
            onCancel={id => void handleCancel(id)}
          />

          <BookingForm roomId={roomId} date={date} onCreate={handleCreate} />
        </div>
      </main>
    </div>
  );
}
