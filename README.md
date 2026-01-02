# Tour Explorer

Academic project for the Service Oriented Architectures course at Faculty of Technical Sciences, University of Novi Sad

## About Project

Tour Explorer is a microservices-based web application developed as an academic project focused on service-oriented and microservice architectures. The system supports three roles (Administrator, Guide, Tourist) and provides functionalities such as user and role management, blogging with comments and likes, user following, tour creation and execution, purchasing workflows, and geospatial features.

The application consists of multiple independent microservices (Stakeholders, Auth, Blog, Followers, Tour, Purchase, Gateway), each deployed separately with its own database and exposed via REST or gRPC APIs. The Followers service uses a graph NoSQL database for implementing social relationships, while other services rely on SQL or document-based NoSQL databases.

The system implements key architectural patterns and infrastructure features including API Gateway, Docker containerization, Docker Compose orchestration, inter-service communication, SAGA pattern for distributed transactions, rate limiting, logging, tracing, monitoring, and NoSQL integration.

## Used Tools

► Visual Studio <br>
► Visual Studio Code <br>
► Docker Desktop <br>

## Databases

► PostgreSQL <br>
► MongoDB <br>
► Neo4j

### Authors

► Ana Edelinski <br>
► Masa Mastilovic <br>
► Anja Vujacic <br>
► Ana Moraca
