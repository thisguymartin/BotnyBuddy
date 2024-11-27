import { Hono } from "@hono/hono";
import { Logger } from "tslog";
const logger = new Logger({ name: "User" });

const user = new Hono();

user.get("/", (c) => {
  logger.info("List Users", { context: c.body });
  return c.text("List Users");
}); // GET /user

export default user;
