<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

    <xsl:output method="xml" omit-xml-declaration="yes" encoding="iso-8859-1" />

    <xsl:param name="platform" select="string('.NETFramework')" />
    <xsl:param name="version" select="string('4.0.30319')" />

    <xsl:template match="Frameworks">
      <xsl:for-each select="//Framework[@Platform=$platform and @Version=$version]/AssemblyLocations/Location/AssemblyDetails">
        <xsl:value-of select="concat(ancestor::Location/@Path, '\', @Filename)" />
        <xsl:text>&#13;</xsl:text>
      </xsl:for-each>
    </xsl:template>

</xsl:stylesheet>
