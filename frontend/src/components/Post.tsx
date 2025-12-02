import { Post } from "blog-api/src/post";

import lockOpen from "../assets/lock-open.svg";
import { NavLink, useNavigate } from "react-router";
import { type ComponentRef, useContext, useEffect, useRef, useState } from "react";
import { Client } from "../client";
import { ErrorDisplay } from "./Error";
import type { CreatePost } from "blog-api/src/schemas/post";

export function PostContainer({ post }: { post?: Post }) {
  const client = useContext(Client);
  let isOwner = client.currentUser
    ? client.currentUser.data.id === post?.data.userId
    : false;

  if (post === undefined) isOwner = true;

  const navigate = useNavigate();

  const [editMode, setEditMode] = useState(post === undefined);
  const [deleted, setDeleted] = useState(false);

  const [error, setError] = useState<unknown>();
  const errorDiag = useRef<ComponentRef<"dialog">>(null);

  const [caption, setCaption] = useState<string>(post?.data.caption || "");
  const [content, setContent] = useState<string>(post?.data.content || "");
  const [locked, setLocked] = useState<boolean>(
    post?.data.registredUsersOnly || false
  );

  const cancel = () => {
    if (post == undefined) {
      navigate("/users/me");
      return;
    }

    console.assert(editMode);

    setCaption(post!.data.caption);
    setContent(post!.data.content);
    setLocked(post!.data.registredUsersOnly);

    setEditMode(false);
  };

  const edit = () => {
    if (!post) return;

    console.assert(post.data.userId === client.currentUser?.data.id);

    setEditMode(true);
  };

  const delete_ = async () => {
    if (!post) return;

    console.assert(post.data.userId === client.currentUser?.data.id);

    if (
      prompt(
        "Are you sure you want to IRREVERSIBLY DELETE this post?\nIf yes then type, 'delete this post' below."
      ) !== "delete this post"
    )
      return;

    try {
      await post.delete();
      setDeleted(true);
    } catch (e) {
      setError(e);
    }
  };

  const createNew = async () => {
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

  useEffect(() => {
    if (error != undefined) errorDiag.current?.showModal();
  }, [error]);

  if (deleted && post !== undefined)
    return <div id="deleted-post" data-id={post.data.id} />;
  else
    return (
      <div
        className="post-container"
        style={{
          width: post && "90%",
        }}
      >
        <dialog className="error" ref={errorDiag}>
          <ErrorDisplay error={error} />
          <button
            onClick={() => {
              setError(undefined);
              errorDiag.current?.close();
            }}
          >
            Okay
          </button>
        </dialog>

        <div className="post-container-header">
          <input
            className="post-caption"
            value={caption}
            disabled={!editMode}
            placeholder="Caption your post..."
            min={1}
            max={255}
            onChange={(e) =>
              editMode
                ? setCaption(e.target.value)
                : console.error("tried to edit outside of edit mode.")
            }
          />

          <div>
            {post && (
              <div>
                <NavLink to={`/users/${post.data.user.username}`}>
                  {post.data.user.username + (isOwner ? " (You)" : "")}
                </NavLink>
              </div>
            )}
            <img src={lockOpen} className="post-locked-icon" />
          </div>
        </div>
        <textarea
          className="post-container-content"
          value={content}
          disabled={!editMode}
          onChange={(e) =>
            editMode
              ? setContent(e.target.value)
              : console.error("tried to edit outside of edit mode.")
          }
        />
        <div className="post-container-footer">
          {!isOwner ? (
            client.currentUser ? (
              <>
                <button className="mute-button">Mute</button>
                <button className="hide-post-button">Hide Post</button>
              </>
            ) : (
              <>
                <NavLink
                  to="/login"
                  aria-description="Redircts to login page inorder to access POST actions."
                >
                  <em>Post actions are only available for logged in users.</em>
                </NavLink>
              </>
            )
          ) : editMode ? (
            <>
              <button className="post-cancel-button" onClick={cancel}>
                Cancel
              </button>
              <button
                className="post-save-button"
                onClick={
                  post
                    ? () => setError("not yet implemented")
                    : createNew
                }
              >
                {!post ? "Create Post" : "Save"}
              </button>
            </>
          ) : (
            <>
              <button className="edit-post-button" onClick={edit}>
                Edit
              </button>
              <button className="delete-post-button" onClick={delete_}>
                Delete
              </button>
            </>
          )}
        </div>
      </div>
    );
}
