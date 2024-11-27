import { Hono } from "@hono/hono";
import routes from "./routes/v1/register_routes.ts";
import * as Logger_Middleware_for_Hono_ from "@hono/hono/logger";
import { Logger } from "tslog";
const app = new Hono();

const logger = new Logger();
// app.use(Logger_Middleware_for_Hono_.logger(logger.debug));
app.route("/api", routes);

Deno.serve({ port: 8000 }, app.fetch);
