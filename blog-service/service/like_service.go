package service

import (
	"blog-service/repository"
	"context"
	"fmt"

	"github.com/google/uuid"
)

type LikeService struct {
	Repo *repository.LikeRepository
}

func NewLikeService(repo *repository.LikeRepository) *LikeService {
	return &LikeService{Repo: repo}
}

func (s *LikeService) LikePost(ctx context.Context, blogId string, authorId int64) error {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Create(ctx, bid, authorId)
}

func (s *LikeService) UnlikePost(ctx context.Context, blogId string, authorId int64) error {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Delete(ctx, bid, authorId)
}

func (s *LikeService) HasUserLiked(ctx context.Context, blogId string, authorId int64) (bool, error) {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return false, fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.Exists(ctx, bid, authorId)
}

func (s *LikeService) CountLikes(ctx context.Context, blogId string) (int64, error) {
	bid, err := uuid.Parse(blogId)
	if err != nil {
		return 0, fmt.Errorf("invalid blog id: %w", err)
	}
	return s.Repo.CountForBlog(ctx, bid)
}
