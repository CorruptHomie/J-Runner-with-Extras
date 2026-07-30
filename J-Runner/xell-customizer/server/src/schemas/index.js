import { z } from "zod";

// Ported unchanged from xell-customizer-api/src/schemas/index.ts - Zod doesn't care what
// JS runtime it's under.
export const generateSchema = z.object({
  background_color: z.string().optional(),
  foreground_color: z.string().optional(),
  ascii_art: z.string().optional(),
});
