release:
	rm -rf build
	dotnet publish --self-contained -r osx-x64 -o build ../Annium.AspNetCore.TestServer

gen-client: release
	xws gen \
		-a build/Annium.AspNetCore.TestServer.dll \
		-o WebSocketClient \
		-ns Annium.AspNetCore.IntegrationTesting.WebSocketClient \
		-n TestServer

.PHONY: $(MAKECMDGOALS)