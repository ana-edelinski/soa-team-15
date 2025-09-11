package handler

import (
	"strings"

	"github.com/gofiber/fiber/v2"
	"followers-service/config"
)

// Validira JWT iz Stakeholders i upisuje userId/role u ctx.
func JwtAuth(c *fiber.Ctx) error {
	auth := c.Get("Authorization")
	claims, err := config.ParseAndValidateToken(auth)
	if err != nil {
		// za POST/DELETE je obavezno, za GET dozvoljavamo i bez tokena
		if c.Method() == fiber.MethodPost || c.Method() == fiber.MethodDelete {
			return c.Status(401).JSON(fiber.Map{"error": "unauthorized: " + err.Error()})
		}
		return c.Next()
	}
	role := claims.Role
	if i := strings.LastIndex(role, "/"); i >= 0 && i+1 < len(role) { role = role[i+1:] }

	c.Locals("userId", claims.ID)
	c.Locals("role", role)
	return c.Next()
}
