import { Route, Routes } from "react-router";
import { Homepage } from "./pages/Homepage";
import { UserPage } from "./pages/User";
import { WebAPIClient } from "blog-api";
import { useMemo, useState } from "react";
import { Client } from "./contexts";
import { LoginPage } from "./pages/Login";
import { CreatePostPage } from "./pages/CreatePost";
import { PostsPage } from "./pages/Posts";
import { NotLoggedIn, PageNotFound } from "./pages/NotFound";
import { RegisterPage } from "./pages/Register";
import { MutedUsersPage } from "./pages/MutedUsers";
import { ContextMenuProvider } from "./components/ContextMenuProvider";
import { AboutPage } from "./pages/About";
import { PostPage } from "./pages/Post";
import { SettingsPage } from "./pages/Settings";
import { NotificationProvider } from "./components/NotificationProvider";

function App() {
  const [apiToken, setAPIToken] = useState<string | null>(
    localStorage.getItem("token")
  );
  const client = useMemo(
    () =>
      new WebAPIClient(
        "http://localhost:5233/api",
        apiToken ? apiToken : undefined,
        (v) => {
          if (v !== undefined) {
            localStorage.setItem("token", v);
            setAPIToken(v);
          } else {
            localStorage.removeItem("token");
            setAPIToken(null);
          }
        }
      ),
    [apiToken]
  );

  return (
    <>
      <ContextMenuProvider>
        <NotificationProvider>
          <Client.Provider value={client}>
            <Routes>
              <Route path="/" element={<Homepage />} />
              <Route path="/users/:id" element={<UserPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/posts/:id" element={<PostPage />} />
              <Route path="/posts" element={<PostsPage />} />
              <Route path="/create-post" element={<CreatePostPage />} />
              <Route path="/muted-users" element={<MutedUsersPage />} />
              <Route path="/about" element={<AboutPage />} />
              <Route path="/settings" element={<SettingsPage />} />
              <Route
                path="/errors/not-logged-in"
                element={<NotLoggedIn isPage />}
              />
              <Route path="*" element={<PageNotFound />} />
            </Routes>
          </Client.Provider>
        </NotificationProvider>
      </ContextMenuProvider>
    </>
  );
}

export default App;
