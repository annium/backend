start-binary:
	dotnet run -p Demo.Infrastructure.WebSockets.Server/Demo.Infrastructure.WebSockets.Server.csproj &

start-text:
	dotnet run -p Demo.Infrastructure.WebSockets.Server/Demo.Infrastructure.WebSockets.Server.csproj --use-text &

stop:
	$(call kill,Demo.Infrastructure.WebSockets.Server.csproj)

drop:
	$(call kill,net5.0/Demo.Infrastructure.WebSockets.Client,-9)

listen:
	$(call cli,listen)

request-void:
	$(call cli,request-void)

request-response:
	$(call cli,request-response)

sub-unsub:
	$(call cli,sub-unsub)

demo-request:
	$(call cli,demo request)

demo-sink:
	$(call cli,demo sink)

demo-echo:
	$(call cli,demo echo)

demo-echo-kill:
	$(call cli,demo echo -k)

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