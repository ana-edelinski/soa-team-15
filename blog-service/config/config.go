package config

import "os"

type Config struct {
	Port     string
	MongoURI string
	MongoDB  string
	UploadDir string
}

func getenv(k, def string) string {
	if v := os.Getenv(k); v != "" {
		return v
	}
	return def
}

func NewConfig() *Config {
	return &Config{
		Port:      getenv("BLOG_SERVICE_PORT", "8080"),
		MongoURI:  getenv("MONGO_URI", "mongodb://blog-db:27017"),
		MongoDB:   getenv("MONGO_DB", "blogdb"),
		UploadDir: getenv("UPLOAD_DIR", "./uploads"),
	}
}
