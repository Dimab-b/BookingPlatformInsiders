import type { RoomDto } from '../types';
import { addDaysUtc, formatDateUtc, todayUtc } from '../utils/date';

interface RoomToolbarProps {
  rooms: RoomDto[];
  roomId: string;
  date: string;
  loading: boolean;
  onRoomChange: (roomId: string) => void;
  onDateChange: (date: string) => void;
}

export function RoomToolbar({
  rooms,
  roomId,
  date,
  loading,
  onRoomChange,
  onDateChange
}: RoomToolbarProps) {
  const selectedRoom = rooms.find(room => room.id === roomId);

  return (
    <section className="toolbar card" aria-label="Room and date selection">
      <div className="toolbar-fields">
        <label className="field">
          <span>Room</span>
          <select
            value={roomId}
            onChange={event => onRoomChange(event.target.value)}
            disabled={loading || rooms.length === 0}
          >
            {rooms.length === 0 && <option value="">No rooms available</option>}
            {rooms.map(room => (
              <option key={room.id} value={room.id}>
                {room.name} · capacity {room.capacity}
              </option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>Date (UTC)</span>
          <input
            type="date"
            value={date}
            onChange={event => onDateChange(event.target.value)}
            required
          />
        </label>

        <div className="quick-dates" role="group" aria-label="Quick date selection">
          <button
            type="button"
            className={date === todayUtc() ? 'chip is-active' : 'chip'}
            onClick={() => onDateChange(todayUtc())}
          >
            Today
          </button>
          <button
            type="button"
            className={date === addDaysUtc(todayUtc(), 1) ? 'chip is-active' : 'chip'}
            onClick={() => onDateChange(addDaysUtc(todayUtc(), 1))}
          >
            Tomorrow
          </button>
        </div>
      </div>

      {selectedRoom && (
        <div className="room-meta">
          <div>
            <span className="meta-label">Location</span>
            <strong>{selectedRoom.location}</strong>
          </div>
          <div>
            <span className="meta-label">Capacity</span>
            <strong>{selectedRoom.capacity} people</strong>
          </div>
          <div className="room-meta-description">
            <span className="meta-label">Notes</span>
            <strong>{selectedRoom.description ?? 'No description'}</strong>
          </div>
          <div className="room-meta-date">
            <span className="meta-label">Showing</span>
            <strong>{formatDateUtc(date)}</strong>
          </div>
        </div>
      )}
    </section>
  );
}
