import { useContext } from "react";
import { Client } from "../client";

import account from "../assets/account.png";

import "../styles/components/Header.css";
import { NavLink, useNavigate } from "react-router";

export function Header() {
  const client = useContext(Client);
  const navigate = useNavigate();

  return (
    <header className="blog-header">
      <div className="header-links-container">
        <img src="logo.png" alt="simple blog" onClick={() => navigate("/")} />

        <NavLink to="/posts" className={"header-posts"}>
          Posts
        </NavLink>
      </div>

      {!client.isAuthenticated() ? (
        <button>Login</button>
      ) : (
        <div className="profile-image-container">
          <img src={account} className="header-profile-image" />
          <div className="profile-navigation">
            <button>Profile</button>
            <button>Settings</button>
            <button>Log Out</button>
          </div>
        </div>
      )}
    </header>
  );
}
