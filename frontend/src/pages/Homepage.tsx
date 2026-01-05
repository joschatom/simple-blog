import { useContext, useEffect, useState } from "react";
import { Client } from "../contexts";
import { User } from "blog-api";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";
import { UsernameDisplay } from "../components/Username";
import { Footer } from "../components/Footer";

import "../styles/pages/Homepage.css";
import { useUserNotify } from "../helpers/useUserNotify";


export function Homepage() {
  const client = useContext(Client);

  const [error, setError] = useState<unknown>();
  const [users, setUsers] = useState<User[]>();

  const notify = useUserNotify();

  notify({
    type: "info",
    text: "INFO"
  })

  useEffect(() => {
    const load = async () => {
      try {
        setUsers(await User.fetchAll(client));
      } catch (e) {
        setError(e);
      }
    };

    load();
  }, [client]);

  return (
    <>
      <Header />

      <main>
        {error !== undefined && <ErrorDisplay error={error} />}
        {users && users.map((u) => <UsernameDisplay user={u} />)}
      </main>

      <Footer />
    </>
  );
}
