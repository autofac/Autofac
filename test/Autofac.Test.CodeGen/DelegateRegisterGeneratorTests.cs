// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.CodeGen.Helpers;

namespace Autofac.Test.CodeGen;

[UsesVerify]
public class DelegateRegisterGeneratorTests
{
    [Fact]
    public Task VerifyGeneratedCode()
    {
       return CompilationVerifier.Verify(@"
namespace Autofac
{
    public static partial class RegistrationExtensions
    {
    }
}
");
    }
}
