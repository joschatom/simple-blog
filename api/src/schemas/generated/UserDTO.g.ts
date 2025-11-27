/* 9817040DD90401B7D06F0EA9AF760B6A5F2CB7E9 */
// Auto generated using C# source generator.
// DO NOT MODIFY

import z from "zod";

// See UserDTO.
// @generated
export const UserDTO = z.object({
	id: z.guid(),
	username: z.string(),
	email: z.string(),
	createdAt: z.date(),
	updatedAt: z.date(),

});

export type UserDTO = z.infer<typeof UserDTO>;