import { useContext, useState, useTransition } from "react";
import { Client } from "../contexts";
import { APIError } from "blog-api";
import { NavLink, useNavigate } from "react-router";
import { Header } from "../components/Header";

import "../styles/pages/Auth.css";
import { Footer } from "../components/Footer";
import { useNotifyReset, useUserNotify } from "../helpers/useUserNotify";
import { ZodError } from "zod";
import Button from "../components/Button";

export function RegisterPage() {
  const client = useContext(Client);

  const navigate = useNavigate();
  const notify = useUserNotify();
  const reset = useNotifyReset();

  const [isPending, startTransition] = useTransition();

  const [username, setUsername] = useState<string>();
  const [email, setEmail] = useState<string>();
  const [password, setPassword] = useState<string>();

  const register = () =>
    startTransition(async () => {
      reset();

      try {
        await client.register(username || "", password || "", email || "");
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
        } else {
          notify({
            type: "error",
            text: "Unknown Error",
            detail: JSON.stringify(e, null, 2),
          });
        }
      }
    });

  return (
    <>
      <Header />

      <main>
        <form action={register} className="auth-form" noValidate>
          <div>
            Register to share your own posts with others,
            <br />
            already have an account? <NavLink to="/login">Login</NavLink>
          </div>
          <div>
            <label htmlFor="username">Username</label>
            <input
              id="username"
              name="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>
          <div>
            <label htmlFor="email">Email </label>
            <input
              id="email"
              name="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
          <div>
            <label htmlFor="password">Password</label>
            <input
              id="password"
              name="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          <hr className={isPending ? "loading" : ""} />
          <Button type="submit" id="register-button">
            Register
          </Button>
        </form>
      </main>

      <Footer />
    </>
  );
}
