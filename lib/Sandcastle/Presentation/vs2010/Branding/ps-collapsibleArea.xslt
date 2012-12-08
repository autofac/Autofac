<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:branding="urn:FH-Branding"
								xmlns:xs="http://www.w3.org/2001/XMLSchema"
>

	<xsl:template match="mtps:CollapsibleArea"
								name="ps-collapsible-area">
		<xsl:element name="div"
								 namespace="{$xhtml}">
			<xsl:attribute name="class">OH_CollapsibleAreaRegion</xsl:attribute>
			<xsl:element name="div"
									 namespace="{$xhtml}">
				<xsl:attribute name="class">OH_regiontitle</xsl:attribute>
				<!-- Fix to allow section titles to contain HTML codes -->
				<xsl:choose>
					<xsl:when test="xhtml:xml/xhtml:string[@id='Title']">
						<xsl:apply-templates select="xhtml:xml/xhtml:string[@id='Title']/child::node()"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@Title"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="div"
									 namespace="{$xhtml}">
				<xsl:attribute name="class">OH_CollapsibleArea_HrDiv</xsl:attribute>
				<xsl:element name="hr"
										 namespace="{$xhtml}">
					<xsl:attribute name="class">OH_CollapsibleArea_Hr</xsl:attribute>
				</xsl:element>
				<xsl:value-of select="''"/>
			</xsl:element>
		</xsl:element>
		<xsl:element name="div"
								 namespace="{$xhtml}">
			<xsl:attribute name="class">OH_clear</xsl:attribute>
			<xsl:text> </xsl:text>
		</xsl:element>
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<!-- Fix to allow section titles to contain HTML codes -->
	<xsl:template match="mtps:CollapsibleArea/xhtml:xml[xhtml:string[@id='Title']]"/>

</xsl:stylesheet>
