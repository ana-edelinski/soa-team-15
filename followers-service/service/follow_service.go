package service

import "followers-service/repository"

type FollowService struct{ repo *repository.FollowRepo }

func NewFollowService() *FollowService { return &FollowService{repo: repository.NewFollowRepo()} }

func (s *FollowService) Follow(u, t string) error                   { return s.repo.Follow(u, t) }
func (s *FollowService) Unfollow(u, t string) error                 { return s.repo.Unfollow(u, t) }
func (s *FollowService) Followers(t string) ([]string, error)       { return s.repo.Followers(t) }
func (s *FollowService) Following(u string) ([]string, error)       { return s.repo.Following(u) }
func (s *FollowService) IsFollowing(u, t string) (bool, error)      { return s.repo.IsFollowing(u, t) }
func (s *FollowService) Recommendations(u string) ([]string, error) { return s.repo.Recommendations(u) }
