import {
  useContext,
  useState,
  useTransition,
} from "react";
import { Client } from "../contexts";
import { APIError } from "blog-api";
import { NavLink, useNavigate } from "react-router";
import { Header } from "../components/Header";

import "../styles/pages/Auth.css";
import { Footer } from "../components/Footer";
import { useNotifyReset, useUserNotify } from "../helpers/useUserNotify";
import { ZodError } from "zod";
import Button from "../components/Button";

export function LoginPage() {
  const navigate = useNavigate();
  const notify = useUserNotify();
  const reset = useNotifyReset();

  const client = useContext(Client);
  const [isPending, startTransition] = useTransition();

  const [username, setUsername] = useState<string>();
  const [password, setPassword] = useState<string>();

  const login = () =>
    startTransition(async () => {
      reset();

      try {
        await client.login(
          username || "",
          password || "",
        );
        await navigate("/users/me");
      } catch (e) {
        let err;
        if ((err = APIError.asDowncast(e, "generic"))) {
          notify({
            type: "error",
            text: err?.title,
            detail: err?.detail,
          });
        }
        if ((err = APIError.asDowncast(e, "validation"))) {
          console.log(err);

          Object.entries(err.errors).forEach(([key, errors]) => {
            notify({
              type: "error",
              text: (
                <>
                  <a href={`#${key.toLowerCase()}`}>{key.toLowerCase()}</a>{" "}
                  {errors[0].split(" ").slice(3).join(" ")}
                </>
              ),
            });
          });
        } else if (e instanceof ZodError) {
          notify({
            type: "error",
            text: e.name,
            detail: e.message,
          });
        }
      }
    });

  return (
    <>
      <Header />

      <main>
        <form action={login} className="auth-form" noValidate>
          <div>
            Login to continue sharing your own posts. <br />
            Don't have a account yet? <NavLink to="/register">Sign Up</NavLink>
          </div>

          <div>
            <label htmlFor="username">Username </label>
            <input id="username" name="username" value={username} onChange={e => setUsername(e.target.value)} />
          </div>
          <div>
            <label htmlFor="password">Password </label>
            <input
              id="password"
              name="password"
              type="password" 
              value={password} onChange={e => setPassword(e.target.value)}
            />
          </div>
          <hr className={isPending ? "loading" : ""} />
          <Button level="neutral" id="login-button" disabled={!username || !password}>Login</Button>
        </form>
      </main>

      <Footer />
    </>
  );
}
