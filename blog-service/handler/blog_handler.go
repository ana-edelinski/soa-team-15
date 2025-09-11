package handler

import (
	"blog-service/domain"
	"blog-service/dto"
	"blog-service/service"
	"context"
	"encoding/json"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strconv"
	"time"

	"github.com/gorilla/mux"
	"github.com/google/uuid"
)

type BlogHandler struct {
	BlogService *service.BlogService
}

func NewBlogHandler(blogService *service.BlogService) *BlogHandler {
	return &BlogHandler{BlogService: blogService}
}

func (h *BlogHandler) CreateBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 8*time.Second)
	defer cancel()

	if err := r.ParseMultipartForm(10 << 20); err != nil { // 10MB
		http.Error(w, "Failed to parse form: "+err.Error(), http.StatusBadRequest)
		return
	}

	title := r.FormValue("title")
	content := r.FormValue("content")
	authorIDStr := r.FormValue("authorId")
	authorID, err := strconv.ParseInt(authorIDStr, 10, 64)
	if err != nil {
		http.Error(w, "Invalid author ID", http.StatusBadRequest)
		return
	}

	// Ensure upload dir exists (mapped in docker compose to /app/uploads)
	uploadDir := "./uploads"
	if err := os.MkdirAll(uploadDir, 0o755); err != nil {
		http.Error(w, "Cannot prepare upload dir: "+err.Error(), http.StatusInternalServerError)
		return
	}

	var imagePaths []string
	if form := r.MultipartForm; form != nil && form.File != nil {
		files := form.File["images"]
		for _, fh := range files {
			src, err := fh.Open()
			if err != nil {
				http.Error(w, "Failed to open file: "+err.Error(), http.StatusInternalServerError)
				return
			}
			defer src.Close()

			filename := uuid.New().String() + "_" + fh.Filename
			savePath := filepath.Join(uploadDir, filename)

			dst, err := os.Create(savePath)
			if err != nil {
				http.Error(w, "Failed to save file: "+err.Error(), http.StatusInternalServerError)
				return
			}
			if _, err := io.Copy(dst, src); err != nil {
				dst.Close()
				http.Error(w, "Failed to write file: "+err.Error(), http.StatusInternalServerError)
				return
			}
			dst.Close()

			// Public path you already use in FE
			imagePaths = append(imagePaths, "/uploads/"+filename)
		}
	}

	blog := domain.Blog{
		Title:      title,
		Content:    content,
		AuthorID:   authorID,
		ImagePaths: imagePaths,
		// CreatedAt/UpdatedAt set in repo; okay if zero here.
	}

	createdBlog, err := h.BlogService.Create(ctx, &blog)
	if err != nil {
		http.Error(w, "Failed to create blog: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	_ = json.NewEncoder(w).Encode(createdBlog)
}

func (h *BlogHandler) GetBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	id := vars["id"]

	blog, err := h.BlogService.GetById(ctx, id)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(blog)
}

func (h *BlogHandler) GetAllBlogs(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	page, _ := strconv.Atoi(r.URL.Query().Get("page"))
	limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
	if page <= 0 {
		page = 1
	}
	if limit <= 0 {
		limit = 10
	}

	blogs, err := h.BlogService.GetAll(ctx, page, limit)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(blogs)
}

func (h *BlogHandler) UpdateBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	id := vars["id"]

	var blogRequest dto.UpdateBlogRequest
	if err := json.NewDecoder(r.Body).Decode(&blogRequest); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	blog := domain.Blog{
		Title:   blogRequest.Title,
		Content: blogRequest.Content,
		// ImagePaths can be updated if you add it to DTO in future
	}

	updatedBlog, err := h.BlogService.Update(ctx, id, &blog)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(updatedBlog)
}

func (h *BlogHandler) DeleteBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	id := vars["id"]

	if err := h.BlogService.Delete(ctx, id); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
