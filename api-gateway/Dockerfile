# build stage
FROM golang:1.24 AS builder
WORKDIR /app

# Ako koristiš Go toolchain directive u go.mod, dozvoli autoupgrade
ENV GOTOOLCHAIN=auto

COPY go.mod go.sum ./
RUN go mod download

COPY . .
# (opciono: ubrzaj build bez VCS inf.)
# ENV GOFLAGS=-buildvcs=false
RUN CGO_ENABLED=0 GOOS=linux go build -o gateway .

# final stage (imenovan)
FROM gcr.io/distroless/base-debian12 AS final
WORKDIR /
COPY --from=builder /app/gateway .
EXPOSE 8090
CMD ["/gateway"]
