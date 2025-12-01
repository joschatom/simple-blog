import { Route, Routes } from "react-router";
import { Homepage } from "./pages/Homepage";
import { UserPage } from "./pages/User";
import { User, WebAPIClient } from "blog-api";
import { Suspense, useEffect, useMemo, useState } from "react";
import { Client } from "./client";
import { LoginPage } from "./pages/Login";
import { CreatePostPage } from "./pages/CreatePost";
import { PostsPage } from "./pages/Posts";

function App() {
  const [apiToken, setAPIToken] = useState<string | null>(localStorage.getItem("token"));
  const client = useMemo(() => new WebAPIClient(
    "http://localhost:5233/api",
    apiToken ? apiToken : undefined,
    undefined,
    (v) => {
      console.log(v);
      if (v !== undefined) {
        localStorage.setItem("token", v);
        setAPIToken(v);
      } else {
        localStorage.removeItem("token");
        setAPIToken(null);
        }
      }
    ), [apiToken]);

  return (
    <>
      <Client.Provider value={client}>
        <Routes>
          <Route path="/" element={<Homepage />} />
          <Route path="/users/:id" element={<UserPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/posts" element={
              <PostsPage />
            } />
          <Route path="/create-post" element={<CreatePostPage />} />
        </Routes>
      </Client.Provider>
    </>
  );
}

export default App;
