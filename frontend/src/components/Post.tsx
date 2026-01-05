import { Post } from "blog-api/src/post";

import { NavLink, useNavigate } from "react-router";
import {
  type ComponentRef,
  type ComponentProps,
  useContext,
  useEffect,
  useRef,
  useState,
  useCallback,
  type Ref,
  type FunctionComponent,
  createElement,
} from "react";
import { Client } from "../contexts";
import { ErrorDisplay } from "./Error";
import type { CreatePost } from "blog-api/src/schemas/post";
import moment from "moment";
import { UsernameDisplay } from "./Username";
import { useContextMenu } from "../helpers/useContextMenu";

import lockOpen from "../assets/icons/lockOpen.svg?react";
import LockClosed from "../assets/icons/lockClosed.svg?react";
import Edit from "../assets/icons/Edit.svg?react";
import MuteIcon from "../assets/icons/mute.svg?react";
import Close from "../assets/icons/close.svg?react";
import Delete from "../assets/icons/delete.svg?react";
import Done from "../assets/icons/done.svg?react";
import CreatePostIcon from "../assets/icons/createPost.svg?react";
import Button from "./Button";
import { APIError } from "blog-api";
import z, { ZodError } from "zod";
import { useUserNotify } from "../helpers/useUserNotify";
import Markdown from "react-markdown";

function DurationSince({
  date,
  ...props
}: { date: Date } & ComponentProps<"span">) {
  const [dur, setDur] = useState<moment.Duration>(
    moment.duration({
      from: date,
      to: Date.now(),
    })
  );

  const timeoutMin = (dur2: moment.Duration) => {
    if (dur2.asSeconds() < 60) return 1000;

    return 60 * 1000;
  };

  const tick = useCallback(() => {
    const dur = moment.duration({
      from: date,
      to: moment.utc(),
    });
    console.info("tick", timeoutMin(dur));
    setDur(dur);

    setTimeout(tick, timeoutMin(dur));
  }, [date]);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => tick(), []);

  const formatElem = (ident: string, val: number, cond: boolean) => {
    if (cond) return undefined;
    let res = "";
    if (val >= 1) res = `${val.toFixed(0)} ${ident}`;
    else return undefined;
    if (val >= 2) res += "s";

    return res;
  };

  const parts = [
    formatElem("month", dur.months(), false),
    formatElem("day", dur.days(), false),
    formatElem("hour", dur.hours(), dur.asMonths() >= 1),
    formatElem("minute", dur.minutes(), dur.asDays() >= 1),
    formatElem("second", dur.seconds(), dur.asMinutes() >= 10),
  ]
    .filter((v) => v != undefined)
    .join(", ");

  const last = parts.lastIndexOf(",");

  return (
    <span {...props}>
      Posted {parts.substring(0, last)}
      {last && " and"}
      {parts.substring(last + 1)} ago
    </span>
  );
}

export function Select<TrueProps extends object, FalseProps extends object>({
  cond,
  trueComponent,
  falseComponent,
  ...props
}: {
  cond: boolean;
  trueComponent: FunctionComponent<TrueProps>;
  falseComponent: FunctionComponent<FalseProps>;
} & (TrueProps | FalseProps)) {
  if (cond) return createElement(trueComponent, props as TrueProps);
  else return createElement(falseComponent, props as FalseProps);
}

export function PostContainer({ post }: { post?: Post }) {
  const client = useContext(Client);
  let isOwner = client.currentUser
    ? client.currentUser.data.id === post?.data.userId
    : false;

  if (post === undefined) isOwner = true;

  const navigate = useNavigate();
  const notify = useUserNotify();

  const [editMode, setEditMode] = useState(post === undefined);
  const [deleted, setDeleted] = useState(false);

  const [error, setError] = useState<unknown>();
  const errorDiag = useRef<ComponentRef<"dialog">>(null);

  const [caption, setCaption] = useState<string>(post?.data.caption || "");
  const [content, setContent] = useState<string>(post?.data.content || "");
  const [locked, setLocked] = useState<boolean>(
    post?.data.registredUsersOnly || false
  );

  const targetRef = useContextMenu({
    "Go to author": () =>
      navigate(
        `/users/${
          post!.data.user ? post!.data.user!.username : `:${post!.data.userId}`
        }`
      ),
    "Copy Link": () =>
      navigator.clipboard.writeText(`${location.host}/posts/${post!.data.id}`),
    "Copy ID": () => navigator.clipboard.writeText(post!.data.id),
  });

  const cancel = () => {
    if (post == undefined) {
      navigate(-1);
      return;
    }

    console.assert(editMode);

    setCaption(post!.data.caption);
    setContent(post!.data.content);
    setLocked(post!.data.registredUsersOnly);

    setEditMode(false);
  };

  const validate = () => {
    if (caption.length == 0 || !caption) {
      notify({
        type: "error",
        text: "A post must have a caption.",
      });
    }
    if (content.length == 0 || !content)
      notify({
        type: "error",
        text: "A post must have some content in it.",
      });
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
      notify({
        type: "success",
        text: "Post deleted successfully."
      })
    } catch (e) {
      let err;
      if ((err = APIError.asDowncast(e, "generic")))
        notify({
          type: "error",
          text: err.title,
          detail: err.detail,
        });
      else if (e instanceof ZodError)
        notify({
          type: "error",
          text: e.name,
        });
    }
  };

  const createNew = async () => {
    validate();

    const post: CreatePost = {
      caption: caption!,
      content: content!,
      registeredUsersOnly: locked,
    };

    try {
      await Post.createPost(client, post);
      navigate("/users/me");
    } catch (e) {
      let err;
      if ((err = APIError.asDowncast(e, "generic")))
        notify({
          type: "error",
          text: err.title,
          detail: err.detail,
        });
      else if (e instanceof ZodError && (err = JSON.parse(e.message)[0])) {
        console.log(err);
        notify({
          type: "error",
          text: "Invalid Input",
          detail: z.prettifyError(e),
        });
      }
    }

    if (post == undefined) {
      navigate(-1);
      return;
    }
  };

  const update = async () => {
    console.assert(post !== undefined);

    const updateReq: CreatePost = {
      caption: caption!,
      content: content!,
      registeredUsersOnly: locked,
    };

    try {
      await post?.update(updateReq);
    } catch (e) {
      setError(e);
    }

    setEditMode(false);
  };

  const mute = async () => {
    console.assert(client.currentUser !== undefined);
    console.assert(post !== undefined);

    if (!confirm(`Mute the user with the name ${post!.data.user!.username}?`))
      return;

    await client.muting?.mute(post!.data.userId);
    // TODO: Uncomment. ^
    location.reload();
  };

  useEffect(() => {
    if (error != undefined) errorDiag.current?.showModal();
  }, [error]);

  if (deleted && post !== undefined)
    return <div id="deleted-post" data-id={post.data.id} />;
  else
    return (
      <div
        ref={targetRef as Ref<HTMLDivElement>}
        className="post-container"
        id={`post-${post?.data.id || "new"}`}
      >
        <ErrorDisplay error={error} />

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
                <UsernameDisplay userData={post.data.user} />
              </div>
            )}
            <Select
              trueComponent={LockClosed}
              falseComponent={lockOpen}
              cond={locked}
              className="post-locked-icon"
              onClick={editMode ? () => setLocked((v) => !v) : () => {}}
            />
          </div>
        </div>

        <span
          className="post-container-content"
          data-no-scroll={
            post?.data.content.startsWith("<noscroll/>") ? "on" : undefined
          }
        >
          {editMode ? (
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
          ) : (
            <Markdown skipHtml={false}>
              {content}
            </Markdown>
          )}
        </span>

        <div className="post-container-footer">
          <div className="start-actions">
            {post && !editMode && <DurationSince date={post.data.createdAt} />}
          </div>
          <div className="end-actions">
            {!isOwner ? (
              client.currentUser ? (
                <Button className="mute-button" onClick={mute} level="neutral">
                  <MuteIcon />
                  Mute
                </Button>
              ) : (
                <NavLink to="/login">
                  <i>Post actions are only available for logged in users.</i>
                </NavLink>
              )
            ) : editMode ? (
              <>
                <Button level="success" onClick={post ? update : createNew}>
                  {!post ? (
                    <>
                      <CreatePostIcon />
                      Create Post
                    </>
                  ) : (
                    <>
                      <Done />
                      Save
                    </>
                  )}
                </Button>
                <Button level="milder-danger" onClick={cancel}>
                  <Close />
                  Cancel
                </Button>
              </>
            ) : (
              <>
                <Button level="danger" onClick={delete_}>
                  <Delete />
                  Delete
                </Button>
                <Button level="notice" onClick={edit}>
                  <Edit />
                  Edit
                </Button>
              </>
            )}
          </div>
        </div>
      </div>
    );
}
