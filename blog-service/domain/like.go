package domain

import (
	"time"
	"github.com/google/uuid"
	"gorm.io/gorm"
)

type Like struct {
	ID        uuid.UUID `json:"id" gorm:"primaryKey"`
	BlogID    uuid.UUID `json:"blogId" gorm:"not null"`
	AuthorID  int64     `json:"authorId" gorm:"not null"`
	CreatedAt time.Time `json:"createdAt"`
}

func (l *Like) BeforeCreate(tx *gorm.DB) error {
	l.ID = uuid.New()
	l.CreatedAt = time.Now()
	return nil
}
