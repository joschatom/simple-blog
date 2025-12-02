import { NavLink, useLocation } from "react-router";
import { Header } from "../components/Header";

import "../styles/pages/NotFound.css";
import type { ReactNode } from "react";

function ErrorPageInternal({
  title,
  message,
  children,
}: {
  title: ReactNode | string;
  message: ReactNode | string;
  children?: ReactNode;
}) {
  return (
    <>
      <Header />

      <main>
        <h1 children={title}></h1>
        <p children={message}></p>
        <div className="error-details" children={children}></div>
      </main>
    </>
  );
}

export function PageNotFound() {
  const location = useLocation();

  return (
    <ErrorPageInternal
      title="Page not found."
      message={
        <>
          The page at <code>{location.pathname}</code> doesn't exist.
        </>
      }
    >
      <NavLink to="/">Go back home</NavLink>
    </ErrorPageInternal>
  );
}


export function NotLoggedIn() {
  const location = useLocation();

  return (
    <ErrorPageInternal
      title="Login Required"
      message={
        <>
          The page at <code>{location.pathname}</code> is only accessable when logged in.
        </>
      }
    >
      <NavLink to="/login">Login</NavLink><br/>
      <NavLink to="/">Go back home</NavLink>
    </ErrorPageInternal>
  );
}