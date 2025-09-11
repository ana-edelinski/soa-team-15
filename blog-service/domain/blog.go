package domain

import (
	"github.com/google/uuid"
	"time"

)

type Blog struct {
	ID        uuid.UUID  `json:"id" bson:"_id,omitempty"`
	Title     string     `json:"title" bson:"title"`
	Content   string     `json:"content" bson:"content"`
	AuthorID  int64      `json:"authorId" bson:"authorId"`
	ImagePaths []string  `json:"imagePaths" bson:"imagePaths"`
	Comments  []Comment  `json:"comments" bson:"comments"`
	Likes     []Like     `json:"likes" bson:"likes"`
	CreatedAt time.Time  `json:"createdAt" bson:"createdAt"`
	UpdatedAt time.Time  `json:"updatedAt" bson:"updatedAt"`
}

