package repository

import (
	"blog-service/domain"
	"gorm.io/gorm"
)

type LikeRepository struct {
	DB *gorm.DB
}

func NewLikeRepository(db *gorm.DB) *LikeRepository {
	return &LikeRepository{DB: db}
}

func (r *LikeRepository) Create(like *domain.Like) error {
	return r.DB.Create(like).Error
}

func (r *LikeRepository) Delete(blogId string, authorId int64) error {
	return r.DB.Where("blog_id = ? AND author_id = ?", blogId, authorId).Delete(&domain.Like{}).Error
}

func (r *LikeRepository) Exists(blogId string, authorId int64) (bool, error) {
	var count int64
	err := r.DB.Model(&domain.Like{}).Where("blog_id = ? AND author_id = ?", blogId, authorId).Count(&count).Error
	return count > 0, err
}

func (r *LikeRepository) CountForBlog(blogId string) (int64, error) {
	var count int64
	err := r.DB.Model(&domain.Like{}).Where("blog_id = ?", blogId).Count(&count).Error
	return count, err
}
