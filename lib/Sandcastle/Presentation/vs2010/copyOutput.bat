if not exist Output mkdir Output
if not exist Output\html mkdir Output\html
if not exist Output\icons mkdir Output\icons
copy "%DXROOT%\Presentation\vs2010\icons\*" Output\icons
if not exist Intellisense mkdir Intellisense
