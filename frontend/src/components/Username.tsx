import { useContext, type Ref } from "react";
import { NavLink, useNavigate } from "react-router";
import { Client } from "../contexts";
import type { User } from "blog-api";
import { useContextMenu } from "../helpers/useContextMenu";
import type { UserData } from "blog-api/src/schemas/user";

export function UsernameDisplay({
  user,
  noMute,
  userData,
}: {
  user?: User;
  noMute?: boolean;
  userData?: UserData;
}) {
  const navigate = useNavigate();
  const client = useContext(Client);

  const data = userData ? userData : user!.data;

  const targetRef = useContextMenu({
    "View Profile": () => navigate(`/users/${data?.username}`),
    "Copy ID": () => navigator.clipboard.writeText(data!.id),
    "Mute User": {
      handler: () => client.muting?.mute(data!.id),
      disabled:
        !client.isAuthenticated() ||
        client.muting == undefined ||
        (noMute ? noMute : false),
    },
  });

  return (
    <NavLink
      ref={targetRef as Ref<HTMLAnchorElement>}
      to={`/users/${data.username}`}
    >
      {data.username}
      {client.currentUser?.data.id === data.id && "(You)"}
    </NavLink>
  );
}
