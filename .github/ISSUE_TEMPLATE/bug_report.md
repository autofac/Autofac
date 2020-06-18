---
name: Bug Report
about: Create a report to help us fix a problem in core Autofac.
title: ''
labels: ''
assignees: ''
---

<!--
  This is for CORE AUTOFAC ONLY. If you are having trouble with an integration library
  like Autofac.Integration.Mvc, Autofac.Multitenant, or Autofac.Extensions.DependencyInjection
  please file in the appropriate repo!
-->

## Describe the Bug

<!-- A clear and concise description of what the bug is. -->

## Steps to Reproduce

<!-- Tell us how to reproduce the issue. Ideally provide a failing unit test. -->

```c#
public class ReproTest
{
  [Fact]
  public void Repro()
  {
    var builder = new ContainerBuilder();
    var container = builder.Build();
    Assert.NotNull(container);
  }
}
```

## Expected Behavior

<!-- Describe what you expected to happen. -->

## Exception with Stack Trace

<!-- If you see an exception, put the WHOLE THING here. -->

```text
Put the exception with stack trace here.
```

## Dependency Versions

Autofac: <!-- Fill in the version of Autofac you're using -->
<!-- What other dependencies are you using? Names and versions. -->

## Additional Info

<!-- Add any other context about the problem here. -->
