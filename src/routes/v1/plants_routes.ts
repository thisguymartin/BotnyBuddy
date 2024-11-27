import { Hono } from "@hono/hono";
import { Logger } from "tslog";
const logger = new Logger({ name: "Plant", type: "json" });

const plant = new Hono();

plant.all("/", (c) => {
  logger.info("List Plant", { context: c });
  return c.text("List Plants");
}); // GET /plant

export default plant;
