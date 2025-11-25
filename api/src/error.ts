import { AxiosError, AxiosHeaders, type AxiosResponse } from "axios";
import z, { ZodError } from "zod";
import { ProblemDetailsError, ValidationErrors } from "./schemas/errors.ts";

export type APIErrorKind = "validation" | "generic" | "axios";
export declare const APIErrorKind: APIErrorKind;

type APIErrorInner<T extends APIErrorKind> = T extends "validation"
  ? ValidationErrors
  : T extends "axios"
  ? Omit<ProblemDetailsError, "status">
  : ProblemDetailsError;

export class APIError {
  public kind: APIErrorKind;
  public inner: APIErrorInner<typeof this.kind>;

  constructor(kind: APIErrorKind, inner: APIErrorInner<typeof kind>) {
    this.inner = inner;
    this.kind = kind;
  }

  public toString = (): string => {
    return JSON.stringify({ kind: this.kind, ...this.inner }, null, 2);
  };

  public downcast<T extends APIErrorKind>(
    kind: T
  ): APIErrorInner<T> | undefined {
    if (this.kind === kind) return this.inner as APIErrorInner<typeof kind>;
    else return undefined;
  }

  static asDowncast<T extends APIErrorKind>(
    err: any,
    kind: T
  ): APIErrorInner<T> | undefined {
    if (err instanceof APIError) return err.downcast<T>(kind);
    else return undefined;
  }
}

function handleAPIErrorThrowing(err: AxiosError) {
  const resp = err.response;

  if (!resp)
    throw new APIError("axios", {
      type: err.message,
      title: err.name
    });

  try {
    const error = z.parse(ValidationErrors, JSON.parse(resp.data as string));
    throw new APIError("validation", error);
  } catch (e) {
    console.log(e);
    if (!(e instanceof ZodError)) throw e;
  }

  try {
    throw new APIError(
      "generic",
      z.parse(ProblemDetailsError, JSON.parse(resp.data as string))
    );
  } catch (e) {
    if (!(e instanceof ZodError)) throw e;
  }

  throw new APIError("generic", {
    status: resp.status,
    type: "Generic API Error",
    title: resp.statusText,
    detail:
      typeof resp.data == "object"
        ? JSON.stringify(resp.data)
        : resp.data?.toString(),
  });
}

export async function handleAPIResponse<T>(
  req: () => Promise<AxiosResponse<T>>
): Promise<T> {
  try {
    return (await req()).data;
  } catch (e) {
    if (e instanceof AxiosError) handleAPIErrorThrowing(e);
    else throw e;
  }

  throw "unreachable";
}

export async function parseAPIResponse<T extends z.ZodType>(
  schema: T,
  req: () => Promise<AxiosResponse<z.infer<T>>>
): Promise<z.infer<T>> {
  try {
    const data = (await req()).data;

    return z.parse(schema, data);
  } catch (e) {
    if (e instanceof AxiosError) handleAPIErrorThrowing(e);
    else throw e;
  }

  throw "unreachable";
}
