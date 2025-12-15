import { Footer } from "../components/Footer";
import { Header } from "../components/Header";

import "../styles/pages/About.css";

export function AboutPage() {
  return (
    <>
      <Header />

      <main id="about-main">
        <h2>About Simple Blog</h2>
        A simple blog that allows the user to sign into an
        account a view posts by other users and make posts themselves.
        <h3>Development</h3>
        This is a fullstack application that uses `ASP.NET` for the backend and
        `React.JS` (via `vite.js`) for the frontend.
        <span><br></br>See <a href="https://github.com/joschatom/simple-blog">GitHub</a> for more information.</span>
        <strong>
          Note: This is NOT a real application, it is a project at Autismuslink.
        </strong>
      </main>

      <Footer />
    </>
  );
}
