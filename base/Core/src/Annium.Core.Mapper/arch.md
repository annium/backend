1. Mapping configuration is singleton
Using scoped services will kill down all performance

2. Core base
As far as going without map repacking (makes no sense in terms of flexibility with custom injections from DI)
- provide second level lambda, that will inject mapper and context into resulting function call

3. Map configuration
Profile, as it was - is a set of maps.
Profile allows to create several IMapConfiguration
Built-in capabilities:
All field expressions - are starting with IFieldConfiguration:
- .Item() - down to item in IEnumerable field
- .When() - specify condition, when this map is applicable
- .With(Func) - direct field mapping
- .Ignore() - ignore field in mapping
- .Throw(Func<Expression>) - throw expression, if mapping occurs

4. Static mapper
It's configuration is internal and synced with instance mapper. 
Indeed, instance mapper is simply provided through static readonly property through Lazy

5. Generic configuration
Simply with .Generic() in IMapConfiguration