package repository

import (
	"blog-service/domain"
	"gorm.io/gorm"
)

type CommentRepository struct {
	DB *gorm.DB
}

func NewCommentRepository(db *gorm.DB) *CommentRepository {
	return &CommentRepository{DB: db}
}

func (r *CommentRepository) Create(comment *domain.Comment) error {
	return r.DB.Create(comment).Error
}

func (r *CommentRepository) GetByBlogID(blogId string) ([]domain.Comment, error) {
	var comments []domain.Comment
	err := r.DB.Where("blog_id = ?", blogId).Find(&comments).Error
	return comments, err
}

func (r *CommentRepository) Update(comment *domain.Comment) error {
	return r.DB.Save(comment).Error
}

func (r *CommentRepository) GetByID(commentId string) (*domain.Comment, error) {
	var comment domain.Comment
	if err := r.DB.Where("id = ?", commentId).First(&comment).Error; err != nil {
		return nil, err
	}
	return &comment, nil
}

func (r *CommentRepository) Delete(commentId string) error {
	return r.DB.Where("id = ?", commentId).Delete(&domain.Comment{}).Error
}

