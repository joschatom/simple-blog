import { useContext } from "react";
import { Client } from "../client";
import { User } from "blog-api";
import { useNavigate } from "react-router";
<<<<<<< Updated upstream
=======
import {
  Button,
  Container,
  createTheme,
  CssBaseline,
  FormLabel,
  Input,
  OutlinedInput,
  ThemeProvider,
} from "@mui/material";

const darkTheme = createTheme({
  palette: {
    mode: "dark"
  },
});
>>>>>>> Stashed changes

export function Homepage() {
  const client = useContext(Client);

  const navigate = useNavigate();

  return (
    <>
      <ThemeProvider theme={darkTheme}>
        <CssBaseline />
        <Container maxWidth={"lg"}>
          <form
            noValidate
            autoComplete="off"
            action={async (data) => {
              console.log(data);

              await client.login(
                data.get("username")!.toString(),
                data.get("password")!.toString()
              );
              alert(JSON.stringify(await User.currentUser(client), null, 2));
              navigate("/users/me");
            }}
          >
            <div className="form-field">
              <FormLabel htmlFor="username">Username</FormLabel>
              <Input id="username" name="username" />
            </div>

            <div>
              <FormLabel htmlFor="password">Password </FormLabel>
              <Input id="password" name="password" type="password" />
            </div>

            <br/>

            <div>
              <Button variant="contained" type="submit">
              Login
            </Button>
            
            </div>
          </form>
        </Container>
      </ThemeProvider>
    </>
  );
}
