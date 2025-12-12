import {
  useContext,
  useEffect,
  useRef,
  useState,
  useTransition,
  type ComponentRef,
} from "react";
import { Client } from "../contexts";
import { APIError } from "blog-api";
import { NavLink, useFetcher, useLocation, useNavigate } from "react-router";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";

import "../styles/pages/Auth.css";
import { Footer } from "../components/Footer";

export function LoginPage() {
  const client = useContext(Client);
  const [error, setError] = useState<APIError>();
  const navigate = useNavigate();
  const [isPending, startTransition] = useTransition();

  const login = (data: FormData) =>
    startTransition(async () => {
      try {
        await client.login(
          data.get("username")!.toString(),
          data.get("password")!.toString()
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

        <form action={login} className="auth-form">
          <div>
            Login to continue sharing your own posts. <br />
            Don't have a account yet? <NavLink to="/register">Sign Up</NavLink>
          </div>

          <div>
            <label htmlFor="username">Username </label>
            <input id="username" name="username" required />
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
          <button type="submit" id="login-button">
            Login
          </button>
        </form>
      </main>

      <Footer />
    </>
  );
}
