@ECHO OFF
CLS

IF "%1%"=="H2" GOTO H2Viewer

REM This is an example script to show how to use the Help Library Manager
REM Launcher to remove an MS Help Viewer file.  You can use this as an example
REM for creating a script to run from your product's uninstaller.

REM NOTE: If not executed from within the same folder as the executable, a
REM full path is required on the executable.

HelpLibraryManagerLauncher.exe /product "{@CatalogProductId}" /version "{@CatalogVersion}" /locale {@Locale} /uninstall /silent /vendor "{@VendorName}" /mediaBookList "{@HelpTitle}" /productName "{@ProductTitle}"

GOTO Exit

:H2Viewer

REM The Help Library Manager Launcher tool does not support MS Help Viewer 2 yet so this calls the tool directly
REM for temporary support.

"%SYSTEMDRIVE%\Program Files\Microsoft Help Viewer\v2.0\HlpCtntMgr.exe" /operation uninstall /catalogName VisualStudio11 /locale {@Locale} /vendor "{@VendorName}"  /productName "{@HtmlEncProductTitle}" /bookList "{@HtmlEncHelpTitle}"

:Exit
