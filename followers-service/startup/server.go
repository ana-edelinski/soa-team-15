package startup

import (
	"log"
	"os"

	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cors"
	"followers-service/config"
	"followers-service/handler"
)

func Run() {
	config.ConnectNeo4j()
	defer config.CloseNeo4j()

	app := fiber.New()

	origin := os.Getenv("CORS_ORIGIN")
	if origin == "" {
		origin = "http://localhost:4200"
	}
	app.Use(cors.New(cors.Config{
		AllowOrigins:     origin,
		AllowHeaders:     "Content-Type, Authorization",
		AllowMethods:     "GET,POST,PUT,PATCH,DELETE,OPTIONS",
		AllowCredentials: true,
	}))



	h := handler.NewFollowHandler()
	h.Register(app)

	log.Println("Followers service on :8085")
	log.Fatal(app.Listen(":8085"))
}
