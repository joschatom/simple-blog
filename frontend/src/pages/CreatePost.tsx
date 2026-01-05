
import "../styles/components/PostContainer.css";
import "../styles/pages/CreatePost.css"

import { PostContainer } from "../components/Post";
import { Footer } from "../components/Footer";
import { Header } from "../components/Header";

export function CreatePostPage() {
  return <>
    <Header/>

    <main>
    <PostContainer  post={undefined}/>
    </main>

    <Footer/>
  </>
}
