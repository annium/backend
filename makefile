setup:
	dotnet tool restore

format:
	dotnet tool run csharpier format .
	dotnet tool run xs format -sc -ic

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	dotnet tool run xs update all -sc -ic

clean:
	dotnet tool run xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs rm

buildNumber?=0
build:
	dotnet build -c Release --nologo -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(apiKey)
	find . -type f -name '*.nupkg' | xargs rm

.PHONY: $(MAKECMDGOALS)
