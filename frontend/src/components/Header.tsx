import { useContext } from "react";
import { Client } from "../contexts";

import account from "../assets/user.png";
import Logout from "../assets/icons/logout.svg?react"
import Login from "../assets/icons/login.svg?react"
import Settings from "../assets/icons/settings.svg?react" 
import Person from "../assets/icons/person.svg?react";

import "../styles/components/Header.css";
import { NavLink, useNavigate } from "react-router";

export function Header() {
  const client = useContext(Client);
  const navigate = useNavigate();

  return (
    <>
      <header className="blog-header">
        <div className="header-links-container">
          <img src="logo.png" alt="simple blog" onClick={() => navigate("/")} title="Go back to homepage" loading="eager" />

          <NavLink to="/posts" className={"header-posts"}>
            Posts
          </NavLink>
        </div>

        {!client.isAuthenticated() ? (
          
            <Login className="login-button" title="Login" onClick={() => navigate("/login")}/>
        ) : (
          <div className="profile-image-container">
            <img src={account} className="header-profile-image" />
            <div className="profile-navigation">
              <button onClick={() => navigate("/users/me")}><Person/>Profile</button>
              <button onClick={() => navigate("/settings")}><Settings/>Settings</button>
              <button
                onClick={async () => {
                  await client.logout();
                  navigate("/");
                }}
              >
                <Logout/>Log Out
              </button>
            </div>
          </div>
        )}
      </header>
      <div className="header-placeholder" />
    </>
  );
}
