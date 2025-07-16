package repository

import (
	"blog-service/domain"
	"gorm.io/gorm"
)

type BlogRepository struct {
	DatabaseConnection *gorm.DB
}

func (repo *BlogRepository) Create(blog *domain.Blog) (*domain.Blog, error) {
	dbResult := repo.DatabaseConnection.Create(blog)
	if dbResult.Error != nil {
		return nil, dbResult.Error
	}
	return blog, nil
}

func (repo *BlogRepository) GetAll(page, limit int) (*[]domain.Blog, error) {
	var blogs []domain.Blog
	offset := (page - 1) * limit
	result := repo.DatabaseConnection.Offset(offset).Limit(limit).Find(&blogs)
	if result.Error != nil {
		return nil, result.Error
	}
	return &blogs, nil
}

func (repo *BlogRepository) GetById(id string) (*domain.Blog, error) {
	var blog domain.Blog
	result := repo.DatabaseConnection.First(&blog, "id = ?", id)
	if result.Error != nil {
		return nil, result.Error
	}
	return &blog, nil
}

func (repo *BlogRepository) Update(id string, blog *domain.Blog) (*domain.Blog, error) {
	var existingBlog domain.Blog
	if err := repo.DatabaseConnection.First(&existingBlog, "id = ?", id).Error; err != nil {
		return nil, err
	}

	existingBlog.Title = blog.Title
	existingBlog.Content = blog.Content

	if err := repo.DatabaseConnection.Save(&existingBlog).Error; err != nil {
		return nil, err
	}

	return &existingBlog, nil
}

func (repo *BlogRepository) Delete(id string) error {
	if err := repo.DatabaseConnection.Delete(&domain.Blog{}, "id = ?", id).Error; err != nil {
		return err
	}
	return nil
}