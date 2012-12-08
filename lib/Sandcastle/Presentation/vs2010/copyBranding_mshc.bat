MSHCPackager /extract /r "%DXROOT%\Presentation\vs2010\branding\dev10.mshc" "branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\dev10.manifest" /arg:noIcons 
MSHCPackager /extract /r "%DXROOT%\Presentation\vs2010\branding\dev10.mshc" "output\icons" "/manifest:%DXROOT%\Presentation\vs2010\branding\dev10.manifest" /arg:onlyIcons 
MSHCPackager /copy /r "branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\branding.manifest" /arg:noIcons 
MSHCPackager /copy /r "output\icons" "/manifest:%DXROOT%\Presentation\vs2010\branding\branding.manifest" /arg:onlyIcons 
XslTransform /xsl:"%DXROOT%\Presentation\vs2010\copyBranding.xsl" "%DXROOT%\Presentation\vs2010\branding\branding.xml" /out:branding\branding.xml /w /arg:catalogProductFamily=VS,catalogProductVersion=100,catalogLocale=en-US
