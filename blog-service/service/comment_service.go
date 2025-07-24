package service

import (
	"blog-service/domain"
	"blog-service/repository"
)

type CommentService struct {
	Repo *repository.CommentRepository
}

func NewCommentService(r *repository.CommentRepository) *CommentService {
	return &CommentService{Repo: r}
}

func (s *CommentService) Create(comment *domain.Comment) error {
	return s.Repo.Create(comment)
}

func (s *CommentService) GetByBlogID(blogId string) ([]domain.Comment, error) {
	return s.Repo.GetByBlogID(blogId)
}

func (s *CommentService) GetByID(commentId string) (*domain.Comment, error) {
	return s.Repo.GetByID(commentId)
}

func (s *CommentService) Update(comment *domain.Comment) error {
	return s.Repo.Update(comment)
}

func (s *CommentService) Delete(commentId string) error {
	return s.Repo.Delete(commentId)
}
