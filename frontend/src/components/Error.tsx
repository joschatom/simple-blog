import { APIError } from "blog-api";

export function ErrorDisplay({ error }: { error: unknown; marker?: string }) {
  const formatInner = () => {
    let err;
    if ((err = APIError.asDowncast(error, "axios")))
      return (
        <>
          <h2>Failed to call API</h2>
          <p>{err.detail}</p>
          <code>Type: {err.type}</code>
        </>
      );
    else if ((err = APIError.asDowncast(error, "generic")))
      return (
        <>
          <h2>
            {err.status && <>[Status {err.status}]</>}  {err.title}
          </h2>
          <p>{err.detail}</p>
          <code>Type: {err.type}</code>
        </>
      );
    else if ((err = APIError.asDowncast(error, "validation")))
      return (
        <>
          <h2>
            {err.status && <>[Status {err.status}]</>} {err.title}
          </h2>
          <div>
            {Object.entries(err.errors).map(([field, err]) => (
              <span>
                <strong>{field}</strong>: {err}
                <br />
              </span>
            ))}
          </div>
          <code>Type: {err.type}</code>
          <br />
          <code>Trace ID: {err.traceId}</code>
        </>
      );
    else
      return (
        <h3>
          UNKNOWN ERROR:
          <br />
          {JSON.stringify(error, null, 2)}
        </h3>
      );
  };

  return (
    <div
      style={{
        margin: 40,
      }}
    >
      {formatInner()}
    </div>
  );
}
