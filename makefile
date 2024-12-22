setup:
	xx remote restore -user $(user) -password $(pass)
	dotnet tool restore

format:
	xx format -sc -ic
	dotnet csharpier .

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	xx update all -sc -ic

clean:
	xx clean -sc -ic

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat .xx.credentials)
	find . -type f -name '*.nupkg' | xargs rm

.PHONY: $(MAKECMDGOALS)
