import { OpenAPIHono } from "@hono/zod-openapi";
import { route } from "./presentation/api/routes.ts";

const app = new OpenAPIHono();

// Add the route
app.openapi(route, (c) => {
  console.log("Request received");
  return c.json({
    id: 1,
    age: 20,
    name: "Ultra-man",
  });
});

// Start the server
Deno.serve(app.fetch);
