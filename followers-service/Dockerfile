# ---------- build ----------
FROM golang:1.25-alpine AS builder
WORKDIR /app
ENV GOTOOLCHAIN=auto

COPY go.mod go.sum ./
RUN go mod download

COPY . .
# main.go je u rootu followers-service
RUN CGO_ENABLED=0 GOOS=linux go build -o followers-service .

# ---------- final ----------
FROM gcr.io/distroless/static-debian12 AS final
WORKDIR /
COPY --from=builder /app/followers-service /followers-service

EXPOSE 8085

# Env se poklapa sa config/neo4j.go (NEO4J_PASSWORD!)
ENV NEO4J_URI="bolt://neo4j-db:7687" \
    NEO4J_USER="neo4j" \
    NEO4J_PASSWORD="neo4jpass" \
    CORS_ORIGIN="http://localhost:4200"

USER nonroot:nonroot
ENTRYPOINT ["/followers-service"]