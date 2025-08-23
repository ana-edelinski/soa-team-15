package dto

type CreateLikeRequest struct {
	BlogID   string `json:"blogId"`
	AuthorID int64  `json:"authorId"`
}
