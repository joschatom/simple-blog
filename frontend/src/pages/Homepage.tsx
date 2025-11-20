import { useContext } from "react";
import { Client } from "../client";
import { User } from "blog-api";
import { useNavigate } from "react-router";
import { Button } from "../components/Button";

export function Homepage() {
  const client = useContext(Client);

  const navigate = useNavigate();

  return (
    <>
      <form
        action={async (data) => {
          await client.login(
            data.get("username")!.toString(),
            data.get("password")!.toString()
          );
          alert(JSON.stringify(await User.currentUser(client), null, 2));
          navigate("/users/me");
        }}
      >
        <label htmlFor="username">Username: </label>
        <input id="username" name="username" />
        <br />
        <label htmlFor="password">Password: </label>
        <input id="password" name="password" type="password" />
        <br />
        <button type="submit">Login</button>
      </form>
    </>
  );
}
