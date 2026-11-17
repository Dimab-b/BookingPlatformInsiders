import type { BookingDto } from '../types';
import { formatTimeUtc } from '../utils/date';

interface ScheduleTableProps {
  bookings: BookingDto[];
  loading: boolean;
  cancellingId: string | null;
  onCancel: (id: string) => void;
}

export function ScheduleTable({ bookings, loading, cancellingId, onCancel }: ScheduleTableProps) {
  return (
    <section className="card schedule-card" aria-labelledby="schedule-title">
      <div className="card-header">
        <div>
          <h2 id="schedule-title">Schedule</h2>
          <p className="muted">
            Active bookings for the selected room and UTC day, ordered by start time.
          </p>
        </div>
        <span className="counter" aria-label={`${bookings.length} bookings`}>
          {bookings.length}
        </span>
      </div>

      {loading ? (
        <div className="empty-state">
          <span className="spinner" aria-hidden="true" />
          <p>Loading schedule…</p>
        </div>
      ) : bookings.length === 0 ? (
        <div className="empty-state">
          <strong>No active bookings</strong>
          <p>This room is free for the selected day. Be the first to book it.</p>
        </div>
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th scope="col">Time (UTC)</th>
                <th scope="col">Title</th>
                <th scope="col">Status</th>
                <th scope="col" className="cell-actions">Actions</th>
              </tr>
            </thead>
            <tbody>
              {bookings.map(booking => (
                <tr key={booking.id} className={booking.isOwnedByCurrentUser ? 'is-own' : undefined}>
                  <td className="cell-time">
                    <strong>{formatTimeUtc(booking.startUtc)}</strong>
                    <span className="time-separator">–</span>
                    <span>{formatTimeUtc(booking.endUtc)}</span>
                  </td>
                  <td>
                    <div className="booking-title">
                      <span>{booking.title}</span>
                      {booking.isOwnedByCurrentUser && <span className="badge badge-own">Yours</span>}
                    </div>
                  </td>
                  <td>
                    <span className={`badge badge-${booking.status.toLowerCase()}`}>
                      {booking.status}
                    </span>
                  </td>
                  <td className="cell-actions">
                    {booking.isOwnedByCurrentUser ? (
                      <button
                        type="button"
                        className="btn btn-danger-ghost btn-small"
                        onClick={() => onCancel(booking.id)}
                        disabled={cancellingId !== null}
                      >
                        {cancellingId === booking.id ? 'Cancelling…' : 'Cancel'}
                      </button>
                    ) : (
                      <span className="muted">Not yours</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
