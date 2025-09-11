package handler

import (
	"blog-service/domain"   // <-- add this
	"blog-service/dto"
	"blog-service/service"
	"context"
	"encoding/json"
	"net/http"
	"time"

	"github.com/gorilla/mux"
	"github.com/google/uuid"
)

type CommentHandler struct {
	Service *service.CommentService
}

func NewCommentHandler(service *service.CommentService) *CommentHandler {
	return &CommentHandler{Service: service}
}

// POST /api/blogs/{blogId}/comments
func (h *CommentHandler) CreateForBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	if _, err := uuid.Parse(blogIDStr); err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	var req dto.CreateCommentRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	comment := domain.Comment{
		AuthorID: req.AuthorID,
		Content:  req.Content,
	}

	if err := h.Service.Create(ctx, blogIDStr, &comment); err != nil {
		http.Error(w, "Failed to create comment", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	// You can return `req` if you want the old shape; returning `comment` includes generated id/timestamps.
	_ = json.NewEncoder(w).Encode(comment)
}

// GET /api/blogs/{blogId}/comments
func (h *CommentHandler) GetAllForBlog(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	if _, err := uuid.Parse(blogIDStr); err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	comments, err := h.Service.GetByBlogID(ctx, blogIDStr)
	if err != nil {
		http.Error(w, "Failed to fetch comments", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(comments)
}

// PUT /api/comments/{id}
func (h *CommentHandler) Update(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	commentID := vars["id"]

	var req dto.UpdateCommentRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}
	if req.Content == "" {
		http.Error(w, "Content must not be empty", http.StatusBadRequest)
		return
	}

	if err := h.Service.Update(ctx, commentID, req.Content); err != nil {
		http.Error(w, "Failed to update comment", http.StatusInternalServerError)
		return
	}

	updated, err := h.Service.GetByID(ctx, commentID)
	if err != nil {
		http.Error(w, "Updated comment not found", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(updated)
}

// DELETE /api/comments/{id}
func (h *CommentHandler) Delete(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	commentID := vars["id"]

	if err := h.Service.Delete(ctx, commentID); err != nil {
		http.Error(w, "Failed to delete comment", http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
