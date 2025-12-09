import { NavLink, useLocation } from "react-router";
import { Header } from "../components/Header";

import "../styles/pages/NotFound.css";
import type { ReactNode } from "react";
import { Footer } from "../components/Footer";

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

      <Footer />
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

export function EntityNotFound({
  name,
  keyval,
  keyname,
  listing,
}: {
  name: string;
  keyval: string;
  keyname: string;
  listing?: string
}) {
  return (
    <ErrorPageInternal
      title={<>{name} not found.</>}
      message={
        <>
          <>
            The {name} with {keyname} of <code>{keyval}</code> not found.
          </>
        </>
      }
    >
      {listing && <NavLink to={listing}>See all {name}s</NavLink>}
      <br/>
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
          The page at <code>{location.pathname}</code> is only accessable when
          logged in.
        </>
      }
    >
      <NavLink to="/login">Login</NavLink>
      <br />
      <NavLink to="/">Go back home</NavLink>
    </ErrorPageInternal>
  );
}
