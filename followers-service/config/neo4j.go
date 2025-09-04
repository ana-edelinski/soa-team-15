package config

import (
	"context"
	"log"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

var driver neo4j.DriverWithContext

func Neo4j() neo4j.DriverWithContext { return driver }

func ConnectNeo4j() {
	uri  := GetEnv("NEO4J_URI", "bolt://localhost:7687")
	user := GetEnv("NEO4J_USER", "neo4j")
	pass := GetEnv("NEO4J_PASSWORD", "neo4jpass")

	d, err := neo4j.NewDriverWithContext(uri, neo4j.BasicAuth(user, pass, ""))
	if err != nil { log.Fatal("neo4j driver:", err) }
	driver = d
}

func CloseNeo4j() { driver.Close(context.Background()) }
