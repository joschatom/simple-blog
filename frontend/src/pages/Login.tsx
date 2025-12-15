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
import { useUserNotify } from "../helpers/useUserNotify";
import { ZodAny, ZodError } from "zod";

export function LoginPage() {
  const navigate = useNavigate();
  const notify = useUserNotify();

  const client = useContext(Client);
  const [error, setError] = useState<APIError>();
  const [isPending, startTransition] = useTransition();

  const [username, setUsername] = useState<string>();
  const [password, setPassword] = useState<string>();

  const login = (data: FormData) =>
    startTransition(async () => {
      try {
        await client.login(
          data.get("username")!.toString(),
          data.get("password")!.toString()
        );
        await navigate("/users/me");
      } catch (e) {
        let err;
        if ((err = APIError.asDowncast(e, "generic")))
          notify({
            type: "error",
            text: err.title,
            detail: err.detail,
          });
        else if (e instanceof ZodError)
          notify({
            type: "error",
            text: e.name,
          })
      }
    });

  return (
    <>
      <Header />

      <main>
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
          <button id="login-button">Login</button>
        </form>
      </main>

      <Footer />
    </>
  );
}
