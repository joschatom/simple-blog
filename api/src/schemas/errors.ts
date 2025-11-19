import z from "zod";

export const ValidationErrors = z.object({
  type: z.url(),
  title: z.string(),
  status: z.number(),
  errors: z.record(z.string(), z.array(z.string())),
  traceId: z.guid(),
});

export type ValidationErrors = z.infer<typeof ValidationErrors>;

export const ProblemDetailsError = z.object({
  type: z.string(),
  title: z.string(),
  status: z.number(),
  detail: z.string(),
});

export type ProblemDetailsError = z.infer<typeof ProblemDetailsError>;