package startup

import (
	"log"

	"github.com/gofiber/fiber/v2"
	"followers-service/config"
	"followers-service/handler"
)

func Run() {
	config.ConnectNeo4j()
	defer config.CloseNeo4j()

	app := fiber.New()
	h := handler.NewFollowHandler()
	h.Register(app)

	log.Println("Followers service on :8085")
	log.Fatal(app.Listen(":8085"))
}
