export function ErrorBanner({ message, onDismiss }: { message: string | null; onDismiss: () => void }) {
  if (!message) {
    return null;
  }

  return (
    <div className="alert alert-error" role="alert">
      <div>
        <strong>Something went wrong</strong>
        <p>{message}</p>
      </div>
      <button className="btn btn-ghost btn-small" type="button" onClick={onDismiss}>
        Dismiss
      </button>
    </div>
  );
}
