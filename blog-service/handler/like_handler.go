package handler

import (
	"blog-service/service"
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/gorilla/mux"
	"github.com/google/uuid"
)

type LikeHandler struct {
	Service *service.LikeService
}

func NewLikeHandler(service *service.LikeService) *LikeHandler {
	return &LikeHandler{Service: service}
}

// POST /api/blogs/{blogId}/likes   (toggle)
func (h *LikeHandler) ToggleLike(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	if _, err := uuid.Parse(blogIDStr); err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	var body struct {
		AuthorID int64 `json:"authorId"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	liked, err := h.Service.HasUserLiked(ctx, blogIDStr, body.AuthorID)
	if err != nil {
		http.Error(w, "Failed to check like status", http.StatusInternalServerError)
		return
	}

	if liked {
		if err := h.Service.UnlikePost(ctx, blogIDStr, body.AuthorID); err != nil {
			http.Error(w, "Failed to unlike post", http.StatusInternalServerError)
			return
		}
		w.WriteHeader(http.StatusNoContent)
		return
	}

	if err := h.Service.LikePost(ctx, blogIDStr, body.AuthorID); err != nil {
		http.Error(w, "Failed to like post", http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusCreated)
}

// GET /api/blogs/{blogId}/likes
func (h *LikeHandler) CountLikes(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]

	count, err := h.Service.CountLikes(ctx, blogIDStr)
	if err != nil {
		http.Error(w, "Failed to count likes", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(map[string]int64{"likeCount": count})
}

// GET /api/blogs/{blogId}/likedByMe?authorId=123
func (h *LikeHandler) IsLikedByUser(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	authorIDStr := r.URL.Query().Get("authorId")

	var authorID int64
	if _, err := fmt.Sscan(authorIDStr, &authorID); err != nil {
		http.Error(w, "Invalid authorId", http.StatusBadRequest)
		return
	}

	liked, err := h.Service.HasUserLiked(ctx, blogIDStr, authorID)
	if err != nil {
		http.Error(w, "Failed to check like status", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(map[string]bool{"liked": liked})
}
