import lockClosed from "../assets/lock-closed.svg";
import React, { useEffect, useRef, type ComponentProps } from "react";

export function EmbedByUriUnsafe({
  uri,
  ...props
}: { uri: string } & ComponentProps<"svg">) {
  const ref = useRef<HTMLElement>(null);

  useEffect(() => {
    const svg = ref.current?.firstElementChild as SVGElement;
    console.log(svg);
    Object.entries(props).forEach(([key, val]) => {
      if (val != null || val != undefined)
        svg.setAttribute(key, val.toString());
    });
  }, [props]);

  const html = decodeURI(uri)
    .replace("data:image/svg+xml,", "")
    .replace(/<\?xml .*<svg/, "<svg");
  console.log(html);
  if (html.startsWith("<svg"))
    return <span ref={ref} dangerouslySetInnerHTML={{ __html: html }} />;
  else return <img src={uri} />;
}

