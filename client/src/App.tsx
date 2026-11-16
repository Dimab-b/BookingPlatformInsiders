import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { api, describeError } from './api';
import type { BookingDto, RoomDto } from './types';

function todayUtc(): string {
  return new Date().toISOString().slice(0, 10);
}

function formatTime(iso: string): string {
  return new Date(iso).toISOString().slice(11, 16);
}

export default function App() {
  const [rooms, setRooms] = useState<RoomDto[]>([]);
  const [roomId, setRoomId] = useState('');
  const [date, setDate] = useState(todayUtc);
  const [bookings, setBookings] = useState<BookingDto[]>([]);
  const [title, setTitle] = useState('');
  const [startTime, setStartTime] = useState('10:00');
  const [endTime, setEndTime] = useState('11:00');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let active = true;

    void (async () => {
      try {
        const result = await api.getRooms();

        if (!active) {
          return;
        }

        setRooms(result.items);
        setRoomId(current => current || result.items[0]?.id || '');
      } catch (err) {
        if (active) {
          setError(describeError(err));
        }
      }
    })();

    return () => {
      active = false;
    };
  }, []);

  const loadSchedule = useCallback(async (selectedRoomId: string, selectedDate: string) => {
    if (!selectedRoomId) {
      setBookings([]);
      return;
    }

    try {
      const result = await api.getSchedule(selectedRoomId, selectedDate);
      setBookings(result.items);
    } catch (err) {
      setError(describeError(err));
    }
  }, []);

  useEffect(() => {
    void loadSchedule(roomId, date);
  }, [roomId, date, loadSchedule]);

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await api.createBooking({
        roomId,
        title,
        startUtc: `${date}T${startTime}:00Z`,
        endUtc: `${date}T${endTime}:00Z`
      });

      setTitle('');
      await loadSchedule(roomId, date);
    } catch (err) {
      setError(describeError(err));
    } finally {
      setLoading(false);
    }
  }

  async function handleCancel(id: string) {
    setError(null);

    try {
      await api.cancelBooking(id);
      await loadSchedule(roomId, date);
    } catch (err) {
      setError(describeError(err));
    }
  }

  const selectedRoom = rooms.find(room => room.id === roomId);

  return (
    <main className="app">
      <header>
        <h1>Meeting Room Booking</h1>
        <p>Rooms, schedule for a date (UTC), create and cancel bookings.</p>
      </header>

      {error && (
        <div className="error" role="alert">
          <strong>Error</strong>
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)}>Dismiss</button>
        </div>
      )}

      <section className="controls">
        <label>
          Room
          <select value={roomId} onChange={event => setRoomId(event.target.value)}>
            {rooms.map(room => (
              <option key={room.id} value={room.id}>
                {room.name} ({room.capacity}) - {room.location}
              </option>
            ))}
          </select>
        </label>

        <label>
          Date (UTC)
          <input type="date" value={date} onChange={event => setDate(event.target.value)} />
        </label>

        {selectedRoom && (
          <p className="hint">
            {selectedRoom.name}: {selectedRoom.description ?? 'no description'}
          </p>
        )}
      </section>

      <section>
        <h2>Schedule for {date}</h2>

        {bookings.length === 0 ? (
          <p>No active bookings.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Start (UTC)</th>
                <th>End (UTC)</th>
                <th>Title</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {bookings.map(booking => (
                <tr key={booking.id}>
                  <td>{formatTime(booking.startUtc)}</td>
                  <td>{formatTime(booking.endUtc)}</td>
                  <td>{booking.title}</td>
                  <td>{booking.status}</td>
                  <td>
                    <button type="button" onClick={() => void handleCancel(booking.id)}>
                      Cancel
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      <section>
        <h2>Create booking</h2>

        <form onSubmit={handleCreate}>
          <label>
            Title
            <input
              value={title}
              onChange={event => setTitle(event.target.value)}
              maxLength={200}
              required
            />
          </label>

          <label>
            Start (UTC)
            <input
              type="time"
              value={startTime}
              onChange={event => setStartTime(event.target.value)}
              required
            />
          </label>

          <label>
            End (UTC)
            <input
              type="time"
              value={endTime}
              onChange={event => setEndTime(event.target.value)}
              required
            />
          </label>

          <button type="submit" disabled={loading || !roomId}>
            {loading ? 'Creating...' : 'Create'}
          </button>
        </form>
      </section>
    </main>
  );
}
