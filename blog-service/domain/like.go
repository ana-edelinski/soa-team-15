package domain

import (
	"time"
	"github.com/google/uuid"
)


type Like struct {
	ID        uuid.UUID `json:"id" bson:"id"`
	AuthorID  int64     `json:"authorId" bson:"authorId"`
	CreatedAt time.Time `json:"createdAt" bson:"createdAt"`
}
