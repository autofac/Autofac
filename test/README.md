# Test Projects

- `Autofac.Test` is for unit tests that likely access internals to check on boundary conditions and so on. As versions of Autofac change and grow, these may also change and grow.
- `Autofac.Specification.Test` is for tests that access Autofac through the more standard public API - doing register and resolve calls, passing in parameters, that sort of thing. In general, if the "average user" wouldn't do it (e.g., follow the interfaces all the way down to count things like how many registration sources are there) or if it accesses internals, it shouldn't be there. This test suite is intended to stay pretty stable so we can ensure the 90% case is still working if we refactor internals and possibly make breaking changes.
