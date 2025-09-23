package main

import (
	"log"
	"os"

	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cors"

	"followers-service/handler"
)

func env(k, d string) string {
	if v := os.Getenv(k); v != "" {
		return v
	}
	return d
}

func main() {
	app := fiber.New()

	// CORS za Angular dev
	app.Use(cors.New(cors.Config{
		AllowOrigins: env("CORS_ORIGIN", "http://localhost:4200"),
		AllowMethods: "GET,POST,DELETE,OPTIONS",
		AllowHeaders: "Origin, Content-Type, Accept, Authorization",
	}))

	// Montiraj follow rute na /api/follow
	api := app.Group("/api/follow")
	fh := handler.NewFollowHandler()
	fh.Register(api)

	port := env("PORT", "8085")
	log.Printf("followers-service listening on :%s", port)
	if err := app.Listen(":" + port); err != nil {
		log.Fatal(err)
	}
}
