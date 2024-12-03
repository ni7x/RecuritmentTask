Simple ToDo API Demo

## Running the Application with Docker

### Prerequisites

- Docker
- Docker Compose
  
First, navigate to the `/docker/` directory where your `docker-compose.yml` file is located, then run docker-compose.

```bash
cd /docker/
docker-compose up -d
```
Once the containers are up and running, you can access your application by navigating to http://localhost:8080


## Running the Application Without Docker
1. Clone the Repository
Clone the repository to your local machine:

```bash
git clone <repository-url>
cd .\RecuritmentTask\
```

2. Restore project dependencies:
```bash
 dotnet restore
```

3. Run tests
```bash
dotnet test .\RecuritmentTask\test\RecuritmentTaskTests\
```
/
```bash
dotnet test
```
4. Run App
```bash
dotnet run --project .\RecuritmentTask\src\RecuritmentTask\RecuritmentTask.csproj
```
