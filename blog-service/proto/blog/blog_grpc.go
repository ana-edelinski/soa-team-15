package blog

import (
	"blog-service/domain"
	"blog-service/service"
	"context"
	"os"

	"github.com/google/uuid"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

type BlogGrpcServer struct {
	UnimplementedBlogServiceServer
	BlogService *service.BlogService
}

func NewBlogGrpcServer(s *service.BlogService) *BlogGrpcServer {
	return &BlogGrpcServer{BlogService: s}
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
