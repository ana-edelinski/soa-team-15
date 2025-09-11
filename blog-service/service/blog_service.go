package service

import (
	"blog-service/domain"
	"blog-service/repository"
	"fmt"
)

type BlogService struct {
	BlogRepository *repository.BlogRepository
}

func (service *BlogService) Create(blog *domain.Blog) (*domain.Blog, error) {
	return service.BlogRepository.Create(blog)
}

func (service *BlogService) GetAll(page, limit int) (*[]domain.Blog, error) {
	return service.BlogRepository.GetAll(page, limit)
}

func (service *BlogService) GetById(id string) (*domain.Blog, error) {
	blog, err := service.BlogRepository.GetById(id)
	if err != nil {
		return nil, fmt.Errorf("blog with id %s not found", id)
	}
	return blog, nil
}

func (service *BlogService) Update(id string, blog *domain.Blog) (*domain.Blog, error) {
	return service.BlogRepository.Update(id, blog)
}

func (service *BlogService) Delete(id string) error {
	return service.BlogRepository.Delete(id)
}