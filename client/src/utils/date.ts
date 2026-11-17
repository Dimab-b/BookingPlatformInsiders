export function todayUtc(): string {
  return new Date().toISOString().slice(0, 10);
}

export function addDaysUtc(date: string, days: number): string {
  const value = new Date(`${date}T00:00:00Z`);
  value.setUTCDate(value.getUTCDate() + days);

  return value.toISOString().slice(0, 10);
}

export function formatTimeUtc(iso: string): string {
  return new Date(iso).toISOString().slice(11, 16);
}

export function formatDateUtc(date: string): string {
  const value = new Date(`${date}T00:00:00Z`);

  return value.toLocaleDateString(undefined, {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    timeZone: 'UTC'
  });
}
