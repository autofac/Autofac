<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl branding"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:branding="urn:FH-Branding"
								xmlns:xs="http://www.w3.org/2001/XMLSchema">

	<xsl:import href="body.xslt"/>

	<xsl:template match="xhtml:body" name="ps-body">
		<xsl:copy>
			<xsl:attribute name="onload">onLoad()</xsl:attribute>
			<xsl:if test="not($pre-branding)">
				<xsl:attribute name="class">OH_body</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="@*"/>

			<xsl:element name="div" namespace="{$xhtml}">
				<xsl:attribute name="class">OH_outerDiv</xsl:attribute>
				<xsl:element name="div" namespace="{$xhtml}">
					<xsl:attribute name="class">OH_outerContent</xsl:attribute>
					<xsl:apply-templates select="node()"/>
				</xsl:element>
			</xsl:element>
			<xsl:call-template name="footer"/>
		</xsl:copy>
	</xsl:template>

	<!-- ============================================================================================
	Generic transforms (see Identity.xslt for others)
	============================================================================================= -->

	<!-- strip style attributes by default -->
	<xsl:template match="@style[translate(.,' ;','')!='display:none' and translate(.,' ;','')!='display:inline']"
								name="ps-style"/>

	<!-- pass through styles for p and h elements -->
	<xsl:template match="//xhtml:p[@style]|xhtml:h1[@style]|xhtml:h2[@style]|xhtml:h3[@style]|xhtml:h4[@style]|xhtml:h5[@style]|xhtml:h6[@style]"
								name="ps-allow-styles">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:attribute name="style">
				<xsl:value-of select="@style"/>
			</xsl:attribute>
			<xsl:apply-templates/>
		</xsl:copy>
	</xsl:template>

	<!-- ============================================================================================
	Specific transforms
	============================================================================================= -->

	<!-- redirect image sources to the appropriate path -->
	<xsl:template match="xhtml:img" name="ps-img">
		<xsl:copy>
			<xsl:choose>
				<xsl:when test="not($pre-branding)">
					<xsl:for-each select="@*">
						<xsl:choose>
							<xsl:when test="name()='src'">
								<xsl:attribute name="src">
									<xsl:choose>
										<xsl:when test="$downscale-browser">
											<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/',.))"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="branding:BuildContentPath($contentFolder,.)"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="."/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:for-each>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="@*"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
