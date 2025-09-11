// internal/repository/blog_repository.go
package repository

import (
	"blog-service/domain"
	"context"
	"errors"
	"time"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

type BlogRepository struct {
	Coll *mongo.Collection
}

func NewBlogRepository(coll *mongo.Collection) *BlogRepository {
	return &BlogRepository{Coll: coll}
}

func (r *BlogRepository) Create(ctx context.Context, blog *domain.Blog) (*domain.Blog, error) {
	if blog.ID == uuid.Nil {
		blog.ID = uuid.New()
	}
	now := time.Now().UTC()
	if blog.CreatedAt.IsZero() {
		blog.CreatedAt = now
	}
	blog.UpdatedAt = now

	// ensure arrays are stored as [] (not null)
	if blog.ImagePaths == nil {
		blog.ImagePaths = []string{}
	}
	if blog.Comments == nil {
		blog.Comments = []domain.Comment{}
	}
	if blog.Likes == nil {
		blog.Likes = []domain.Like{}
	}

	_, err := r.Coll.InsertOne(ctx, blog)
	if err != nil {
		return nil, err
	}
	return blog, nil
}


func (r *BlogRepository) GetAll(ctx context.Context, page, limit int) (*[]domain.Blog, error) {
	if page < 1 {
		page = 1
	}
	if limit <= 0 || limit > 1000 {
		limit = 10
	}
	skip := int64((page - 1) * limit)
	lim := int64(limit)

	opts := options.Find().
		SetSkip(skip).
		SetLimit(lim).
		SetSort(bson.D{{Key: "createdAt", Value: -1}})

	cur, err := r.Coll.Find(ctx, bson.D{}, opts)
	if err != nil {
		return nil, err
	}
	defer cur.Close(ctx)

	var blogs []domain.Blog
	if err := cur.All(ctx, &blogs); err != nil {
		return nil, err
	}
	return &blogs, nil
}

func (r *BlogRepository) GetById(ctx context.Context, id uuid.UUID) (*domain.Blog, error) {
	var blog domain.Blog
	err := r.Coll.FindOne(ctx, bson.M{"_id": id}).Decode(&blog)
	if err != nil {
		return nil, err
	}
	return &blog, nil
}

func (r *BlogRepository) Update(ctx context.Context, id uuid.UUID, blog *domain.Blog) (*domain.Blog, error) {
	update := bson.M{
		"$set": bson.M{
			"title":      blog.Title,
			"content":    blog.Content,
			"imagePaths": blog.ImagePaths,
			"updatedAt":  time.Now().UTC(),
		},
	}

	res := r.Coll.FindOneAndUpdate(
		ctx,
		bson.M{"_id": id},
		update,
		options.FindOneAndUpdate().SetReturnDocument(options.After),
	)

	if err := res.Err(); err != nil {
		return nil, err
	}

	var updated domain.Blog
	if err := res.Decode(&updated); err != nil {
		return nil, err
	}
	return &updated, nil
}


func (r *BlogRepository) Delete(ctx context.Context, id uuid.UUID) error {
	result, err := r.Coll.DeleteOne(ctx, bson.M{"_id": id})
	if err != nil {
		return err
	}
	if result.DeletedCount == 0 {
		return errors.New("blog not found")
	}
	return nil
}
