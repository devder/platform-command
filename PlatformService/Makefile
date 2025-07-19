build_image:
	docker build -t devder/platform-service:latest -f Dockerfile .

push_image: build_image
	docker push devder/platform-service:latest

run_image:
	docker run --name platform-service -p 8080:8080 devder/platform-service:latest

.PHONY: build_image push_image run_image