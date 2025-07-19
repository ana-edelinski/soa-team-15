package startup

import (
	"blog-service/config"
	"blog-service/handler"
	"blog-service/repository"
	"blog-service/service"
	"fmt"
	"log"
	"net/http"
	"time"
	"blog-service/domain"
	"github.com/gorilla/mux"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
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

    if err := db.AutoMigrate(&domain.Blog{}); err != nil {
        log.Fatal(err)
    }

    return db
}


func (server *Server) Start() {
	db := server.InitializeDb()

	blogRepository := &repository.BlogRepository{DatabaseConnection: db}
	blogService := &service.BlogService{BlogRepository: blogRepository}
	blogHandler := handler.NewBlogHandler(blogService)

	router := mux.NewRouter()
	router.HandleFunc("/api/blogs", blogHandler.CreateBlog).Methods("POST")
	router.HandleFunc("/api/blogs/{id}", blogHandler.GetBlog).Methods("GET")
	router.HandleFunc("/api/blogs", blogHandler.GetAllBlogs).Methods("GET")
	router.HandleFunc("/api/blogs/{id}", blogHandler.UpdateBlog).Methods("PUT")
	router.HandleFunc("/api/blogs/{id}", blogHandler.DeleteBlog).Methods("DELETE")

	srv := &http.Server{
		Handler:      router,
		Addr:         fmt.Sprintf(":%s", server.config.Port),
		WriteTimeout: 15 * time.Second,
		ReadTimeout:  15 * time.Second,
	}

	log.Printf("Server starting on port %s", server.config.Port)
	log.Fatal(srv.ListenAndServe())
}