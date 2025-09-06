package blog

import (
	"blog-service/domain"
	"blog-service/service"
	"context"
	"os"
	"time"

	"github.com/google/uuid"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

type BlogGrpcServer struct {
	UnimplementedBlogServiceServer
	BlogService    *service.BlogService
	LikeService    *service.LikeService
	CommentService *service.CommentService
}

func NewBlogGrpcServer(
	blogService *service.BlogService,
	likeService *service.LikeService,
	commentService *service.CommentService,
) *BlogGrpcServer {
	return &BlogGrpcServer{
		BlogService:    blogService,
		LikeService:    likeService,
		CommentService: commentService,
	}
}

func (s *BlogGrpcServer) CreateBlog(ctx context.Context, req *CreateBlogRequest) (*BlogDto, error) {
	blog := &domain.Blog{
		Title:      req.Title,
		Content:    req.Content,
		AuthorID:   req.AuthorId,
		ImagePaths: req.ImagePaths,
	}
	created, err := s.BlogService.Create(blog)
	if err != nil {
		return nil, status.Errorf(codes.Internal, err.Error())
	}
	return toBlogDto(created), nil
}

func (s *BlogGrpcServer) GetBlog(ctx context.Context, req *GetBlogRequest) (*BlogDto, error) {
	blog, err := s.BlogService.GetById(req.Id)
	if err != nil {
		return nil, status.Errorf(codes.NotFound, err.Error())
	}
	return toBlogDto(blog), nil
}

func (s *BlogGrpcServer) GetAllBlogs(ctx context.Context, req *GetAllBlogsRequest) (*BlogsListResponse, error) {
	page := int(req.Page)
	limit := int(req.Limit)

	if page <= 0 {
		page = 1
	}
	if limit <= 0 {
		limit = 100 // ili koliko želiš po defaultu
	}

	blogsList, err := s.BlogService.GetAll(page, limit)
	if err != nil {
		return nil, status.Errorf(codes.Internal, err.Error())
	}

	resp := &BlogsListResponse{}
	for _, b := range *blogsList {
		resp.Blogs = append(resp.Blogs, toBlogDto(&b))
	}
	return resp, nil
}

func (s *BlogGrpcServer) UpdateBlog(ctx context.Context, req *UpdateBlogRequest) (*BlogDto, error) {
	blog := &domain.Blog{
		Title:   req.Title,
		Content: req.Content,
	}
	updated, err := s.BlogService.Update(req.Id, blog)
	if err != nil {
		return nil, status.Errorf(codes.Internal, err.Error())
	}
	return toBlogDto(updated), nil
}

func (s *BlogGrpcServer) DeleteBlog(ctx context.Context, req *DeleteBlogRequest) (*DeleteBlogResponse, error) {
	if err := s.BlogService.Delete(req.Id); err != nil {
		return &DeleteBlogResponse{Success: false}, status.Errorf(codes.Internal, err.Error())
	}
	return &DeleteBlogResponse{Success: true}, nil
}

func toBlogDto(blog *domain.Blog) *BlogDto {
	return &BlogDto{
		Id:       blog.Id.String(),
		Title:    blog.Title,
		Content:  blog.Content,
		AuthorId: blog.AuthorID,
		Images:   blog.ImagePaths,
	}
}

func (s *BlogGrpcServer) UploadImage(ctx context.Context, req *UploadImageRequest) (*UploadImageResponse, error) {
	// Osiguraj da postoji ./uploads folder
	if err := os.MkdirAll("./uploads", os.ModePerm); err != nil {
		return nil, status.Errorf(codes.Internal, "failed to create upload dir: %v", err)
	}

	filename := uuid.New().String() + "_" + req.Filename
	savePath := "./uploads/" + filename

	// Zapiši fajl
	err := os.WriteFile(savePath, req.File, 0644)
	if err != nil {
		return nil, status.Errorf(codes.Internal, "failed to save file: %v", err)
	}

	return &UploadImageResponse{
		Url: "/uploads/" + filename,
	}, nil
}

func (s *BlogGrpcServer) ToggleLike(ctx context.Context, req *ToggleLikeRequest) (*ToggleLikeResponse, error) {
	liked, err := s.BlogService.LikeService.HasUserLiked(req.BlogId, req.AuthorId)
	if err != nil {
		return nil, status.Errorf(codes.Internal, "failed to check like: %v", err)
	}

	if liked {
		// već lajkovao → unlikujemo
		if err := s.BlogService.LikeService.UnlikePost(req.BlogId, req.AuthorId); err != nil {
			return nil, status.Errorf(codes.Internal, "failed to unlike: %v", err)
		}
		return &ToggleLikeResponse{Liked: false}, nil
	} else {
		// nije lajkovao → dodajemo like
		like := &domain.Like{
			BlogID:   uuid.MustParse(req.BlogId),
			AuthorID: req.AuthorId,
		}
		if err := s.BlogService.LikeService.LikePost(like); err != nil {
			return nil, status.Errorf(codes.Internal, "failed to like: %v", err)
		}
		return &ToggleLikeResponse{Liked: true}, nil
	}
}

func (s *BlogGrpcServer) CountLikes(ctx context.Context, req *CountLikesRequest) (*CountLikesResponse, error) {
	count, err := s.BlogService.LikeService.CountLikes(req.BlogId)
	if err != nil {
		return nil, status.Errorf(codes.Internal, "failed to count likes: %v", err)
	}
	return &CountLikesResponse{LikeCount: count}, nil
}

func (s *BlogGrpcServer) IsLikedByUser(ctx context.Context, req *IsLikedByUserRequest) (*IsLikedByUserResponse, error) {
	liked, err := s.BlogService.LikeService.HasUserLiked(req.BlogId, req.AuthorId)
	if err != nil {
		return nil, status.Errorf(codes.Internal, "failed to check like: %v", err)
	}
	return &IsLikedByUserResponse{Liked: liked}, nil
}

// CreateComment
func (s *BlogGrpcServer) CreateComment(ctx context.Context, req *CreateCommentRequest) (*CommentDto, error) {
	comment := &domain.Comment{
		BlogID:   uuid.MustParse(req.BlogId),
		AuthorID: req.AuthorId,
		Content:  req.Content,
	}

	if err := s.CommentService.Create(comment); err != nil {
		return nil, status.Errorf(codes.Internal, "failed to create comment: %v", err)
	}

	return toCommentDto(comment), nil
}

// GetComments
func (s *BlogGrpcServer) GetComments(ctx context.Context, req *GetCommentsRequest) (*CommentsListResponse, error) {
	comments, err := s.CommentService.GetByBlogID(req.BlogId)
	if err != nil {
		return nil, status.Errorf(codes.Internal, "failed to get comments: %v", err)
	}

	resp := &CommentsListResponse{}
	for _, c := range comments {
		resp.Comments = append(resp.Comments, toCommentDto(&c))
	}
	return resp, nil
}

// UpdateComment
func (s *BlogGrpcServer) UpdateComment(ctx context.Context, req *UpdateCommentRequest) (*CommentDto, error) {
	existing, err := s.CommentService.GetByID(req.Id)
	if err != nil {
		return nil, status.Errorf(codes.NotFound, "comment not found: %v", err)
	}

	existing.Content = req.Content
	existing.UpdatedAt = time.Now()

	if err := s.CommentService.Update(existing); err != nil {
		return nil, status.Errorf(codes.Internal, "failed to update comment: %v", err)
	}

	return toCommentDto(existing), nil
}

// DeleteComment
func (s *BlogGrpcServer) DeleteComment(ctx context.Context, req *DeleteCommentRequest) (*DeleteCommentResponse, error) {
	if err := s.CommentService.Delete(req.Id); err != nil {
		return &DeleteCommentResponse{Success: false}, status.Errorf(codes.Internal, "failed to delete: %v", err)
	}
	return &DeleteCommentResponse{Success: true}, nil
}

func toCommentDto(c *domain.Comment) *CommentDto {
	return &CommentDto{
		Id:        c.ID.String(),
		BlogId:    c.BlogID.String(),
		AuthorId:  c.AuthorID,
		Content:   c.Content,
		CreatedAt: c.CreatedAt.String(),
		UpdatedAt: c.UpdatedAt.String(),
	}
}
