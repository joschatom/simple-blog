import {
  createContext,
  Dispatch,
  SetStateAction,
  useContext,
  useEffect,
  useReducer,
  useRef,
  useState,
  type Ref,
  type RefObject,
} from "react";
import { Client } from "../client";
import { User } from "blog-api";
import { NavLink, useNavigate } from "react-router";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";
import { useContextMenu } from "../helpers/useContextMenu";
import { UsernameDisplay } from "../components/Username";

export function Homepage() {
  const client = useContext(Client);

  const ctx1 = useContextMenu({
    "Test 123": () => alert(1),
    "Item 2": () => alert("ITEM 2"),
  });

  const ctx2 = useContextMenu({
    "Test ABC": () => alert(2),
    "Item D": () => alert("ITEM D!"),
  });

  const ctxg = useContextMenu({
    "Test G1": () => alert(3),
    "Item GLOBAL": () => alert("ITEM G!"),
  });

  const [error, setError] = useState<unknown>();
  const [users, setUsers] = useState<User[]>();

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

      <main ref={ctxg}>
        {error !== undefined && <ErrorDisplay error={error} />}

        <h1 ref={ctx1 as Ref<HTMLHeadingElement>}>Click me 1</h1>
        <h1 ref={ctx2 as Ref<HTMLHeadingElement>}>Or me 2</h1>

        {users &&
          users.map((u) => (
            <UsernameDisplay user={u}/>
          ))}
      </main>
    </>
  );
}
