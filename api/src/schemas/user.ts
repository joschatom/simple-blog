import z from "zod";

export const UserData = z.object({
  id: z.guid(),
  username: z.string(),
  createdAt: z.date(),
  email: z.email().optional(),
  lastLogin: z.date().optional(),
  updatedAt: z.date().optional(),
});

export type UserData = z.infer<typeof UserData>;
