start-seq:
	docker run \
		--rm \
		--name seq \
		-d \
		-e ACCEPT_EULA=Y \
		-p 5341:80 \
		datalust/seq:latest

stop-seq:
	docker stop seq

.PHONY: $(MAKECMDGOALS)