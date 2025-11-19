import { APIError, User } from "blog-api";
import { useContext, useEffect, useState } from "react";
import { NavLink, useParams } from "react-router";
import { Client } from "../client";

export function UserPage() {
  const { id } = useParams();
  const client = useContext(Client);
  const [user, setUser] = useState<User>();
  const [error, setError] = useState<APIError>();

  useEffect(() => {
    const load = async () => {
      try {
        if (!id) {
          alert("id not given");
          return;
        }
        if (id === "me") setUser(await User.currentUser(client));
        else if (id?.startsWith(":"))
          setUser(await User.getByID(client, id.slice(1)));
        else setUser(await User.getByName(client, id));

        await client.login("admin", "abc12345");

      } catch (e) {
        if (e instanceof APIError) setError(e);
      }
    };

    load();

  }, [id, client]);

  return (
    <div style={{ fontSize: 24 }}>
      {error && <h1>{error.inner.title}</h1>}
      {user && <div>
          Username: {user.data.username}<br/>
          ID: {user.data.id}<br/>

          <NavLink to="/users/me">Me</NavLink>
        </div>}
    </div>
  );
}
