import z from "zod";
import { isoDatetimeToDate } from "./codecs.ts";
import { UserData } from "./user.ts";


export const PostData = z.object({
    id: z.guid(),
    caption: z.string(),
    content: z.string(),
    updatedAt: isoDatetimeToDate.nullable(),
    createdAt: isoDatetimeToDate,
    userId: z.guid(),
    registredUsersOnly: z.boolean(),
    user: UserData.optional()
});

export type PostData = z.infer<typeof PostData>;

export const CreatePost = z.object({
    caption: z.string().min(1).max(255),
    content: z.string().min(1).max(10000),
    registeredUsersOnly: z.boolean().default(false)
});

export type CreatePost = z.infer<typeof CreatePost>;

export const UpdatePost = z.object({
    caption: z.string().min(1).max(255).optional(),
    content: z.string().min(1).max(10000).optional(),
    registeredUsersOnly: z.boolean().default(false).optional()
});

export type  UpdatePost = z.infer<typeof UpdatePost>;

