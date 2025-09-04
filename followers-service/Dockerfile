FROM golang:1.22 AS build
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN CGO_ENABLED=0 go build -o followers-service

FROM gcr.io/distroless/base-debian12
WORKDIR /app
COPY --from=build /app/followers-service .
# ENV za Neo4j 
ENV NEO4J_URI=bolt://neo4j:7687 NEO4J_USER=neo4j NEO4J_PASSWORD=neo4jpass
# ENV za JWT – moraju se poklopiti sa Stakeholders
ENV JWT_KEY=explorer_super_secret_key_that_is_long_enough
ENV JWT_ISSUER=explorer
ENV JWT_AUDIENCE=explorer-front.com
EXPOSE 8085
ENTRYPOINT ["/app/followers-service"]
