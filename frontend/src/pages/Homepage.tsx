import { useContext, useEffect, useState } from "react";
import { Client } from "../client";
import { User } from "blog-api";
import { NavLink } from "react-router";
import { ErrorDisplay } from "../components/Error";

export function Homepage() {
  const client = useContext(Client);

  const [error, setError] = useState<unknown>();
  const [users, setUsers] = useState<User[]>();

  useEffect(() => {
    const load = async () => {
      try {
       setUsers(await User.fetchAll(client));
      } catch (e) {
        setError(e)
      }
    };

    load();
  }, [client]);

  return (
    <>
      {error && <ErrorDisplay error={error}/>}

      {users &&
        users.map((u) => (
          <>
            <NavLink to={`/users/${u.data.username}`}>
              {u.data.username}
            </NavLink>
            <br />
          </>
        ))}
    </>
  );
}
