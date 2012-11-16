MSHCPackager /extract /r "%DXROOT%\Presentation\vs2010\branding\dev10.mshc" "output\branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\dev10.manifest" /arg:noTransforms 
MSHCPackager /extract /r "%DXROOT%\Presentation\vs2010\branding\dev10.mshc" "branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\dev10.manifest" /arg:onlyTransforms 
MSHCPackager /copy /r "output\branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\branding.manifest" /arg:noTransforms 
MSHCPackager /copy /r "branding" "/manifest:%DXROOT%\Presentation\vs2010\branding\branding.manifest" /arg:onlyTransforms 
XslTransform /xsl:"%DXROOT%\Presentation\vs2010\copyBranding.xsl" "%DXROOT%\Presentation\vs2010\branding\branding.xml" /out:branding\branding.xml /w /arg:catalogProductFamily=VS,catalogProductVersion=100,catalogLocale=en-US
