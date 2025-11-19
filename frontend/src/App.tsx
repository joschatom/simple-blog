
import { Route, Routes } from "react-router";
import { Homepage } from "./pages/Homepage";
import { UserPage } from "./pages/User";
import "./App.css";


function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<Homepage/>}/>
        <Route path="/users/:id" element={<UserPage/>}/>
      </Routes>
    </>
  );
}

export default App;
