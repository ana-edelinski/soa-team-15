/*package config

import (
	"errors"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

type AuthClaims struct {
	ID       string `json:"id"`
	Username string `json:"username"`
	PersonID string `json:"personId"`
	Role     string `json:"role"` // normalizovaćemo ispod
	jwt.RegisteredClaims
}

func jwtKey() []byte     { return []byte(GetEnv("JWT_KEY", "explorer_super_secret_key_that_is_long_enough")) }
func jwtIssuer() string  { return GetEnv("JWT_ISSUER", "explorer") }
func jwtAudience() string{ return GetEnv("JWT_AUDIENCE", "explorer-front.com") }

func ParseAndValidateToken(bearer string) (*AuthClaims, error) {
	if bearer == "" { return nil, errors.New("missing Authorization header") }
	parts := strings.SplitN(bearer, " ", 2)
	if len(parts) != 2 || !strings.EqualFold(parts[0], "Bearer") {
		return nil, errors.New("invalid Authorization header format")
	}
	tokenStr := parts[1]

	claims := &AuthClaims{}
	token, err := jwt.ParseWithClaims(tokenStr, claims, func(token *jwt.Token) (any, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, errors.New("unexpected signing method")
		}
		return jwtKey(), nil
	}, jwt.WithAudience(jwtAudience()), jwt.WithIssuer(jwtIssuer()))
	if err != nil { return nil, err }
	if !token.Valid { return nil, errors.New("invalid token") }

	// normalizuj role (nekad stigne kao pun URL claim)
	if claims.Role == "" {
		if m, ok := token.Claims.(jwt.MapClaims); ok {
			if v, ok := m["role"].(string); ok { claims.Role = v }
			if v, ok := m["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"].(string); ok && claims.Role == "" {
				claims.Role = v
			}
		}
	}
	return claims, nil
}
*/

package config

import (
	"errors"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

type Claims struct {
	ID   string `json:"id"`
	Role string `json:"role"`
	jwt.RegisteredClaims
}

// ParseAndValidateToken: DEV varijanta bez verifikacije potpisa.
// U produkciji OBAVEZNO verifikuj potpis!
func ParseAndValidateToken(authHeader string) (*Claims, error) {
	if authHeader == "" {
		return nil, errors.New("missing Authorization header")
	}
	parts := strings.SplitN(authHeader, " ", 2)
	if len(parts) != 2 || !strings.EqualFold(parts[0], "Bearer") {
		return nil, errors.New("invalid Authorization format")
	}

	tok, _, err := new(jwt.Parser).ParseUnverified(parts[1], &Claims{})
	if err != nil {
		return nil, err
	}
	cl, ok := tok.Claims.(*Claims)
	if !ok {
		return nil, errors.New("invalid claims")
	}
	if cl.Role == "" {
		cl.Role = "tourist" // default za lokalno
	}
	return cl, nil
}
