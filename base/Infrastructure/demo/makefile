mbus-echo:
	cd Demo.Infrastructure.MessageBus.EchoServer && dotnet run

mbus-proxy:
	docker-compose -f Demo.Infrastructure.MessageBus.EchoServer/docker-compose.yml up proxy

mbus-sink:
	docker-compose -f Demo.Infrastructure.MessageBus.EchoServer/docker-compose.yml up sink

mbus-down:
	docker-compose -f Demo.Infrastructure.MessageBus.EchoServer/docker-compose.yml down

ws-start-binary:
	dotnet run -p Demo.Infrastructure.WebSockets.Server/Demo.Infrastructure.WebSockets.Server.csproj &

ws-start-text:
	dotnet run -p Demo.Infrastructure.WebSockets.Server/Demo.Infrastructure.WebSockets.Server.csproj --use-text &

ws-stop:
	$(call kill,Demo.Infrastructure.WebSockets.Server.csproj)

ws-drop:
	$(call kill,net5.0/Demo.Infrastructure.WebSockets.Client,-9)

ws-listen:
	$(call cli,listen)

ws-request-void:
	$(call cli,request-void)

ws-request-response:
	$(call cli,request-response)

ws-sub-unsub:
	$(call cli,sub-unsub)

define cli
	@$(eval command := $(1))
	dotnet run -p Demo.Infrastructure.WebSockets.Client/Demo.Infrastructure.WebSockets.Client.csproj $(command) -s ws://localhost:5000/ws
endef

define kill
	@$(eval search := $(1))
	@$(eval killArgs := $(2))
	ps -ax | grep $(search) | grep -v src | grep -v grep | sed -e 's#^ *##' | cut -d ' ' -f 1 | xargs -I% kill $(killArgs) %
endef

.PHONY: $(MAKECMDGOALS)