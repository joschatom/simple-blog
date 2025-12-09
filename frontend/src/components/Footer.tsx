import { Link } from "react-router";
import "../styles/components/Footer.css"


export function Footer() {
  

  return (
    <footer>
      <ul>
        <li>Links</li>
        <li>
          <a href="https://github.com/joschatom/simple-blog">Github</a>
        </li>
        <li>
          <Link to="/about">About</Link>
        </li>
      </ul>
      <ul>
        <li>About</li>
        <li>Version: 0.1-test</li>
        <li>Branch: main</li>
      </ul>
    </footer>
  );
}
