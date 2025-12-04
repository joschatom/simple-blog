import { useLocation, useNavigate } from "react-router";

export function useRefresh(): () => Promise<void> {
  const location = useLocation();
  const navigate = useNavigate();

  return async () =>
    await navigate(
      location.pathname +
        (location.search !== undefined ? location.search : ""),
      {
        replace: true,
        preventScrollReset: true,
      }
    );
}
