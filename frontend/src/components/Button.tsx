import { type ButtonHTMLAttributes } from "react";

import "../styles/components/Button.css"
type ButtonLevel =
  | "danger"
  | "milder-danger"
  | "warning"
  | "notice"
  | "success"
  | "neutral";

export const Button = ({ level, className, ...props }: { level?: ButtonLevel } & ButtonHTMLAttributes<HTMLButtonElement>) =>
  <button className={`leveled ${className}`} data-level={level} {...props}/>

export default Button;
