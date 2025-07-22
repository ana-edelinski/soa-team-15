package handler

import (
	"blog-service/domain"
	"blog-service/dto"
	"blog-service/service"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/gorilla/mux"
	"github.com/google/uuid"

	"io"
    "os"
)

type BlogHandler struct {
	BlogService *service.BlogService
}

func NewBlogHandler(blogService *service.BlogService) *BlogHandler {
	return &BlogHandler{BlogService: blogService}
}

func (h *BlogHandler) CreateBlog(w http.ResponseWriter, r *http.Request) {
	err := r.ParseMultipartForm(10 << 20) // 10MB max memory
	if err != nil {
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

	form := r.MultipartForm
	files := form.File["images"]
	var imagePaths []string

	for _, fileHeader := range files {
		file, err := fileHeader.Open()
		if err != nil {
			http.Error(w, "Failed to open file: "+err.Error(), http.StatusInternalServerError)
			return
		}
		defer file.Close()

		filename := uuid.New().String() + "_" + fileHeader.Filename
		savePath := "./uploads/" + filename

		dst, err := os.Create(savePath)
		if err != nil {
			http.Error(w, "Failed to save file: "+err.Error(), http.StatusInternalServerError)
			return
		}
		defer dst.Close()

		_, err = io.Copy(dst, file)
		if err != nil {
			http.Error(w, "Failed to write file: "+err.Error(), http.StatusInternalServerError)
			return
		}

		imagePaths = append(imagePaths, "/uploads/"+filename)
	}

	imagesJSON, err := json.Marshal(imagePaths)
	if err != nil {
		http.Error(w, "Failed to marshal image paths: "+err.Error(), http.StatusInternalServerError)
		return
	}

	blog := domain.Blog{
		Title:      title,
		Content:    content,
		AuthorID:   authorID,
		Images:     string(imagesJSON),
		ImagePaths: imagePaths,
	}

	createdBlog, err := h.BlogService.Create(&blog)
	if err != nil {
		//http.Error(w, err.Error(), http.StatusInternalServerError)
		http.Error(w, string(imagesJSON), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(createdBlog)
}


func (h *BlogHandler) GetBlog(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id := vars["id"]

	blog, err := h.BlogService.GetById(id)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(blog)
}

func (h *BlogHandler) GetAllBlogs(w http.ResponseWriter, r *http.Request) {
	page, _ := strconv.Atoi(r.URL.Query().Get("page"))
	limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))

	if page == 0 {
		page = 1
	}
	if limit == 0 {
		limit = 10
	}

	blogs, err := h.BlogService.GetAll(page, limit)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(blogs)
}

func (h *BlogHandler) UpdateBlog(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id := vars["id"]

	var blogRequest dto.UpdateBlogRequest
	if err := json.NewDecoder(r.Body).Decode(&blogRequest); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	blog := domain.Blog{
		Title:    blogRequest.Title,
		Content:  blogRequest.Content,
	}

	updatedBlog, err := h.BlogService.Update(id, &blog)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(updatedBlog)
}

func (h *BlogHandler) DeleteBlog(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id := vars["id"]

	if err := h.BlogService.Delete(id); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}