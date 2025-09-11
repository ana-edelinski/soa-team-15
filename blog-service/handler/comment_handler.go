package handler

import (
	"blog-service/domain"
	"blog-service/dto"
	"blog-service/service"
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
	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]

	blogID, err := uuid.Parse(blogIDStr)
	if err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	var req dto.CreateCommentRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	comment := domain.Comment{
		BlogID:   blogID,
		AuthorID: req.AuthorID,
		Content:  req.Content,
	}

	if err := h.Service.Create(&comment); err != nil {
		http.Error(w, "Failed to create comment", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(comment)
}

// GET /api/blogs/{blogId}/comments
func (h *CommentHandler) GetAllForBlog(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]

	//Validacija UUID
	_, err := uuid.Parse(blogIDStr)
	if err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	comments, err := h.Service.GetByBlogID(blogIDStr)
	if err != nil {
		http.Error(w, "Failed to fetch comments", http.StatusInternalServerError)
		return
	}

	// Odgovor
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(comments)
}


// PUT /api/comments/{id}
func (h *CommentHandler) Update(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	commentID := vars["id"]

	existing, err := h.Service.GetByID(commentID)
	if err != nil {
		http.Error(w, "Comment not found", http.StatusNotFound)
		return
	}

	var req dto.UpdateCommentRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	existing.Content = req.Content
	existing.UpdatedAt = time.Now()

	if err := h.Service.Update(existing); err != nil {
		http.Error(w, "Failed to update comment", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(existing)
}

// DELETE /api/comments/{id}
func (h *CommentHandler) Delete(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	commentID := vars["id"]

	if err := h.Service.Delete(commentID); err != nil {
		http.Error(w, "Failed to delete comment", http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
