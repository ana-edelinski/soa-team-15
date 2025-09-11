FROM golang:1.21-alpine AS builder

WORKDIR /app

COPY go.mod .

RUN go mod download all

COPY . .

RUN CGO_ENABLED=0 GOOS=linux go build -o blog-service .

FROM alpine:latest

WORKDIR /app

COPY --from=builder /app/blog-service .

EXPOSE 8080

CMD ["./blog-service"]