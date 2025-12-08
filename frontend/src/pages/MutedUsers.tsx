import { useContext, useEffect, useState, useTransition } from "react";
import { Header } from "../components/Header";
import { Client } from "../client";
import { NotLoggedIn } from "./NotFound";
import { User } from "blog-api";
import { UsernameDisplay } from "../components/Username";
import { Footer } from "../components/Footer";

function MutedUser({
  user,
  unmute,
}: {
  user: User;
  unmute: () => Promise<void>;
}) {
  return (
    <div>
      <UsernameDisplay user={user} noMute={true} />
      <button onClick={async () => await unmute()}>Unmute</button>
    </div>
  );
}

export function MutedUsersPage() {
  const client = useContext(Client);
  const [muted, setMuted] = useState<User[]>([]);
  const [isPending, startLoad] = useTransition();

  useEffect(
    () =>
      startLoad(async () => {
        if (client.isAuthenticated() && client.muting)
          setMuted(await client.muting.getMutedUsers());
      }),
    [client]
  );

  if (!client.isAuthenticated()) return <NotLoggedIn />;

  const unmute = async (id: string) => {
    await client.muting?.unmute(id);

    setMuted((arr) => arr.filter((u) => u.data.id !== id));
  };

  return (
    <>
      <Header />

      <main>
        <h1>Muted Users</h1>

        {isPending && <i>Loading</i>}
        {muted.length === 0 && !isPending && <strong>Nothing here yet.</strong>}
        {muted.map((mu) => (
          <MutedUser
            key={mu.data.id}
            user={mu}
            unmute={() => unmute(mu.data.id)}
          />
        ))}
      </main>

      <Footer />
    </>
  );
}
