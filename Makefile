.PHONY: build test coverage format clean docker-up docker-down lint restore publish docker-build help

# --- Configuration ---
SOLUTION    := CognitiveMesh.sln
CONFIG      := Release
COVERAGE_DIR := TestResults/coverage

# --- Build ---
build: ## Build the solution
	dotnet build $(SOLUTION) -c $(CONFIG)

restore: ## Restore NuGet packages
	dotnet restore $(SOLUTION)

publish: ## Publish the runtime project
	dotnet publish src/MeshSimRuntime/MeshSimRuntime.csproj -c $(CONFIG) -o out/publish

# --- Test ---
test: ## Run all tests
	dotnet test $(SOLUTION) --no-build -c $(CONFIG)

coverage: ## Run tests with code coverage (opencover format)
	dotnet test $(SOLUTION) -c $(CONFIG) \
		--collect:"XPlat Code Coverage;Format=opencover" \
		--results-directory $(COVERAGE_DIR)
	@echo "Coverage reports written to $(COVERAGE_DIR)"

# --- Code Quality ---
format: ## Format code using dotnet format
	dotnet format $(SOLUTION) --verbosity normal

lint: ## Run dotnet format in verify mode (CI-friendly)
	dotnet format $(SOLUTION) --verify-no-changes --verbosity normal

# --- Docker ---
docker-up: ## Start local dev dependencies (Redis, Qdrant, Azurite)
	docker compose up -d

docker-down: ## Stop local dev dependencies
	docker compose down

docker-build: ## Build the application Docker image
	docker build -t cognitive-mesh:local .

# --- Cleanup ---
clean: ## Remove build artifacts and test results
	dotnet clean $(SOLUTION) -c $(CONFIG)
	rm -rf out TestResults
	rm -rf src/*/bin src/*/obj
	rm -rf tests/*/bin tests/*/obj

# --- Help ---
help: ## Show this help message
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'

.DEFAULT_GOAL := help
