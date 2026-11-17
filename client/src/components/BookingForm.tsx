import { useState } from 'react';
import type { FormEvent } from 'react';
import { describeError } from '../api';
import type { CreateBookingRequest } from '../api';
import { todayUtc } from '../utils/date';

interface BookingFormProps {
  roomId: string;
  date: string;
  onCreate: (request: CreateBookingRequest) => Promise<void>;
}

export function BookingForm({ roomId, date, onCreate }: BookingFormProps) {
  const [title, setTitle] = useState('');
  const [startTime, setStartTime] = useState('10:00');
  const [endTime, setEndTime] = useState('11:00');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const trimmedTitle = title.trim();

    if (!trimmedTitle) {
      setError('Please enter a meeting title.');
      return;
    }

    if (!startTime || !endTime) {
      setError('Please choose start and end times.');
      return;
    }

    if (endTime <= startTime) {
      setError('End time must be after start time.');
      return;
    }

    const startUtc = `${date}T${startTime}:00Z`;
    const endUtc = `${date}T${endTime}:00Z`;
    const start = new Date(startUtc);

    if (date === todayUtc() && start.getTime() <= Date.now()) {
      setError('The booking must start in the future.');
      return;
    }

    setSubmitting(true);

    try {
      await onCreate({
        roomId,
        title: trimmedTitle,
        startUtc,
        endUtc
      });
      setTitle('');
    } catch (err) {
      setError(describeError(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="card form-card" aria-labelledby="create-title">
      <div className="card-header">
        <div>
          <h2 id="create-title">New booking</h2>
          <p className="muted">Times are interpreted as UTC.</p>
        </div>
      </div>

      {error && (
        <div className="alert alert-error" role="alert">
          <p>{error}</p>
        </div>
      )}

      <form className="booking-form" onSubmit={handleSubmit} noValidate>
        <label className="field">
          <span>Meeting title</span>
          <input
            value={title}
            onChange={event => setTitle(event.target.value)}
            maxLength={200}
            placeholder="e.g. Sprint planning"
            required
          />
        </label>

        <div className="time-grid">
          <label className="field">
            <span>Start (UTC)</span>
            <input
              type="time"
              value={startTime}
              onChange={event => setStartTime(event.target.value)}
              step={60}
              required
            />
          </label>

          <label className="field">
            <span>End (UTC)</span>
            <input
              type="time"
              value={endTime}
              onChange={event => setEndTime(event.target.value)}
              step={60}
              required
            />
          </label>
        </div>

        <button
          className="btn btn-primary btn-block"
          type="submit"
          disabled={submitting || !roomId || !date}
        >
          {submitting ? 'Creating booking…' : 'Create booking'}
        </button>
      </form>
    </section>
  );
}
