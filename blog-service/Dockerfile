FROM golang:1.21-alpine AS deps
WORKDIR /src
RUN apk add --no-cache git ca-certificates
COPY go.mod .
RUN go mod download

FROM golang:1.21-alpine AS builder
WORKDIR /app
RUN apk add --no-cache ca-certificates
COPY --from=deps /src/go.mod .
COPY --from=deps /src/go.sum .
COPY . .
RUN go mod tidy
RUN CGO_ENABLED=0 GOOS=linux go build -o blog-service .

FROM alpine:3.20
WORKDIR /app
RUN adduser -D app && mkdir -p /app/uploads && chown -R app:app /app
COPY --from=builder /app/blog-service .
USER app
EXPOSE 8080
ENV UPLOAD_DIR=/app/uploads
CMD ["./blog-service"]
