import { useContext, useState } from "react";
import { Client } from "../client";
import { APIError } from "blog-api";
import { NavLink } from "react-router";
import { ErrorDisplay } from "../components/Error";

export function Homepage() {
  const client = useContext(Client);
  const [error, setError] = useState<APIError>();

  const login = async (data: FormData) => {
    try {
      await client.login(
        data.get("username")!.toString(),
        data.get("password")!.toString()
      );
    } catch (e) {
      if (e instanceof APIError) setError(e);
    }
  };

  return (
    <>
      {error && <ErrorDisplay error={error} />}

      {!client.isAuthenticated() ? (
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
          <br />
          <button type="submit">Login</button>
        </form>
      ) : (
        <>
          <h1>
            Logged in as <NavLink to="/users/me">User</NavLink>
          </h1>{" "}
          <button
            onClick={async () => {
              client.logout();
            }}
          >
            Logout
          </button>
        </>
      )}
    </>
  );
}
