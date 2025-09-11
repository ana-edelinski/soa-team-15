package service

import (
	"blog-service/domain"
	"blog-service/repository"
	"context"
	"fmt"

	"github.com/google/uuid"
)

type BlogService struct {
	Repo *repository.BlogRepository
}

func NewBlogService(r *repository.BlogRepository) *BlogService {
	return &BlogService{Repo: r}
}

func (s *BlogService) Create(ctx context.Context, blog *domain.Blog) (*domain.Blog, error) {
	return s.Repo.Create(ctx, blog)
}

func (s *BlogService) GetAll(ctx context.Context, page, limit int) (*[]domain.Blog, error) {
	return s.Repo.GetAll(ctx, page, limit)
}

func (s *BlogService) GetById(ctx context.Context, id string) (*domain.Blog, error) {
	uid, err := uuid.Parse(id)
	if err != nil {
		return nil, fmt.Errorf("invalid blog id: %w", err)
	}
	blog, err := s.Repo.GetById(ctx, uid)
	if err != nil {
		return nil, fmt.Errorf("blog with id %s not found: %w", id, err)
	}
	return blog, nil
}

func (s *BlogService) Update(ctx context.Context, id string, blog *domain.Blog) (*domain.Blog, error) {
	uid, err := uuid.Parse(id)
	if err != nil {
		return nil, fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Update(ctx, uid, blog)
}

func (s *BlogService) Delete(ctx context.Context, id string) error {
	uid, err := uuid.Parse(id)
	if err != nil {
		return fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Delete(ctx, uid)
}
