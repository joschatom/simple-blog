import { useContext, useEffect, useRef, useState, useTransition, type ComponentRef } from "react";
import { Client } from "../client";
import { APIError } from "blog-api";
import { NavLink, useNavigate } from "react-router";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";

import "../styles/pages/Auth.css";

export function RegisterPage() {
  const client = useContext(Client);
  const [error, setError] = useState<APIError>();
  const navigate = useNavigate();
  const [isPending, startTransition] = useTransition();

  const register = (data: FormData) =>
    startTransition(async () => {
      try {
        await client.register(
          data.get("username")!.toString(),
          data.get("password")!.toString(),
          data.get("email")!.toString()
        );
        await navigate("/users/me");
      } catch (e) {
        if (e instanceof APIError) setError(e);
        else
          setError(
            new APIError("generic", {
              type: "Unknown Error",
              title: "An unknown error occoured",
              detail: JSON.stringify(e),
            })
          );
      }
    });
  const errorDiag = useRef<ComponentRef<"dialog">>(null);

  useEffect(() => {
    if (error != undefined) errorDiag.current?.showModal();
  }, [error]);

  return (
    <>
      <Header />

      <main>
        <dialog className="error" ref={errorDiag}>
          <ErrorDisplay error={error} />
          <button
            onClick={() => {
              setError(undefined);
              errorDiag.current?.close();
            }}
          >
            Okay
          </button>
        </dialog>
        <form action={register} className="auth-form">
          <div>
            Register to share your own posts with others,
            <br />
            already have an account? <NavLink to="/login">Login</NavLink>
          </div>
          <div>
            <label htmlFor="username">Username </label>
            <input id="username" max={255} name="username" required />
          </div>
          <div>
            <label htmlFor="email">Email </label>
            <input id="email" max={255} name="email" type="email" required />
          </div>
          <div>
            <label htmlFor="password">Password </label>
            <input
              id="password"
              name="password"
              type="password"
              required
              minLength={6}
            />
          </div>
          <hr className={isPending ? "loading" : ""} />
          <button type="submit" id="register-button">
            Register
          </button>
        </form>
      </main>
    </>
  );
}
