import {
  useContext,
  useEffect,
  useRef,
  useState,
  useTransition,
  type ComponentRef,
} from "react";
import { Client } from "../contexts";
import { APIError } from "blog-api";
import { NavLink, useFetcher, useLocation, useNavigate } from "react-router";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";

import "../styles/pages/Auth.css";
import { Footer } from "../components/Footer";
import { Close, Password, PasswordOutlined } from "@mui/icons-material";

import {
  unstable_PasswordToggleField as PasswordToggleField,
  Form
} from "radix-ui";
import { EyeClosedIcon, EyeOpenIcon } from "@radix-ui/react-icons";
import { useUserNotify } from "../helpers/useUserNotify";

export function LoginPage() {
  const client = useContext(Client);
  const [error, setError] = useState<APIError>();
  const navigate = useNavigate();
  const [isPending, startTransition] = useTransition();

  const login = (data: FormData) =>
    startTransition(async () => {
      try {
        await client.login(
          data.get("username")!.toString(),
          data.get("password")!.toString()
        );
        await navigate("/users/me");
      } catch (e) {
        if (e instanceof APIError) setError(e);
        else
          setError(
            new APIError("generic", {
              type: "Unknown Error",
              title: "An unknown error occoured",
              detail: JSON.stringify(e),
            })
          );
      }
    });

  const errorDiag = useRef<ComponentRef<"dialog">>(null);

  useEffect(() => {
    if (error != undefined) errorDiag.current?.showModal();
  }, [error]);

  //const [open, setOpen] = useState(true);
  const notify = useUserNotify();

  useEffect(() => {
    console.log("notify");
    notify({
      type: "general",
      text: `Hello there: ${crypto.randomUUID()}`,
      detail: "SOme more text....",
    });
  }, []);

  return (
    <>
      <Header />

      <main>
        <dialog className="error" ref={errorDiag}>
          <ErrorDisplay error={error} />
          <button
            onClick={() => {
              setError(undefined);
              errorDiag.current?.close();
            }}
          >
            Okay
          </button>
        </dialog>

        <Form.Root className="from-root">
          <Form.Field className="from-field" name="username">
            <div>
              <Form.Label className="from-label">Username</Form.Label>
              <Form.Message className="form-message" />
            </div>
            <Form.Control asChild>
              <input className="form-input" type="text" required max={255} />
            </Form.Control>
          </Form.Field>
          <Form.Submit asChild>
            <button className="submit" style={{ marginTop: 10 }}>
              Login
            </button>
          </Form.Submit>
        </Form.Root>
      </main>

      <Footer />
    </>
  );
}
