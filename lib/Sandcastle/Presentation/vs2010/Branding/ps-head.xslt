<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl branding"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:branding="urn:FH-Branding"
								xmlns:xs="http://www.w3.org/2001/XMLSchema"
								xmlns:cs="urn:Get-Paths"

>
	<xsl:import href="head.xslt"/>

	<xsl:template match="xhtml:head"
								name="ps-head">
		<xsl:copy>
			<xsl:call-template name="head-favicon"/>

			<!-- NOTE: These MUST appear INLINE and BEFORE the Branding.css file or the script will not find them. -->
			<xsl:call-template name="head-style-urls"/>

			<xsl:call-template name="head-stylesheet"/>
			<xsl:if test="$downscale-browser">
				<xsl:call-template name="head-styles-external"/>
			</xsl:if>
			<xsl:call-template name="head-script"/>

			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="xhtml:head//xhtml:meta"
								name="ps-head-meta">
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template match="xhtml:head//xhtml:xml"
								name="ps-head-xml">
		<xsl:copy-of select="."/>
	</xsl:template>

	<!-- Remove branding data from the header - it's no longer required -->
	<xsl:template match="/xhtml:html/xhtml:head/xhtml:xml[@id='BrandingData']"/>

	<!-- ============================================================================================
	Header Parts
	============================================================================================= -->

	<xsl:template name="head-favicon">
		<xsl:element name="link"
								 namespace="{$xhtml}">
			<xsl:attribute name="rel">
				<xsl:value-of select="'SHORTCUT ICON'"/>
			</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:call-template name="ms-xhelp">
					<xsl:with-param name="ref"
													select="'favicon.ico'"/>
				</xsl:call-template>
			</xsl:attribute>
		</xsl:element>
	</xsl:template>

	<xsl:template name="head-stylesheet">
		<xsl:element name="link" namespace="{$xhtml}">
			<xsl:attribute name="rel">
				<xsl:value-of select="'stylesheet'"/>
			</xsl:attribute>
			<xsl:attribute name="type">
				<xsl:value-of select="'text/css'"/>
			</xsl:attribute>
			<xsl:attribute name="href">
        		<xsl:choose>
        			<xsl:when test="$downscale-browser">
        				<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/../styles/branding.css'))"/>
        			</xsl:when>
        			<xsl:when test="$pre-branding">
        				<xsl:value-of select="branding:BackslashesToFrontslashes('styles/branding.css')"/>
        			</xsl:when>
        			<xsl:otherwise>
        				<xsl:value-of select="concat($brandingPath,branding:BackslashesToFrontslashes('branding.css'))"/>
        			</xsl:otherwise>
        		</xsl:choose>
			</xsl:attribute>
		</xsl:element>
		<xsl:element name="link" namespace="{$xhtml}">
			<xsl:attribute name="rel">
				<xsl:value-of select="'stylesheet'"/>
			</xsl:attribute>
			<xsl:attribute name="type">
				<xsl:value-of select="'text/css'"/>
			</xsl:attribute>
			<xsl:attribute name="href">
        		<xsl:choose>
        			<xsl:when test="$downscale-browser">
        				<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/../styles/',$css-file))"/>
        			</xsl:when>
        			<xsl:when test="$pre-branding">
        				<xsl:value-of select="branding:BackslashesToFrontslashes(concat('styles/',$css-file))"/>
        			</xsl:when>
        			<xsl:otherwise>
        				<xsl:value-of select="concat($brandingPath,branding:BackslashesToFrontslashes($css-file))"/>
        			</xsl:otherwise>
        		</xsl:choose>
			</xsl:attribute>
		</xsl:element>
	</xsl:template>

	<!-- Hack to fix up the background image URLs.  See onLoad() in Branding.js.
 	     NOTE: These MUST appear INLINE and BEFORE the Branding.css file or the script will not find them. -->
	<xsl:template name="head-style-urls">
		<xsl:element name="style" namespace="{$xhtml}">
			<xsl:attribute name="type">text/css</xsl:attribute>
			<xsl:text>.OH_CodeSnippetContainerTabLeftActive, .OH_CodeSnippetContainerTabLeft,.OH_CodeSnippetContainerTabLeftDisabled { backgroundImageName: tabLeftBG.gif; }</xsl:text>
			<xsl:text>.OH_CodeSnippetContainerTabRightActive, .OH_CodeSnippetContainerTabRight,.OH_CodeSnippetContainerTabRightDisabled { backgroundImageName: tabRightBG.gif; }</xsl:text>
			<xsl:text>.OH_footer { backgroundImageName: footer_slice.gif; background-position: top; background-repeat: repeat-x; }</xsl:text>
		</xsl:element>
	</xsl:template>

	<xsl:template name="head-styles-external">
		<xsl:element name="style" namespace="{$xhtml}">
			<xsl:attribute name="type">text/css</xsl:attribute>
			body
			{
			border-left:5px solid #e6e6e6;
			overflow-x:scroll;
			overflow-y:scroll;
			}
		</xsl:element>
	</xsl:template>

	<xsl:template name="head-script">
		<xsl:element name="script" namespace="{$xhtml}">
			<xsl:attribute name="src">
        		<xsl:choose>
        			<xsl:when test="$downscale-browser">
        				<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/../scripts/',$js-file))"/>
        			</xsl:when>
        			<xsl:when test="$pre-branding">
        				<xsl:value-of select="branding:BackslashesToFrontslashes(concat('scripts/',$js-file))"/>
        			</xsl:when>
        			<xsl:otherwise>
        				<xsl:value-of select="concat($brandingPath,branding:BackslashesToFrontslashes($js-file))"/>
        			</xsl:otherwise>
        		</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="type">
				<xsl:value-of select="'text/javascript'"/>
			</xsl:attribute>
			<xsl:comment/>
		</xsl:element>
	</xsl:template>

</xsl:stylesheet>
