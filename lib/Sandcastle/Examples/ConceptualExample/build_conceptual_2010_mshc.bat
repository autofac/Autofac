REM ********** Set path for .NET Framework 4.0, Sandcastle ****************************

setlocal
set PATH=%windir%\Microsoft.NET\Framework\v4.0.30319;%DXROOT%\ProductionTools;%PATH%

if exist output rmdir output /s /q
if exist branding rmdir branding /s /q
if exist Output.mshc del Output.mshc
if exist OutputBranding.mshc del OutputBranding.mshc

XslTransform /xsl:"%DXROOT%\ProductionTransforms\dsmanifesttomanifest.xsl" aspnet_howto.buildmanifest.proj.xml /out:manifest.xml
XslTransform /xsl:"%DXROOT%\ProductionTransforms\dstoctotoc.xsl" extractedfiles\aspnet_howto.toc.xml /out:toc.xml

call "%DXROOT%\Presentation\vs2010\copyOutput.bat"
call "%DXROOT%\Presentation\vs2010\copyBranding_mshc.bat"

BuildAssembler /config:conceptual-2010-mshc.config manifest.xml

MSHCPackager /save /r "output" "Output.mshc"
@rem Create a branding package only if not using the default catalog. Remember to update HelpContentSetup.msha accordingly.
@rem MSHCPackager /save /r "branding" "OutputBranding.mshc"
