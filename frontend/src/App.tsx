import { Route, Routes } from "react-router";
import { Homepage } from "./pages/Homepage";
import { UserPage } from "./pages/User";
import "./App.css";
import { WebAPIClient } from "blog-api";
import { useState } from "react";
import { Client } from "./client";

function App() {
  const [apiToken, setAPIToken] = useState<string | null>(
    localStorage.getItem("token")
  );

  const client = new WebAPIClient(
    "http://localhost:5233",
    apiToken ? apiToken : undefined,
    (v) => {
      console.log(v);
      localStorage.setItem("token", v);
      setAPIToken(v);
    }
  );

  return (
    <>
      <Client.Provider value={client}>
        <Routes>
          <Route path="/" element={<Homepage />} />
          <Route path="/users/:id" element={<UserPage />} />
        </Routes>
      </Client.Provider>
    </>
  );
}

export default App;
