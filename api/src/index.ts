import { WebAPIClient, type APIClient } from "./client";
import { APIError } from "./error";
import { User } from "./user";

export type PostData = {};
export type UserData = {
  id: string;
  username: string;
  createdAt: string;
};
export type UserID = string;
export type PostID = string;





export async function main() {
  const client: APIClient = new WebAPIClient("http://localhost:5233");

  console.info("Looking for user with name, admin: ");
  console.log(
    await User.getByName(client, "invalid").catch((e) => {
      if (e instanceof APIError) {
        let err;
        if ((err = e.downcast("generic"))) console.log("generic", err);
        else if ((err = e.downcast("validation")))
          console.log("validation", err);
        else if ((err = e.downcast("axios"))) console.log("axios", err);
      }
    })
  );
}

main();
