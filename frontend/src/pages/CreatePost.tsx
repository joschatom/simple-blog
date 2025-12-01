import { useContext, useState } from "react";
import { Client } from "../client";
import lockOpen from "../assets/lock-open.svg";

import "../styles/components/PostContainer.css";
import { CreatePost, PostData } from "blog-api/src/schemas/post";
import { Post } from "blog-api/src/post";
import { ErrorDisplay } from "../components/Error";

export function CreatePostPage() {
  const client = useContext(Client);
  const [caption, setCaption] = useState<string>("");
  const [content, setContent] = useState<string>("");
  const [locked, setLocked] = useState<boolean>(false);
  const [error, setError] = useState<unknown>();


  const createPost = async () => {
    const post: CreatePost = {
      caption: caption!,
      content: content!,
      registeredUsersOnly: locked,
    };

    try {
      await Post.createPost(client, post);
    } catch (e) {
      setError(e);
    }
  };

  return (
    <>
      <ErrorDisplay error={error} />

      
        <fieldset className="post-container">
          <legend>Create Post</legend>
          <div className="post-container-header">
            <input
              max={255}
              placeholder="Caption your Post..."
              className="post-caption"
              onChange={(e) => setCaption(e.target.value)}
              value={caption}
              min={1}
            />

            <img src={lockOpen} className="post-locked-icon" />
          </div>
          <textarea
            maxLength={100000}
            name="content"
            className="post-container-content"
            onChange={(e) => setContent(e.target.value)}
            value={content}
          />
          <div className="post-container-footer">
            <button className="create-post-submit" onClick={async () => await createPost()}>Post</button>
          </div>
        </fieldset>
    </>
  );
}
