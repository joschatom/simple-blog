import { useContext, useState, useTransition } from "react";
import { Client } from "../client";
import { APIError } from "blog-api";
import { useNavigate } from "react-router";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";

export function LoginPage() {
  const client = useContext(Client);
  const [error, setError] = useState<APIError>();
  const navigate = useNavigate();
  const [isPending, startTransition] = useTransition();

  const login = (data: FormData) =>
    startTransition(async() => {
      try {
        await client.login(
          data.get("username")!.toString(),
          data.get("password")!.toString()
        );
        await navigate("/users/me");
      } catch (e) {
        if (e instanceof APIError) setError(e);
        else setError(new APIError("generic", {
          type: "Unknown Error",
          title: "An unknown error occoured",
          detail: JSON.stringify(e)
        }))
      }
    });

  return (
    <>
      <Header />

      {error && <ErrorDisplay error={error} />}

      <form action={login}>
        <label htmlFor="username">Username: </label>
        <input id="username" name="username" required />
        <br />
        <label htmlFor="password">Password: </label>
        <input
          id="password"
          name="password"
          type="password"
          required
          minLength={6}
        />
        <hr className={isPending ? "loading" : ""}/>
        <button type="submit">Login</button>
      </form>
    </>
  );
}
