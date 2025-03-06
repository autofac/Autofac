// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test;

public class ComObjectRegistrationTest
{
    private const string WindowsScriptingFileSystemObjectProgramId = "Scripting.FileSystemObject";

    [Fact]
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "File is excluded from non-Windows platforms.")]
    public void AsTest()
    {
        var fsoType = Type.GetTypeFromProgID(WindowsScriptingFileSystemObjectProgramId);
        var fso = Activator.CreateInstance(fsoType);

        var builder = new ContainerBuilder();
        builder.RegisterInstance(fso).As<string>();
        _ = builder.Build();
    }
}
