package domain

import (
	"github.com/google/uuid"
	"gorm.io/gorm"
	"time"
)

type Blog struct {
	Id        uuid.UUID `json:"id" gorm:"primaryKey"`
	Title     string    `json:"title" gorm:"not null"`
	Content   string    `json:"content" gorm:"not null"`
	AuthorID  int64     `json:"authorId" gorm:"not null"` 
	CreatedAt time.Time `json:"createdAt" gorm:"not null"`
	UpdatedAt time.Time `json:"updatedAt" gorm:"not null"`
}

func (blog *Blog) BeforeCreate(scope *gorm.DB) error {
	blog.Id = uuid.New()
	blog.CreatedAt = time.Now()
	blog.UpdatedAt = time.Now()
	return nil
}

func (blog *Blog) BeforeUpdate(scope *gorm.DB) error {
	blog.UpdatedAt = time.Now()
	return nil
}
