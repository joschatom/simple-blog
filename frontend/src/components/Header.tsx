import { useContext } from "react";
import { Client } from "../client";

import account from "../assets/account.png";

import "../styles/components/Header.css";
import { useNavigate } from "react-router";

export function Header() {
  const client = useContext(Client);
  const navigate = useNavigate();

  return (
      <header className="blog-header">
        <img src="logo.png" alt="simple blog" />

        {!client.isAuthenticated() ? (
          <button>Login</button>
        ) : (
          <img src={account} className="profile-image" onClick={() => navigate("/users/me")}/>
        )}
      </header>
  );
}
