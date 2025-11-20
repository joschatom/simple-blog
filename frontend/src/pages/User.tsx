import { APIError, User } from "blog-api";
import { useContext, useEffect, useState } from "react";
import { NavLink, useParams } from "react-router";
import { Client } from "../client";
import { ErrorDisplay } from "../components/Error";

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
      } catch (e) {
        if (e instanceof APIError) setError(e);
      }
    };

    load();
  }, [id, client]);

  console.log(user?.data);

  return (
    <div style={{ fontSize: 24 }}>
      {error && <ErrorDisplay error={error} marker={id} />}

      {user && (
        <div>
          Username: {user.data.username}
          <br />
          Joined: {user.data.createdAt.toLocaleString()}
          <br />
          {user.data.email && (
            <>
              Email: <code>{user.data.email}</code>
              <br />
            </>
          )}
          ID: <code>{user.data.id}</code>
          <br />
          <NavLink to="/users/me">Me</NavLink>
        </div>
      )}

      {client.isAuthenticated() && (
        <button
          onClick={async () => {
            setUser(undefined);
            client.logout();
          }}
        >
          Logout
        </button>
      )}
    </div>
  );
}
