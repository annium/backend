setup:
	dotnet tool restore

format:
	dotnet csharpier format .
	xs format -sc -ic

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs rm

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat .xs.credentials)
	find . -type f -name '*.nupkg' | xargs rm

.PHONY: $(MAKECMDGOALS)
