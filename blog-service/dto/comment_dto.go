package dto

type CreateCommentRequest struct {
	BlogID   string `json:"blogId"`
	AuthorID int64  `json:"authorId"`
	Content  string `json:"content"`
}

type UpdateCommentRequest struct {
	Content string `json:"content"`
}

