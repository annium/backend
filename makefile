format:
	xs format -sc -ic

install:
	xs remote restore -user $(user) -password $(pass)

update:
	xs update all -debug -sc -ic

clean:
	xs clean -sc -ic

build:
	dotnet build --nologo -v q

test:
	dotnet test --nologo -v q

publish:
	xs publish all 0.1.0 -p 1

.PHONY: $(MAKECMDGOALS)
