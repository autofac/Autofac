<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:branding="urn:FH-Branding"
>
	<xsl:output version ="1.0"
							encoding="utf-8"
							method="xml"
							indent="yes"
							omit-xml-declaration="no"/>

	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="catalogProductFamily"
						 select="'VS'"/>
	<xsl:param name="catalogProductVersion"
						 select="'100'"/>
	<xsl:param name="catalogLocale"
						 select="'en-US'"/>
	<xsl:param name="branding-package"
						 select="'Dev10.mshc'"/>
	<xsl:param name="branding-vendor"
						 select="'Microsoft'"/>

	<!-- ============================================================================================
	Transforms
	============================================================================================= -->

	<xsl:template match="branding:branding">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:if test="not(branding:common-parameters)">
				<xsl:element name="common-parameters"
										 namespace="urn:FH-Branding">
					<xsl:call-template name="t_applyParameters"/>
				</xsl:element>
			</xsl:if>
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="branding:common-parameters">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
			<xsl:call-template name="t_applyParameters"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<!-- ============================================================================================
	Parameter Usage
	============================================================================================= -->

	<xsl:template name="t_applyParameters">
		<xsl:element name="parameter"
								 namespace="urn:FH-Branding">
			<xsl:attribute name="name">catalogProductFamily</xsl:attribute>
			<xsl:attribute name="value">
				<xsl:value-of select="$catalogProductFamily"/>
			</xsl:attribute>
		</xsl:element>
		<xsl:text xml:space="preserve">
		</xsl:text>
		<xsl:element name="parameter"
								 namespace="urn:FH-Branding">
			<xsl:attribute name="name">catalogProductVersion</xsl:attribute>
			<xsl:attribute name="value">
				<xsl:value-of select="$catalogProductVersion"/>
			</xsl:attribute>
		</xsl:element>
		<xsl:text xml:space="preserve">
		</xsl:text>
		<xsl:element name="parameter"
								 namespace="urn:FH-Branding">
			<xsl:attribute name="name">catalogLocale</xsl:attribute>
			<xsl:attribute name="value">
				<xsl:value-of select="$catalogLocale"/>
			</xsl:attribute>
		</xsl:element>
		<xsl:text xml:space="preserve">
		</xsl:text>
		<xsl:element name="parameter"
								 namespace="urn:FH-Branding">
			<xsl:attribute name="name">branding-package</xsl:attribute>
			<xsl:attribute name="value">
				<xsl:value-of select="concat('content/',$branding-vendor,'/store/',$branding-package)"/>
			</xsl:attribute>
		</xsl:element>
		<xsl:text xml:space="preserve">
	</xsl:text>
	</xsl:template>

</xsl:stylesheet>
