import { APIError } from "blog-api";
import { ComponentRef, useRef } from "react";
import Button from "./Button";

import CloseIcon from "../assets/icons/close.svg?react";

import "../styles/components/Error.css";
import { ZodError } from "zod";

export function ErrorDisplay({ error }: { error: unknown; marker?: string }) {
  const ref = useRef<ComponentRef<"dialog">>(null);

  if (error == undefined) return <></>;

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
            {err.status && <>[Status {err.status}]</>} {err.title}
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
    else if (error instanceof ZodError) {
      return <>{JSON.parse(error.message)[0].message}</>;
    } else
      return (
        <>
          {typeof error === "object"
            ? JSON.stringify(error, null, 2)
            : error.toString()}
        </>
      );
  };

  return (
    <dialog
      open
      ref={ref}
      style={{
        margin: 40,
      }}
    >
      {formatInner()}
      <CloseIcon onClick={() => ref.current?.close()} />
    </dialog>
  );
}
