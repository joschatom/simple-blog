import z from "zod";



export const zs = {
    username: () => z.string()
        .regex(new RegExp(/^[a-zA-Z0-9-]+$/))
        .max(255),
    updated: (update: z.ZodObject) => z.object({
        updated: z.boolean(),
        updatedFields: z.array(z.keyof(update))
    })
}

