
import "../styles/components/PostContainer.css";

import { PostContainer } from "../components/Post";

export function CreatePostPage() {


/*  return (
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
  );*/

  return <>
    <h1>
      Create Post
    </h1>

    <PostContainer post={undefined}/>
  </>
}
