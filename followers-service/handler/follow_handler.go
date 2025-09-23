package handler

import (
	"github.com/gofiber/fiber/v2"
	"followers-service/service"
)

type FollowHandler struct{ svc *service.FollowService }

func NewFollowHandler() *FollowHandler { return &FollowHandler{svc: service.NewFollowService()} }

// Registruje rute na prosleđeni router (/api/follow)
func (h *FollowHandler) Register(r fiber.Router) {
	r.Use(JwtAuth)

	// POST /api/follow/:targetId
	r.Post("/:targetId", func(c *fiber.Ctx) error {
		u := c.Locals("userId")
		role := c.Locals("role")
		if u == nil || u.(string) == "" {
			return c.Status(401).JSON(fiber.Map{"error": "unauthorized"})
		}
		// Ako ti treba restrikcija po roli, ostavi:
		if role != nil && role.(string) != "tourist" {
			return c.Status(403).JSON(fiber.Map{"error": "only Tourist can follow"})
		}
		t := c.Params("targetId")
		if u.(string) == t {
			return c.Status(400).JSON(fiber.Map{"error": "cannot follow self"})
		}
		if err := h.svc.Follow(u.(string), t); err != nil {
			return c.Status(400).JSON(fiber.Map{"error": err.Error()})
		}
		return c.SendStatus(204)
	})

	// DELETE /api/follow/:targetId
	r.Delete("/:targetId", func(c *fiber.Ctx) error {
		u := c.Locals("userId")
		role := c.Locals("role")
		if u == nil || u.(string) == "" {
			return c.Status(401).JSON(fiber.Map{"error": "unauthorized"})
		}
		if role != nil && role.(string) != "tourist" {
			return c.Status(403).JSON(fiber.Map{"error": "only Tourist can unfollow"})
		}
		t := c.Params("targetId")
		if err := h.svc.Unfollow(u.(string), t); err != nil {
			return c.Status(400).JSON(fiber.Map{"error": err.Error()})
		}
		return c.SendStatus(204)
	})

	// GET /api/follow/followers/:userId
	r.Get("/followers/:userId", func(c *fiber.Ctx) error {
		list, err := h.svc.Followers(c.Params("userId"))
		if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	// GET /api/follow/following/:userId
	r.Get("/following/:userId", func(c *fiber.Ctx) error {
		list, err := h.svc.Following(c.Params("userId"))
		if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	// GET /api/follow/recommendations/:userId
	r.Get("/recommendations/:userId", func(c *fiber.Ctx) error {
		list, err := h.svc.Recommendations(c.Params("userId"))
		if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	// GET /api/follow/is-following?u=<id>&t=<id>
	r.Get("/is-following", func(c *fiber.Ctx) error {
		u, t := c.Query("u"), c.Query("t")
		if u == "" || t == "" {
			return c.Status(400).JSON(fiber.Map{"error": "u and t required"})
		}
		ok, err := h.svc.IsFollowing(u, t)
		if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(fiber.Map{"following": ok})
	})
}
