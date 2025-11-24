import z from "zod";

const isoDatetimeToDate = z.codec(z.string(), z.date(), {
  decode: (isoString) => new Date(isoString),
  encode: (date) => date.toISOString(),
});
 

export const UserData = z.object({
  id: z.guid(),
  username: z.string(),
  createdAt: isoDatetimeToDate,
  email: z.email().optional(),
  lastLogin: isoDatetimeToDate.optional(),
  updatedAt: isoDatetimeToDate.optional(),
});

export type UserData = z.infer<typeof UserData>;

