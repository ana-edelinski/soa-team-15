package main

import (
	"blog-service/config"
	"blog-service/startup"
)

func main() {
	config := config.NewConfig()
	server := startup.NewServer(config)
	server.Start()
}