build_image:
	docker build -t devder/platform-service -f Dockerfile .

push_image: build_image
	docker push devder/platform-service

.PHONY: build_image push_image