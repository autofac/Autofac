REM ********** Set path for .NET Framework 4.0, Sandcastle ****************************

setlocal
set PATH=%windir%\Microsoft.NET\Framework\v4.0.30319;%DXROOT%\ProductionTools;%PATH%

if exist output rmdir output /s /q

XslTransform /xsl:"%DXROOT%\ProductionTransforms\dsmanifesttomanifest.xsl" aspnet_howto.buildmanifest.proj.xml /out:manifest.xml
XslTransform /xsl:"%DXROOT%\ProductionTransforms\dstoctotoc.xsl" extractedfiles\aspnet_howto.toc.xml /out:toc.xml

call "%DXROOT%\Presentation\vs2005\copyOutput.bat"

BuildAssembler /config:conceptual-2005-mshc.config manifest.xml
