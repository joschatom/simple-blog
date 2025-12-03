import z from "zod";
import { isoDatetimeToDate } from "./codecs.ts";
import { zs } from "./shared.ts";

export const UserData = z.object({
  id: z.guid(),
  username: z.string(),
  createdAt: isoDatetimeToDate.optional(),
  email: z.string().includes("@").optional(),
  lastLogin: isoDatetimeToDate.optional(),
  updatedAt: isoDatetimeToDate.optional().nullable(),
});

export type UserData = z.infer<typeof UserData>;

export const UpdateUserDTO = z.object({
  username: zs.username().optional(),
  email: z.email().max(255).optional()
})

export type UpdateUserDTO = z.infer<typeof UpdateUserDTO>;
export const updated = zs.updated(UpdateUserDTO);
export type UpdatedUserDTO = z.infer<typeof updated>