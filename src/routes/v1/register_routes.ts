import { Hono } from "@hono/hono";
import user from "./user_routes.ts";
import plant from "./plants_routes.ts";

const hono = new Hono();

hono.route("/user", user);
hono.route("/plant", plant);

export default hono;
