@if {%1} == {prototype} goto CheckParam2
@if {%1} == {hana} goto CheckParam2
@if {%1} == {vs2005} goto CheckParam2
@if {%1} == {vs2010} goto CheckParam2
@echo please specify presentation style, it should be one of prototype/hana/vs2005/vs2010
goto End

:CheckParam2
@if {%2} == {} (
@echo please specify assembly name
goto End
)

:CheckParam3
@if {%3} == {} (
@echo please specify target type, it should be one of html/chm/hxs/mshc
goto End
)

@if not {%1} == {vs2010} goto CheckParam4
@if not {%3} == {hxs}  goto CheckParam4
@echo presentation style vs2010 and target type hxs are incompatible
goto End
:CheckParam4

REM ********** Set path for .NET Framework 4.0, Sandcastle, hhc, hxcomp****************************

setlocal
set PATH=%windir%\Microsoft.NET\Framework\v4.0.30319;%DXROOT%\ProductionTools;%PATH%
set TOOLSPATH=%ProgramFiles%
if exist "%ProgramFiles% (x86)" set TOOLSPATH=%ProgramFiles(x86)%
set PATH=%TOOLSPATH%\HTML Help Workshop;%TOOLSPATH%\Microsoft Help 2.0 SDK;%PATH%

if exist output rmdir output /s /q
if exist branding rmdir branding /s /q
if exist chm rmdir chm /s /q

REM ********** Compile source files ****************************

::csc /t:library /doc:comments.xml test.cs
::if there are more than one file, please use [ csc /t:library /doc:comments.xml *.cs ]

if exist %2.xml copy /y %2.xml comments.xml

REM ********** Call MRefBuilder ****************************

MRefBuilder %2.dll /out:reflection.org

REM ********** Apply Transforms ****************************

if {%1} == {prototype} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyPrototypeDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddGuidFilenames.xsl" /out:reflection.xml 
) else if {%1} == {hana} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=false /arg:IncludeInheritedOverloadTopics=false
) else (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=true /arg:IncludeInheritedOverloadTopics=false
)

XslTransform /xsl:"%DXROOT%\ProductionTransforms\ReflectionToManifest.xsl"  reflection.xml /out:manifest.xml

call "%DXROOT%\Presentation\%1\copyOutput.bat"
if {%1} == {vs2010} (
if {%3} == {mshc} (
call "%DXROOT%\Presentation\%1\copyBranding_mshc.bat"
) else (
call "%DXROOT%\Presentation\%1\copyBranding.bat"
))

REM **************Generate an intermediate Toc file that simulates the Whidbey TOC format.

if {%1} == {prototype} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createPrototypetoc.xsl" reflection.xml /out:toc.xml 
) else (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createvstoc.xsl" reflection.xml /out:toc.xml 
)

REM ********** Call BuildAssembler ****************************

if {%3} == {mshc} (
BuildAssembler /config:"%DXROOT%\Presentation\%1\configuration\sandcastle-mshc.config" manifest.xml
) else (
BuildAssembler /config:"%DXROOT%\Presentation\%1\configuration\sandcastle.config" manifest.xml
)

REM ************ Generate CHM help project ******************************

if {%3} == {chm} (
if not exist chm mkdir chm
if not exist chm\html mkdir chm\html

if exist output\icons\* xcopy output\icons\* chm\icons\ /y /r /i
if exist output\scripts\* xcopy output\scripts\* chm\scripts\ /y /r /i
if exist output\styles\* xcopy output\styles\* chm\styles\ /y /r /i
if exist output\branding\* xcopy output\branding\* chm\branding\ /y /r /i
REM if exist output\media\* xcopy output\media\* chm\media\ /y /r /i

ChmBuilder.exe /project:%2 /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\%2.hhp
)

REM ************ Generate HxS help project **************************************

if {%3} == {hxs} (
call "%DXROOT%\Presentation\shared\copyhavana.bat" %2

XslTransform /xsl:"%DXROOT%\ProductionTransforms\CreateHxc.xsl" toc.xml /arg:fileNamePrefix=%2 /out:Output\%2.HxC

XslTransform /xsl:"%DXROOT%\ProductionTransforms\TocToHxSContents.xsl" toc.xml /out:Output\%2.HxT

REM If you need to generate hxs, please uncomment the following line. Make sure "Microsoft Help 2.0 SDK" is installed on your machine.
REM hxcomp.exe -p output\%2.hxc
)

REM ************ Generate mshc help project **************************************

if {%3} == {mshc} (
MSHCPackager /save /r "output" "Output.mshc"
REM MSHCPackager /save /r "branding" "OutputBranding.mshc"
)

:End
