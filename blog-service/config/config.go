package config

import "os"

type Config struct {
	Port     string
	DBHost   string
	DBPort   string
	DBName   string
	DBUser   string
	DBPass   string
	GrpcPort string
}

func NewConfig() *Config {
	return &Config{
		Port:     os.Getenv("BLOG_SERVICE_PORT"),
		DBHost:   os.Getenv("BLOG_DB_HOST"),
		DBPort:   os.Getenv("BLOG_DB_PORT"),
		DBName:   os.Getenv("BLOG_DB_NAME"),
		DBUser:   os.Getenv("BLOG_DB_USER"),
		DBPass:   os.Getenv("BLOG_DB_PASS"),
		GrpcPort: os.Getenv("BLOGS_GRPC_PORT"),
	}
}
