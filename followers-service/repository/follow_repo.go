package repository

import (
	"context"

	"followers-service/config"
	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

type FollowRepo struct{ drv neo4j.DriverWithContext }

func NewFollowRepo() *FollowRepo { return &FollowRepo{drv: config.Neo4j()} }

func (r *FollowRepo) Follow(u, t string) error {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeWrite})
	defer session.Close(ctx)

	_, err := neo4j.ExecuteWrite(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		_, err := tx.Run(ctx, `
			MERGE (u:User {id:$u})
			MERGE (t:User {id:$t})
			MERGE (u)-[f:FOLLOWS]->(t)
			ON CREATE SET f.since = datetime()`,
			map[string]any{"u": u, "t": t})
		return true, err
	})
	return err
}

func (r *FollowRepo) Unfollow(u, t string) error {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeWrite})
	defer session.Close(ctx)

	_, err := neo4j.ExecuteWrite(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		_, err := tx.Run(ctx,
			`MATCH (u:User {id:$u})-[f:FOLLOWS]->(t:User {id:$t}) DELETE f`,
			map[string]any{"u": u, "t": t})
		return true, err
	})
	return err
}

func (r *FollowRepo) Followers(t string) ([]string, error) {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeRead})
	defer session.Close(ctx)

	recs, err := neo4j.ExecuteRead(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		res, err := tx.Run(ctx,
			`MATCH (u:User)-[:FOLLOWS]->(t:User {id:$t}) RETURN u.id AS id`,
			map[string]any{"t": t})
		if err != nil { return nil, err }
		var out []string
		for res.Next(ctx) { out = append(out, res.Record().Values[0].(string)) }
		return out, res.Err()
	})
	if err != nil { return nil, err }
	return recs.([]string), nil
}

func (r *FollowRepo) Following(u string) ([]string, error) {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeRead})
	defer session.Close(ctx)

	recs, err := neo4j.ExecuteRead(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		res, err := tx.Run(ctx,
			`MATCH (u:User {id:$u})-[:FOLLOWS]->(t:User) RETURN t.id AS id`,
			map[string]any{"u": u})
		if err != nil { return nil, err }
		var out []string
		for res.Next(ctx) { out = append(out, res.Record().Values[0].(string)) }
		return out, res.Err()
	})
	if err != nil { return nil, err }
	return recs.([]string), nil
}

func (r *FollowRepo) IsFollowing(u, t string) (bool, error) {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeRead})
	defer session.Close(ctx)

	val, err := neo4j.ExecuteRead(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		rx, err := tx.Run(ctx,
			`MATCH (u:User{id:$u})-[:FOLLOWS]->(t:User{id:$t}) RETURN COUNT(*)>0 AS f`,
			map[string]any{"u": u, "t": t})
		if err != nil { return nil, err }
		if rx.Next(ctx) { return rx.Record().Values[0].(bool), rx.Err() }
		return false, rx.Err()
	})
	if err != nil { return false, err }
	return val.(bool), nil
}

func (r *FollowRepo) Recommendations(u string) ([]string, error) {
	ctx := context.Background()
	session := r.drv.NewSession(ctx, neo4j.SessionConfig{AccessMode: neo4j.AccessModeRead})
	defer session.Close(ctx)

	recs, err := neo4j.ExecuteRead(ctx, session, func(tx neo4j.ManagedTransaction) (any, error) {
		res, err := tx.Run(ctx, `
			MATCH (me:User {id:$u})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(cand:User)
			WHERE NOT (me)-[:FOLLOWS]->(cand) AND cand.id <> $u
			RETURN DISTINCT cand.id AS id LIMIT 20`, map[string]any{"u": u})
		if err != nil { return nil, err }
		var out []string
		for res.Next(ctx) { out = append(out, res.Record().Values[0].(string)) }
		return out, res.Err()
	})
	if err != nil { return nil, err }
	return recs.([]string), nil
}
