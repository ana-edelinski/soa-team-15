package startup

import (
	"blog-service/config"
	"blog-service/handler"
	"blog-service/repository"
	"blog-service/service"
	"context"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/gorilla/mux"
	"github.com/rs/cors"

	// Mongo
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

type Server struct {
	config *config.Config
}

func NewServer(cfg *config.Config) *Server {
	return &Server{config: cfg}
}

func (s *Server) connectMongo(ctx context.Context) (*mongo.Client, *mongo.Database, *mongo.Collection) {
	client, err := mongo.Connect(ctx, options.Client().ApplyURI(s.config.MongoURI))
	if err != nil {
		log.Fatal(err)
	}
	if err := client.Ping(ctx, nil); err != nil {
		log.Fatal(err)
	}
	db := client.Database(s.config.MongoDB)
	blogs := db.Collection("blogs")

	// Recommended indexes
	_, _ = blogs.Indexes().CreateMany(ctx, []mongo.IndexModel{
		{Keys: bson.D{{Key: "createdAt", Value: -1}}},
		{Keys: bson.D{{Key: "likes.authorId", Value: 1}}},
	})

	return client, db, blogs
}

func (s *Server) Start() {
	// Mongo init
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	_, _, blogsColl := s.connectMongo(ctx)

	// Repositories (Mongo-backed, using same routes as before)
	blogRepo := repository.NewBlogRepository(blogsColl)
	commentRepo := repository.NewCommentRepository(blogsColl)
	likeRepo := repository.NewLikeRepository(blogsColl)

	// Services (context-aware)
	blogService := service.NewBlogService(blogRepo)
	commentService := service.NewCommentService(commentRepo)
	likeService := service.NewLikeService(likeRepo)

	// Handlers (the ones I sent adapted for Mongo)
	blogHandler := handler.NewBlogHandler(blogService)
	commentHandler := handler.NewCommentHandler(commentService)
	likeHandler := handler.NewLikeHandler(likeService)

	// Router — KEEP YOUR EXISTING ROUTES (no FE changes)
	r := mux.NewRouter()
	r.HandleFunc("/api/blogs", blogHandler.CreateBlog).Methods(http.MethodPost)
	r.HandleFunc("/api/blogs/{id}", blogHandler.GetBlog).Methods(http.MethodGet)
	r.HandleFunc("/api/blogs", blogHandler.GetAllBlogs).Methods(http.MethodGet)
	r.HandleFunc("/api/blogs/{id}", blogHandler.UpdateBlog).Methods(http.MethodPut)
	r.HandleFunc("/api/blogs/{id}", blogHandler.DeleteBlog).Methods(http.MethodDelete)

	r.HandleFunc("/api/blogs/{blogId}/comments", commentHandler.CreateForBlog).Methods(http.MethodPost)
	r.HandleFunc("/api/comments/{id}", commentHandler.Update).Methods(http.MethodPut)
	r.HandleFunc("/api/comments/{id}", commentHandler.Delete).Methods(http.MethodDelete)
	r.HandleFunc("/api/blogs/{blogId}/comments", commentHandler.GetAllForBlog).Methods(http.MethodGet)

	// You currently use singular "/like" — we're keeping it to avoid FE changes
	r.HandleFunc("/api/blogs/{blogId}/like", likeHandler.ToggleLike).Methods(http.MethodPost)
	r.HandleFunc("/api/blogs/{blogId}/like", likeHandler.CountLikes).Methods(http.MethodGet)
	r.HandleFunc("/api/blogs/{blogId}/likedByMe", likeHandler.IsLikedByUser).Methods(http.MethodGet)

	// Static file serving for uploads (same as before)
	r.PathPrefix("/uploads/").Handler(http.StripPrefix("/uploads/", http.FileServer(http.Dir(s.config.UploadDir))))

	// CORS (unchanged)
	c := cors.New(cors.Options{
		AllowedOrigins: []string{
			"http://localhost:4200",
			"http://frontend:80",
		},
		AllowedMethods: []string{
			http.MethodGet, http.MethodPost, http.MethodPut, http.MethodDelete, http.MethodOptions,
		},
		AllowedHeaders: []string{
			"Accept", "Authorization", "Content-Type", "X-CSRF-Token",
		},
		ExposedHeaders: []string{"Link"},
		AllowCredentials: true,
		MaxAge: 300,
	})
	handlerWithCORS := c.Handler(r)

	srv := &http.Server{
		Handler:      handlerWithCORS,
		Addr:         fmt.Sprintf(":%s", s.config.Port),
		WriteTimeout: 15 * time.Second,
		ReadTimeout:  15 * time.Second,
	}

	log.Printf("Blog service (Mongo) starting on port %s", s.config.Port)
	log.Fatal(srv.ListenAndServe())
}
