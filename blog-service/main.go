package main

import (
	"blog-service/config"
	"blog-service/startup"
	"log"

	"github.com/joho/godotenv"
)

func main() {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, relying on system environment variables")
	}

	config := config.NewConfig()
	server := startup.NewServer(config)
	server.Start()
}
