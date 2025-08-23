package domain

import ( 
		"time"
		"github.com/google/uuid"
		"gorm.io/gorm"
)

type Comment struct {
	ID		   uuid.UUID  `json:"id" gorm:"primaryKey"`
	BlogID     uuid.UUID  `json:"blogId" gorm:"not null"`
	AuthorID   int64     `json:"authorId" gorm:"not null"`
	Content    string    `json:"content" gorm:"not null"`
	CreatedAt  time.Time `json:"createdAt"`
	UpdatedAt  time.Time `json:"updatedAt"`
}

func (c *Comment) BeforeCreate(tx *gorm.DB) error {
	c.ID = uuid.New()
	now := time.Now()
	c.CreatedAt = now
	c.UpdatedAt = now
	return nil
}