// internal/repository/comment_repository.go
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

type CommentRepository struct {
	Coll *mongo.Collection // blogs collection (comments embedded)
}

func NewCommentRepository(coll *mongo.Collection) *CommentRepository {
	return &CommentRepository{Coll: coll}
}

// Create adds a comment to a specific blog's comments array.
func (r *CommentRepository) Create(ctx context.Context, blogId uuid.UUID, comment *domain.Comment) error {
	if comment.ID == uuid.Nil {
		comment.ID = uuid.New()
	}
	now := time.Now().UTC()
	comment.CreatedAt = now
	comment.UpdatedAt = now

	update := bson.M{
		"$push": bson.M{"comments": comment},
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

func (r *CommentRepository) GetByBlogID(ctx context.Context, blogId uuid.UUID) ([]domain.Comment, error) {
	var blog struct {
		Comments []domain.Comment `bson:"comments"`
	}
	if err := r.Coll.FindOne(ctx, bson.M{"_id": blogId}).Decode(&blog); err != nil {
		return nil, err
	}
	return blog.Comments, nil
}


func (r *CommentRepository) GetByID(ctx context.Context, commentId uuid.UUID) (*domain.Comment, error) {
	var result struct {
		Comments []domain.Comment `bson:"comments"`
	}
	err := r.Coll.FindOne(
		ctx,
		bson.M{"comments.id": commentId},
		options.FindOne().SetProjection(bson.M{
			"comments": bson.M{"$elemMatch": bson.M{"id": commentId}},
		}),
	).Decode(&result)
	if err != nil {
		return nil, err
	}
	if len(result.Comments) == 0 {
		return nil, errors.New("comment not found")
	}
	c := result.Comments[0]
	return &c, nil
}

func (r *CommentRepository) Update(ctx context.Context, commentId uuid.UUID, newContent string) error {
	update := bson.M{
		"$set": bson.M{
			"comments.$[c].content":   newContent,
			"comments.$[c].updatedAt": time.Now().UTC(),
		},
	}
	opts := options.Update().SetArrayFilters(options.ArrayFilters{
		Filters: bson.A{bson.M{"c.id": commentId}},
	})

	res, err := r.Coll.UpdateMany(ctx, bson.M{"comments.id": commentId}, update, opts)
	if err != nil {
		return err
	}
	if res.MatchedCount == 0 {
		return errors.New("comment not found")
	}
	return nil
}


func (r *CommentRepository) Delete(ctx context.Context, commentId uuid.UUID) error {
	update := bson.M{"$pull": bson.M{"comments": bson.M{"id": commentId}}}
	res, err := r.Coll.UpdateMany(ctx, bson.M{"comments.id": commentId}, update)
	if err != nil {
		return err
	}
	if res.ModifiedCount == 0 {
		return errors.New("comment not found")
	}
	return nil
}
