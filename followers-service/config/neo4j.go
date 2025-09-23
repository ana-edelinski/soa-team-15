package config

import (
	"log"
	"os"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

var drv neo4j.DriverWithContext

func Neo4j() neo4j.DriverWithContext {
	if drv != nil {
		return drv
	}
	uri := os.Getenv("NEO4J_URI")
	user := os.Getenv("NEO4J_USER")
	pass := os.Getenv("NEO4J_PASSWORD")

	d, err := neo4j.NewDriverWithContext(uri, neo4j.BasicAuth(user, pass, ""))
	if err != nil {
		log.Fatalf("Neo4j connection failed: %v", err)
	}
	drv = d
	return drv
}
