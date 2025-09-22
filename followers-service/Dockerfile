# ---------- build ----------
FROM golang:1.25-alpine AS builder
WORKDIR /app
ENV CGO_ENABLED=0

COPY go.mod go.sum ./
RUN go mod download

COPY . .
# main.go ti je u rootu followers-service
RUN go build -o /followers-service ./main.go

# ---------- final ----------
FROM gcr.io/distroless/static-debian12 AS final
WORKDIR /
COPY --from=builder /followers-service /followers-service

EXPOSE 8085
ENV PORT="8085" \
    CORS_ORIGIN="http://localhost:4200" \
    NEO4J_URI="bolt://neo4j-db:7687" \
    NEO4J_USER="neo4j" \
    NEO4J_PASSWORD="neo4jpass"

USER nonroot:nonroot
ENTRYPOINT ["/followers-service"]
