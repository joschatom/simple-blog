import { Post } from "blog-api/src/post";
import { useContext, useEffect, useState, useTransition } from "react";
import { useParams } from "react-router";
import { Client } from "../contexts";
import { EntityNotFound, NotLoggedIn, PageNotFound } from "./NotFound";
import { Header } from "../components/Header";
import { PostContainer } from "../components/Post";
import { APIError } from "blog-api";

export function PostPage() {
  const { id } = useParams();
  const client = useContext(Client);
  const [post, setPost] = useState<Post>();
  const [isLoading, startLoad] = useTransition();
  const [err, setErr] = useState<unknown>();

  useEffect(
    () =>
      startLoad(async () => {
        if (id === undefined) return;

        try {
          setPost(await Post.fetchById(client, id));
        } catch (e) {
          setErr(e);
        }
      }),
    [id, client]
  );

  if (id == undefined) return <PageNotFound />;

  if (err instanceof APIError) {
    if (err.downcast("generic")?.status === 400)
      return (
        <EntityNotFound keyval={id} keyname="ID" name="Post" listing="/posts" />
      );
    else if (err.downcast("generic")?.status == 403)
        return <NotLoggedIn/>
  }

  return (
    <>
      <Header />

      <main>
        {post && <PostContainer post={post} />}
        {!post && isLoading && <i>Loading...</i>}
      </main>
    </>
  );
}
