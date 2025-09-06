package startup

import (
	"blog-service/config"
	"blog-service/domain"
	"blog-service/handler"
	"blog-service/repository"
	"blog-service/service"
	"fmt"
	"log"
	"net"
	"net/http"
	"time"

	"github.com/gorilla/mux"
	"github.com/rs/cors"
	"google.golang.org/grpc"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"

	blog "blog-service/proto/blog"
)

type Server struct {
	config *config.Config
}

func NewServer(config *config.Config) *Server {
	return &Server{
		config: config,
	}
}

func (server *Server) InitializeDb() *gorm.DB {
	dsn := fmt.Sprintf(
		"host=%s user=%s password=%s dbname=%s port=%s sslmode=disable",
		server.config.DBHost,
		server.config.DBUser,
		server.config.DBPass,
		server.config.DBName,
		server.config.DBPort,
	)

	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
	if err != nil {
		log.Fatal(err)
	}

	if err := db.AutoMigrate(&domain.Blog{}, &domain.Comment{}, &domain.Like{}); err != nil {
		log.Fatal(err)
	}

	return db
}

func (server *Server) Start() {
	db := server.InitializeDb()

	blogRepository := &repository.BlogRepository{DatabaseConnection: db}
	blogService := &service.BlogService{BlogRepository: blogRepository}
	blogHandler := handler.NewBlogHandler(blogService)

	go func() {
		lis, err := net.Listen("tcp", fmt.Sprintf(":%s", server.config.GrpcPort))
		if err != nil {
			log.Fatalf("failed to listen: %v", err)
		}

		grpcServer := grpc.NewServer()
		blog.RegisterBlogServiceServer(grpcServer, blog.NewBlogGrpcServer(blogService))

		log.Printf("gRPC server for BlogService listening on %s", server.config.GrpcPort)
		if err := grpcServer.Serve(lis); err != nil {
			log.Fatalf("failed to serve gRPC: %v", err)
		}
	}()

	commentRepo := repository.NewCommentRepository(db)
	commentService := service.NewCommentService(commentRepo)
	commentHandler := handler.NewCommentHandler(commentService)

	likeRepo := repository.NewLikeRepository(db)
	likeService := service.NewLikeService(likeRepo)
	likeHandler := handler.NewLikeHandler(likeService)

	router := mux.NewRouter()
	router.HandleFunc("/api/blogs", blogHandler.CreateBlog).Methods("POST")
	router.HandleFunc("/api/blogs/{id}", blogHandler.GetBlog).Methods("GET")
	router.HandleFunc("/api/blogs", blogHandler.GetAllBlogs).Methods("GET")
	router.HandleFunc("/api/blogs/{id}", blogHandler.UpdateBlog).Methods("PUT")
	router.HandleFunc("/api/blogs/{id}", blogHandler.DeleteBlog).Methods("DELETE")

	router.HandleFunc("/api/blogs/{blogId}/comments", commentHandler.CreateForBlog).Methods("POST")
	router.HandleFunc("/api/comments/{id}", commentHandler.Update).Methods("PUT")
	router.HandleFunc("/api/comments/{id}", commentHandler.Delete).Methods("DELETE")
	router.HandleFunc("/api/blogs/{blogId}/comments", commentHandler.GetAllForBlog).Methods("GET")

	router.HandleFunc("/api/blogs/{blogId}/like", likeHandler.ToggleLike).Methods("POST")
	router.HandleFunc("/api/blogs/{blogId}/like", likeHandler.CountLikes).Methods("GET")
	router.HandleFunc("/api/blogs/{blogId}/likedByMe", likeHandler.IsLikedByUser).Methods("GET")

	router.PathPrefix("/uploads/").Handler(http.StripPrefix("/uploads/", http.FileServer(http.Dir("./uploads/"))))

	// Configure CORS
	c := cors.New(cors.Options{
		AllowedOrigins: []string{
			"http://localhost:4200", // Angular dev server
			"http://frontend:80",    // Docker frontend service
		},
		AllowedMethods: []string{
			http.MethodGet,
			http.MethodPost,
			http.MethodPut,
			http.MethodDelete,
			http.MethodOptions,
		},
		AllowedHeaders: []string{
			"Accept",
			"Authorization",
			"Content-Type",
			"X-CSRF-Token",
		},
		ExposedHeaders: []string{
			"Link",
		},
		AllowCredentials: true,
		MaxAge:           300,
	})

	// Wrap your router with the CORS middleware
	handler := c.Handler(router)

	srv := &http.Server{
		Handler:      handler,
		Addr:         fmt.Sprintf(":%s", server.config.Port),
		WriteTimeout: 15 * time.Second,
		ReadTimeout:  15 * time.Second,
	}

	log.Printf("Server starting on port %s", server.config.Port)
	log.Fatal(srv.ListenAndServe())
}
