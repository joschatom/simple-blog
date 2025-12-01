import type { Post } from "blog-api/src/post";

import lockOpen from "../assets/lock-open.svg"

export function PostContainer({ post }: { post: Post }) {
    return (
        <div className="post-container">
            <div className="post-container-header">
            <div className="post-caption">{post.data.caption}</div>

            <img src={lockOpen} className="post-locked-icon" />
          </div>
          <div className="post-container-content">{post.data.content}</div>
          <div className="post-container-footer">
            <div>{post.data.userId}</div>
          </div>
        </div>
    
    )
}
