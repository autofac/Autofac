<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:branding="urn:FH-Branding"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
>
	<xsl:import href="LanguageSpecificText.xslt"/>

	<!-- Convert old-style LST to new style -->
	<xsl:template match="xhtml:span[@class='languageSpecificText']"
								name="old-lst">
		<xsl:choose>
			<xsl:when test="count(xhtml:span[@class]) = count(*)">
				<xsl:variable name="v_id"
											select="generate-id(.)"/>

				<xsl:element name="span"
										 namespace="{$xhtml}">
					<xsl:attribute name="id">
						<xsl:value-of select="$v_id"/>
					</xsl:attribute>
					<xsl:text>&#160;</xsl:text>
				</xsl:element>
				<xsl:element name="script"
										 namespace="{$xhtml}">
					<xsl:attribute name="type">
						<xsl:value-of select="'text/javascript'"/>
					</xsl:attribute>
					addToLanSpecTextIdSet("<xsl:value-of select="$v_id"/>?<xsl:value-of select ="'vb='"/><xsl:if test="xhtml:span[@class='vb']">
						<xsl:value-of select ="xhtml:span[@class='vb']"/>
					</xsl:if><xsl:value-of select ="'|cpp='"/><xsl:if test="xhtml:span[@class='cpp']">
						<xsl:value-of select ="xhtml:span[@class='cpp']"/>
					</xsl:if><xsl:value-of select ="'|cs='"/><xsl:if test="xhtml:span[@class='cs']">
						<xsl:value-of select ="xhtml:span[@class='cs']"/>
					</xsl:if><xsl:value-of select ="'|fs='"/><xsl:if test="xhtml:span[@class='fs']">
						<xsl:value-of select ="xhtml:span[@class='fs']"/>
					</xsl:if><xsl:value-of select ="'|nu='"/><xsl:if test="xhtml:span[@class='nu']">
						<xsl:value-of select ="xhtml:span[@class='nu']"/>
					</xsl:if><xsl:for-each select="xhtml:span[@class!='vb' and @class!='cpp' and @class!='cs' and @class!='fs' and @class!='nu']">
						<xsl:value-of select ="concat('|',@class,'=')"/>
						<xsl:value-of select ="."/>
					</xsl:for-each>");
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="."/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
