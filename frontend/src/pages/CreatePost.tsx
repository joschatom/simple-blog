
import "../styles/components/PostContainer.css";
import "../styles/pages/CreatePost.css"

import { PostContainer } from "../components/Post";
import { Footer } from "../components/Footer";
import { Header } from "../components/Header";
import { useUserNotify } from "../helpers/useUserNotify";



export function CreatePostPage() {

  const notify = useUserNotify();

  notify({
    type: "error",
    text: "Test",
    detail: "This is more detail"
  });

  return <>
    <Header/>

    <main>
    <PostContainer  post={undefined}/>
    </main>

    <Footer/>
  </>
}
