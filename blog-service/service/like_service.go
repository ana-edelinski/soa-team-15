package service

import (
	"blog-service/domain"
	"blog-service/repository"
)

type LikeService struct {
	Repo *repository.LikeRepository
}

func NewLikeService(repo *repository.LikeRepository) *LikeService {
	return &LikeService{Repo: repo}
}

func (s *LikeService) LikePost(like *domain.Like) error {
	return s.Repo.Create(like)
}

func (s *LikeService) UnlikePost(blogId string, authorId int64) error {
	return s.Repo.Delete(blogId, authorId)
}

func (s *LikeService) HasUserLiked(blogId string, authorId int64) (bool, error) {
	return s.Repo.Exists(blogId, authorId)
}

func (s *LikeService) CountLikes(blogId string) (int64, error) {
	return s.Repo.CountForBlog(blogId)
}
