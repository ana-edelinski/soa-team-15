package service

import (
	"blog-service/domain"
	"blog-service/repository"
	"context"
	"errors"
	"fmt"

	"github.com/google/uuid"
)

type CommentService struct {
	Repo *repository.CommentRepository
}

func NewCommentService(r *repository.CommentRepository) *CommentService {
	return &CommentService{Repo: r}
}

// Create a comment on a specific blog
func (s *CommentService) Create(ctx context.Context, blogId string, comment *domain.Comment) error {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Create(ctx, bid, comment)
}

func (s *CommentService) GetByBlogID(ctx context.Context, blogId string) ([]domain.Comment, error) {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return nil, fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.GetByBlogID(ctx, bid)
}

func (s *CommentService) GetByID(ctx context.Context, commentId string) (*domain.Comment, error) {
	cid, err := uuid.Parse(commentId)
	if err != nil {
		return nil, fmt.Errorf("invalid comment id: %w", err)
	}
	return s.Repo.GetByID(ctx, cid)
}

// Update only comment content (to match embedded-array update)
func (s *CommentService) Update(ctx context.Context, commentId string, newContent string) error {
	if newContent == "" {
		return errors.New("new content must not be empty")
	}
	cid, err := uuid.Parse(commentId)
	if err != nil {
		return fmt.Errorf("invalid comment id: %w", err)
	}
	return s.Repo.Update(ctx, cid, newContent)
}

func (s *CommentService) Delete(ctx context.Context, commentId string) error {
	cid, err := uuid.Parse(commentId)
	if err != nil {
		return fmt.Errorf("invalid comment id: %w", err)
	}
	return s.Repo.Delete(ctx, cid)
}
