import { useCallback, useContext, useEffect, useState } from "react";
import { Footer } from "../components/Footer";
import { Header } from "../components/Header";

import "../styles/pages/Settings.css";
import { Client } from "../client";
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
        </div>

        <div className="actions"></div>

        <hr />

        <label htmlFor="newpassword">New Password</label>
        <span className="password-reset">
          <input
            id="newpassword"
            type="password"
            placeholder="Enter a new password."
          />
          <button>Update</button>
        </span>

        <hr />

        <div className="actions">
          <button id="delete-account">Delete Account</button>
          <button id="delete-all-posts">Delete all Posts</button>
        </div>
      </main>

      <Footer />
    </>
  );
}
