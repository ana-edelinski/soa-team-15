package main

import (
	"context"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"

	"api-gateway/config"
	blogs "api-gateway/proto/blog"
	stk "api-gateway/proto/stakeholders"
	tours "api-gateway/proto/tours"

	"github.com/grpc-ecosystem/grpc-gateway/v2/runtime"
	"github.com/rs/cors"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {
	cfg := config.GetConfig()

	// 1. konekcija prema Stakeholders servisu
	log.Println("Dialing gRPC server at", cfg.StakeholdersServiceAddress)
	stkConn, err := grpc.DialContext(
		context.Background(),
		cfg.StakeholdersServiceAddress,
		grpc.WithBlock(),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
	)
	if err != nil {
		log.Fatalln("Failed to dial StakeholdersService:", err)
	}
	defer stkConn.Close()
	log.Println("Connected to StakeholdersService at", cfg.StakeholdersServiceAddress)

	// 2. konekcija prema Tours servisu
	log.Println("Dialing gRPC server at", cfg.ToursServiceAddress)
	toursConn, err := grpc.DialContext(
		context.Background(),
		cfg.ToursServiceAddress,
		grpc.WithBlock(),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
	)
	if err != nil {
		log.Fatalln("Failed to dial ToursService:", err)
	}
	defer toursConn.Close()
	log.Println("Connected to ToursService at", cfg.ToursServiceAddress)

	// 3. konekcija prema Blog servisu
	log.Println("Dialing gRPC server at", cfg.BlogServiceAddress)
	blogsConn, err := grpc.DialContext(
		context.Background(),
		cfg.BlogServiceAddress,
		grpc.WithBlock(),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
	)
	if err != nil {
		log.Fatalln("Failed to dial BlogService:", err)
	}
	defer blogsConn.Close()
	log.Println("Connected to BlogService at", cfg.BlogServiceAddress)

	// gRPC-Gateway mux
	gwmux := runtime.NewServeMux()

	// Register StakeholdersService
	stkClient := stk.NewStakeholdersServiceClient(stkConn)
	err = stk.RegisterStakeholdersServiceHandlerClient(
		context.Background(),
		gwmux,
		stkClient,
	)
	if err != nil {
		log.Fatalln("Failed to register StakeholdersService handler:", err)
	}

	// Register ToursService
	toursClient := tours.NewToursServiceClient(toursConn)
	err = tours.RegisterToursServiceHandlerClient(
		context.Background(),
		gwmux,
		toursClient,
	)
	if err != nil {
		log.Fatalln("Failed to register ToursService handler:", err)
	}

	// Register PositionsService
	positionsClient := tours.NewPositionsServiceClient(toursConn)
	err = tours.RegisterPositionsServiceHandlerClient(
		context.Background(),
		gwmux,
		positionsClient,
	)
	if err != nil {
		log.Fatalln("Failed to register PositionsService handler:", err)
	}

	// Register BlogService
	blogsClient := blogs.NewBlogServiceClient(blogsConn)
	err = blogs.RegisterBlogServiceHandlerClient(
		context.Background(),
		gwmux,
		blogsClient,
	)
	if err != nil {
		log.Fatalln("Failed to register BlogService handler:", err)
	}

	// CORS
	corsHandler := cors.New(cors.Options{
		AllowedOrigins:   []string{"http://localhost:4200"}, // Angular frontend
		AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowedHeaders:   []string{"*"},
		AllowCredentials: true,
	}).Handler(gwmux)

	// HTTP server koji koristi CORS
	gwServer := &http.Server{
		Addr:    cfg.Address,
		Handler: corsHandler,
	}

	go func() {
		log.Printf("HTTP Gateway listening on %s\n", cfg.Address)
		if err := gwServer.ListenAndServe(); err != nil {
			log.Fatal("server error: ", err)
		}
	}()

	// graceful shutdown
	stopCh := make(chan os.Signal, 1)
	signal.Notify(stopCh, syscall.SIGINT, syscall.SIGTERM)

	<-stopCh
	log.Println("Shutting down gateway...")

	if err = gwServer.Close(); err != nil {
		log.Fatalln("error while stopping server: ", err)
	}
}
