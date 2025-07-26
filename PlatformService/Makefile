build_image:
	docker build --platform=linux/amd64 -t devder/platform-service:latest -f Dockerfile .

push_image: build_image
	docker push devder/platform-service:latest

run_image:
	docker run --name platform-service -p 8080:8080 devder/platform-service:latest

gen_secret:
	kubectl create secret generic mssql --from-literal=SA_PASSWORD=$(password)

new_migration:
	dotnet ef migrations add $(name) --output-dir Data/Migrations

migrateup:
	dotnet ef database update

restart_deployment:
	kubectl rollout restart deployment platforms-deployment

.PHONY: build_image push_image run_image new_migration migrateup restart_deployment