package domain

import (
	"github.com/google/uuid"
	"gorm.io/gorm"
	"time"
	"encoding/json"

)

type Blog struct {
	Id        uuid.UUID `json:"id" gorm:"primaryKey"`
	Title     string    `json:"title" gorm:"not null"`
	Content   string    `json:"content" gorm:"not null"`
	AuthorID  int64     `json:"authorId" gorm:"not null"`
	Images    string    `json:"images" gorm:"type:text"`
	CreatedAt time.Time `json:"createdAt" gorm:"not null"`
	UpdatedAt time.Time `json:"updatedAt" gorm:"not null"`
	ImagePaths []string `json:"imagePaths" gorm:"-"`
}

func (blog *Blog) BeforeCreate(tx *gorm.DB) error {

    blog.Id = uuid.New()

    now := time.Now()
    blog.CreatedAt = now
    blog.UpdatedAt = now

   if blog.ImagePaths != nil {
		jsonData, err := json.Marshal(blog.ImagePaths)
		if err != nil {
			return err
		}
		blog.Images = string(jsonData)
	}
	return nil
}

func (blog *Blog) AfterFind(tx *gorm.DB) error {
	if blog.Images != "" {
		var paths []string
		if err := json.Unmarshal([]byte(blog.Images), &paths); err != nil {
			return err
		}
		blog.ImagePaths = paths
	}
	return nil
}