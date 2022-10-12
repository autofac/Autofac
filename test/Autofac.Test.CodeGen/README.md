# Autofac.Test.CodeGen

This test project uses the [Verify](https://github.com/VerifyTests/Verify) library to perform snapshot testing of the output of our `Autofac.CodeGen` source generator.

Each time the tests run, the set of generated files will be compared with the committed-to-source-control `*.verified.cs` files in the `Snapshots` folder.

If any of the generated files differ from the saved `verified` file of the same name, the test will fail.

When this happens a file will appear in the `Snapshots` folder with the `*.received.cs` suffix with the new file content. In Visual Studio, your IDE will also open a diff window between the two versions.

> The .gitignore in the Snapshots folder exludes the received files from being saved in git, we don't want to store those.

If you make changes to the generator, check that the outputted file is correct, and once you've tested the generated code, you should "accept" the new version by:

- Deleting the existing `verified` file.
- Renaming the new `received` file to be the `verified` one.

That will then cause the tests to pass, and the new verified files should be committed to source control.
