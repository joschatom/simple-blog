import { APIError, User } from "blog-api";
import { useContext, useEffect, useState, useTransition } from "react";
import { useNavigate, useParams } from "react-router";
import { Client } from "../contexts";
import { ErrorDisplay } from "../components/Error";
import { Header } from "../components/Header";
import dayjs from "dayjs";
import { Post } from "blog-api/src/post";
import { PostContainer } from "../components/Post";

import "../styles/pages/Profile.css";
import { NotLoggedIn, PageNotFound } from "./NotFound";
import { Footer } from "../components/Footer";

import profileImage from "../assets/user.png";

export function UserPage() {
  const { id } = useParams();
  const client = useContext(Client);
  const [user, setUser] = useState<User>();
  const [error, setError] = useState<APIError>();
  const [posts, setPosts] = useState<Post[]>();
  const [isPending, startTransition] = useTransition();

  useEffect(() => {
    const load = async () => {
      try {
        if (!id) {
          alert("id not given");
          return;
        }
        if (id === "me") setUser(await User.fetchCurrentUser(client));
        else if (id?.startsWith(":"))
          setUser(await User.fetchByID(client, id.slice(1)));
        else setUser(await User.fetchByName(client, id));
      } catch (e) {
        if (e instanceof APIError) setError(e);
        console.log(e);
      }
    };

    load();
  }, [id, client]);

  useEffect(
    () =>
      user !== undefined
        ? startTransition(async () => {
            setPosts(await user.posts());
          })
        : undefined,
    [user]
  );

  console.log(user?.data);

  const navigate = useNavigate();

  if (id === "me" && client.currentUser == undefined)
    return <NotLoggedIn/>

  if (error?.inner.title == "Not Found")
    return <PageNotFound />

  
  

  return (
    <>
      <Header />

      <main>
        {error && <ErrorDisplay error={error} marker={id} />}

        {user && (
          <>
            <div className="profile-header">
              <img className="profile-image" src={profileImage} />
              <p className="profile-name">{user.data.username}'s Profile</p>
              <p className="profile-joined-at">
                Joined {dayjs(user.data.createdAt).format("MMMM YYYY")}
                <span className="tooltip">{user.data.createdAt?.toLocaleString(undefined, { dateStyle: "full", timeStyle: "full" })}</span>
              </p>
              <p className="profile-post-count">
                {posts?.length == undefined ? <i>Loading</i> : posts.length}{" "}
                Posts
              </p>
            </div>
            <div>
              {user.data.id === client.currentUser?.data.id && (
                <button onClick={() => navigate("/create-post")}>Create Post</button>
              )}
            </div>
            <div
              className={`profile-posts-container ${
                isPending ? "profile-posts-loading" : ""
              }`}
            >
              {posts && posts.map((p) => <PostContainer post={p} />)}
            </div>
          </>
        )}
      </main>

      <Footer />
    </>
  );
}
