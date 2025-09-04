package config

import "os"

func GetEnv(k, d string) string {
	if v := os.Getenv(k); v != "" { return v }
	return d
}
