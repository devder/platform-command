build_image:
	docker build --platform=linux/amd64 -t devder/command-service:latest -f Dockerfile .

push_image: build_image
	docker push devder/command-service:latest

run_image:
	docker run --name command-service -p 8080:8080 devder/command-service:latest

.PHONY: build_image push_image run_image