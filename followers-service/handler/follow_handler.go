package handler

import (
	"strings"

	"github.com/gofiber/fiber/v2"
	"followers-service/service"
)

type FollowHandler struct{ svc *service.FollowService }

func NewFollowHandler() *FollowHandler { return &FollowHandler{svc: service.NewFollowService()} }

func (h *FollowHandler) Register(app *fiber.App) {
	app.Use(JwtAuth)

	app.Post("/follow/:targetId", func(c *fiber.Ctx) error {
		u := c.Locals("userId")
		role := strings.ToLower(c.Locals("role").(string))
		if u == nil || u.(string) == "" { return c.Status(401).JSON(fiber.Map{"error":"unauthorized"}) }
		if role != "tourist" { return c.Status(403).JSON(fiber.Map{"error":"only Tourist can follow"}) }

		t := c.Params("targetId")
		if u.(string) == t { return c.Status(400).JSON(fiber.Map{"error":"cannot follow self"}) }
		if err := h.svc.Follow(u.(string), t); err != nil {
			return c.Status(400).JSON(fiber.Map{"error": err.Error()})
		}
		return c.SendStatus(204)
	})

	app.Delete("/follow/:targetId", func(c *fiber.Ctx) error {
		u := c.Locals("userId")
		role := strings.ToLower(c.Locals("role").(string))
		if u == nil || u.(string) == "" { return c.Status(401).JSON(fiber.Map{"error":"unauthorized"}) }
		if role != "tourist" { return c.Status(403).JSON(fiber.Map{"error":"only Tourist can unfollow"}) }

		t := c.Params("targetId")
		if err := h.svc.Unfollow(u.(string), t); err != nil {
			return c.Status(400).JSON(fiber.Map{"error": err.Error()})
		}
		return c.SendStatus(204)
	})

	app.Get("/followers/:userId", func(c *fiber.Ctx) error {
		t := c.Params("userId")
		list, err := h.svc.Followers(t); if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	app.Get("/following/:userId", func(c *fiber.Ctx) error {
		u := c.Params("userId")
		list, err := h.svc.Following(u); if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	app.Get("/recommendations/:userId", func(c *fiber.Ctx) error {
		u := c.Params("userId")
		list, err := h.svc.Recommendations(u); if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(list)
	})

	app.Get("/is-following", func(c *fiber.Ctx) error {
		u, t := c.Query("u"), c.Query("t")
		if u=="" || t=="" { return c.Status(400).JSON(fiber.Map{"error":"u and t required"}) }
		ok, err := h.svc.IsFollowing(u, t); if err != nil { return c.Status(400).JSON(fiber.Map{"error": err.Error()}) }
		return c.JSON(fiber.Map{"following": ok})
	})
}
