package domain

import (
	"time"

	"github.com/google/uuid"
)

type Comment struct {
	ID        uuid.UUID `json:"id" bson:"id"`
	AuthorID  int64     `json:"authorId" bson:"authorId"`
	Content   string    `json:"content" bson:"content"`
	CreatedAt time.Time `json:"createdAt" bson:"createdAt"`
	UpdatedAt time.Time `json:"updatedAt" bson:"updatedAt"`
}

