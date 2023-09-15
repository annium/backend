install:
	xs remote restore -user $(user) -password $(pass)

update:
	xs update all dotnet -debug -sc -ic

build:
	dotnet build -v q

test:
	dotnet test -v q

publish:
	xs publish all 0.1.0 -p 1

.PHONY: $(MAKECMDGOALS)
