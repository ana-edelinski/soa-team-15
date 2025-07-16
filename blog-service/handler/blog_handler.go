package handler

import (
	"blog-service/domain"
	"blog-service/dto"
	"blog-service/service"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/gorilla/mux"
)

type BlogHandler struct {
	BlogService *service.BlogService
}

func NewBlogHandler(blogService *service.BlogService) *BlogHandler {
	return &BlogHandler{BlogService: blogService}
}

func (h *BlogHandler) CreateBlog(w http.ResponseWriter, r *http.Request) {
	var blogRequest dto.CreateBlogRequest
	if err := json.NewDecoder(r.Body).Decode(&blogRequest); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	blog := domain.Blog{
		Title:    blogRequest.Title,
		Content:  blogRequest.Content,
		AuthorID: blogRequest.AuthorID,
	}

	createdBlog, err := h.BlogService.Create(&blog)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
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