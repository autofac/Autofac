REM ********** Set path for .NET Framework 4.0, Sandcastle, hhc, hxcomp ****************************

setlocal
set PATH=%windir%\Microsoft.NET\Framework\v4.0.30319;%DXROOT%\ProductionTools;%ProgramFiles%\HTML Help Workshop;%PATH%

if exist output rmdir output /s /q
if exist branding rmdir branding /s /q
if exist chm rmdir chm /s /q

XslTransform /xsl:"%DXROOT%\ProductionTransforms\dsmanifesttomanifest.xsl" aspnet_howto.buildmanifest.proj.xml /out:manifest.xml
XslTransform /xsl:"%DXROOT%\ProductionTransforms\dstoctotoc.xsl" extractedfiles\aspnet_howto.toc.xml /out:toc.xml

call "%DXROOT%\Presentation\vs2010\copyOutput.bat"
call "%DXROOT%\Presentation\vs2010\copyBranding.bat"

BuildAssembler /config:conceptual-2010.config manifest.xml

if not exist chm mkdir chm
if not exist chm\html mkdir chm\html
if not exist chm\icons mkdir chm\icons

xcopy output\icons\* chm\icons\ /y /r
xcopy output\branding\* chm\branding\ /y /r

ChmBuilder.exe /project:test /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\test.hhp
