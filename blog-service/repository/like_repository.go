// internal/repository/like_repository.go
package repository

import (
	"blog-service/domain"
	"context"
	"errors"
	"time"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
)

type LikeRepository struct {
	Coll *mongo.Collection // blogs collection (likes embedded)
}

func NewLikeRepository(coll *mongo.Collection) *LikeRepository {
	return &LikeRepository{Coll: coll}
}

// Create adds a like to a blog. Uses $addToSet by AuthorID to keep it idempotent.
// If you need unique by (blogId, authorId), this is perfect.
// If you also want a Like.ID, we generate it and still keep idempotency using $addToSet on an object that matches authorId.
func (r *LikeRepository) Create(ctx context.Context, blogId uuid.UUID, authorId int64) error {
	like := domain.Like{
		ID:        uuid.New(),
		AuthorID:  authorId,
		CreatedAt: time.Now().UTC(),
	}
	// Ensure uniqueness by authorId:
	update := bson.M{
		"$addToSet": bson.M{
			"likes": bson.M{
				"authorId":  like.AuthorID,
				"id":        like.ID,
				"createdAt": like.CreatedAt,
			},
		},
	}
	res, err := r.Coll.UpdateByID(ctx, blogId, update)
	if err != nil {
		return err
	}
	if res.MatchedCount == 0 {
		return errors.New("blog not found")
	}
	return nil
}

// Delete removes like(s) by author on a blog.
func (r *LikeRepository) Delete(ctx context.Context, blogId uuid.UUID, authorId int64) error {
	update := bson.M{
		"$pull": bson.M{"likes": bson.M{"authorId": authorId}},
	}
	res, err := r.Coll.UpdateByID(ctx, blogId, update)
	if err != nil {
		return err
	}
	if res.MatchedCount == 0 {
		return errors.New("blog not found")
	}
	return nil
}

// Exists checks if a like by author exists on a blog.
func (r *LikeRepository) Exists(ctx context.Context, blogId uuid.UUID, authorId int64) (bool, error) {
	filter := bson.M{"_id": blogId, "likes.authorId": authorId}
	err := r.Coll.FindOne(ctx, filter).Err()
	if errors.Is(err, mongo.ErrNoDocuments) {
		return false, nil
	}
	return err == nil, err
}

// CountForBlog returns the number of likes on a blog.
func (r *LikeRepository) CountForBlog(ctx context.Context, blogId uuid.UUID) (int64, error) {
	var result struct {
		Likes []domain.Like `bson:"likes"`
	}
	err := r.Coll.FindOne(ctx, bson.M{"_id": blogId}).Decode(&result)
	if err != nil {
		return 0, err
	}
	return int64(len(result.Likes)), nil
}
