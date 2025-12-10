import {
  type ComponentRef,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { Footer } from "../components/Footer";
import { Header } from "../components/Header";

import "../styles/pages/Settings.css";
import { Client } from "../contexts";
import { NotLoggedIn } from "./NotFound";
import { APIError } from "blog-api";
import { useNavigate } from "react-router";

export function SettingsPage() {
  const client = useContext(Client);

  const navigate = useNavigate();

  const [username, setUsername] = useState(client.currentUser?.data.username);
  const [email, setEmail] = useState(client.currentUser?.data.email);

  const reload = useCallback(async () => {
    try {
      await client.refreshToken();
      setUsername(client.currentUser?.data.username);
      setEmail(client.currentUser?.data.email);
    } catch (e) {
      let err;
      if (!(err = APIError.asDowncast(e, "generic"))) throw e;

      if (err.status == 403) navigate("/errors/not-logged-in?source=/settings");
      throw e;
    }
  }, [client, navigate]);

  useEffect(() => {
    const rload = async () => await reload();

    rload();
  }, [reload]);

  const save = async () => {
    client.currentUser?.update({
      email: email,
      username: username,
    });

    reload();
  };

  const changePassword = async () => {
    if (passwordInput.current == undefined)
      throw "passwordInput Element not referenced.";
    if (client.isAuthenticated())
      await client.changePassword(passwordInput.current?.value);
    else console.warn("tried to change password while loggen out.");
  };

  const deleteAccount = async () => {
    if (!client.currentUser) return;
    if (
      prompt(
        `Are you sure you want to IRREVERSIBLY DELETE the account named ${client.currentUser?.data.username} ?\nIf yes then type, 'delete ${client.currentUser.data.username}' below.`
      ) !== `delete ${client.currentUser.data.username}`
    )
      return;

    if (client.isAuthenticated()) {
      await client.currentUser?.delete();
      await client.logout(false);
      navigate("/");
    } else console.warn("tried to delete account while logged out.");
  };

  const deleteAllPosts = async () => {
    if (
      prompt(
        `Are you sure you want to IRREVERSIBLY DELETE all posts on this account?\nIf yes then type, 'delete all posts' below.`
      ) !== `delete all posts`
    )
      return;
    if (client.isAuthenticated()) {
      await client.deleteAllPosts();
      navigate("/users/me");
    } else console.warn("tried to delete all posts while logged out.");
  };

  const passwordInput = useRef<ComponentRef<"input">>(null);

  if (!client.isAuthenticated()) return <NotLoggedIn />;
  return (
    <>
      <Header />

      <main className="settings-container">
        <h2 className="page-title">Settings</h2>

        <div className="update-user">
          <span>
            <label htmlFor="username">Username</label>
            <input
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder={username ? "Enter a username" : "Loading..."}
              disabled={username == undefined}
            />
          </span>
          <span>
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              placeholder={email ? "Enter an email address" : "Loading..."}
              disabled={email == undefined}
            />
          </span>
          <span>
            <label htmlFor="theme">Theme</label>
            <select id="theme" disabled>
              <option selected>Light (Default)</option>
            </select>
          </span>

          <span>
            <span>
              <i>Set your username, email address and theme</i>
            </span>
            <button id="save" disabled={!username || !email} onClick={save}>
              Save
            </button>
          </span>

          <hr/>

          <span className="password-reset-container">
            {" "}
            <label htmlFor="newpassword">New Password</label>
            <span className="password-reset">
              <input
                ref={passwordInput}
                id="newpassword"
                type="password"
                placeholder="Enter a new password."
              />
              <button onClick={changePassword}>Update</button>
            </span>
          </span>
        </div>

        <div className="actions"></div>
        <hr />

        <div className="actions">
          <button id="delete-account" onClick={deleteAccount}>
            Delete Account
          </button>
          <button id="delete-all-posts" onClick={deleteAllPosts}>
            Delete all Posts
          </button>
        </div>
      </main>

      <Footer />
    </>
  );
}
