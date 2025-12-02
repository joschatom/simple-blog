import { jwtDecode } from "jwt-decode"


export type TokenData = {
    expires: Date,
    username: string,
    userId: string
}

export function decodeToken(token: string) {
  const raw = jwtDecode(token);

  return {
    expires: new Date(Date.now() + raw.exp),
    username: raw["unique_name"],
    userId: JSON.parse(raw["http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata"])
  }
}