package handler

import (
	"blog-service/domain"
	"blog-service/dto"
	"blog-service/service"
	"encoding/json"
	"net/http"

	"github.com/gorilla/mux"
	"github.com/google/uuid"
	"fmt"
)

type LikeHandler struct {
	Service *service.LikeService
}

func NewLikeHandler(service *service.LikeService) *LikeHandler {
	return &LikeHandler{Service: service}
}

// POST /api/blogs/{blogId}/likes
// Toggle like: like if not liked, unlike if already liked
func (h *LikeHandler) ToggleLike(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	blogID, err := uuid.Parse(blogIDStr)
	if err != nil {
		http.Error(w, "Invalid blog ID", http.StatusBadRequest)
		return
	}

	var req dto.CreateLikeRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	liked, err := h.Service.HasUserLiked(blogIDStr, req.AuthorID)
	if err != nil {
		http.Error(w, "Failed to check like status", http.StatusInternalServerError)
		return
	}

	if liked {
		err = h.Service.UnlikePost(blogIDStr, req.AuthorID)
		if err != nil {
			http.Error(w, "Failed to unlike post", http.StatusInternalServerError)
			return
		}
		w.WriteHeader(http.StatusNoContent)
	} else {
		like := domain.Like{
			BlogID:   blogID,
			AuthorID: req.AuthorID,
		}
		err = h.Service.LikePost(&like)
		if err != nil {
			http.Error(w, "Failed to like post", http.StatusInternalServerError)
			return
		}
		w.WriteHeader(http.StatusCreated)
	}
}

// GET /api/blogs/{blogId}/likes
func (h *LikeHandler) CountLikes(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]

	count, err := h.Service.CountLikes(blogIDStr)
	if err != nil {
		http.Error(w, "Failed to count likes", http.StatusInternalServerError)
		return
	}

	json.NewEncoder(w).Encode(map[string]int64{"likeCount": count})
}

// GET /api/blogs/{blogId}/likedByMe?authorId=123
func (h *LikeHandler) IsLikedByUser(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	blogIDStr := vars["blogId"]
	authorIDStr := r.URL.Query().Get("authorId")

	// Parse authorId
	var authorID int64
	_, err := fmt.Sscan(authorIDStr, &authorID)
	if err != nil {
		http.Error(w, "Invalid authorId", http.StatusBadRequest)
		return
	}

	liked, err := h.Service.HasUserLiked(blogIDStr, authorID)
	if err != nil {
		http.Error(w, "Failed to check like status", http.StatusInternalServerError)
		return
	}

	json.NewEncoder(w).Encode(map[string]bool{"liked": liked})
}
