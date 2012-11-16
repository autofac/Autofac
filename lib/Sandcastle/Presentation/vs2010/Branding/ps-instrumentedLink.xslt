<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:branding="urn:FH-Branding"
								xmlns:xs="http://www.w3.org/2001/XMLSchema">

	<xsl:template match="xhtml:a"
								name="insLink">
		<xsl:variable name="href">
			<xsl:value-of	select="translate(@href, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
		</xsl:variable>
		<xsl:variable name="assetid">
			<xsl:choose>
				<xsl:when test="contains($href,'?ID=')">
					<xsl:value-of	select="substring-after($href, '?ID=')"/>
				</xsl:when>
				<xsl:when test="not($downscale-browser)">
					<xsl:choose>
						<xsl:when test="contains($href,'.HTM')">
							<xsl:value-of	select="substring-before($href, '.HTM')"/>
						</xsl:when>
						<xsl:when test="contains($href,'#')">
							<xsl:value-of	select="substring-before($href, '#')"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of	select="$href"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="@class='mtps-external-link' or starts-with($href,'HTTP:') or starts-with($href,'HTTPS') or starts-with($href,'WWW')">
			<xsl:element name="img"
									 namespace="{$xhtml}">
				<xsl:attribute name="src">
					<xsl:call-template name="ms-xhelp">
						<xsl:with-param name="ref"
														select="'online_icon.gif'"/>
					</xsl:call-template>
				</xsl:attribute>
				<xsl:attribute name="class">
					<xsl:value-of select="'OH_offlineIcon'"/>
				</xsl:attribute>
				<xsl:attribute name="alt">
					<xsl:value-of select="$onlineTooltip"/>
				</xsl:attribute>
				<xsl:attribute name="title">
					<xsl:value-of select="$onlineTooltip"/>
				</xsl:attribute>
			</xsl:element>
		</xsl:if>
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:if test="@id">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@href">
				<xsl:attribute name="href">
					<xsl:choose>
						<xsl:when test="$downscale-browser">
							<xsl:choose>
								<xsl:when test="$href='MS-XHELP:///?ID=HELPONHELP.HTM'">
									<span color="red">
										<xsl:value-of select="@href"/>
										<xsl:text>&#160;is not a valid link.</xsl:text>
									</span>
								</xsl:when>
								<xsl:when test="$href='INSTALL'">
									<span color="red">
										<xsl:value-of select="@href"/>
										<xsl:text>&#160;is not a valid link.</xsl:text>
									</span>
								</xsl:when>
								<xsl:when test="$href='INSTALL_SETTING'">
									<span color="red">
										<xsl:value-of select="@href"/>
										<xsl:text>&#160;is not a valid link.</xsl:text>
									</span>
								</xsl:when>
								<xsl:when test="@class='mtps-external-link' or starts-with($href,'#') or starts-with($href,'HTTP:') or starts-with($href,'HTTPS') or starts-with($href,'WWW') or starts-with($href,'MAILTO')">
									<!--external link or anchor-->
									<xsl:value-of select="@href"/>
								</xsl:when>
								<xsl:when test="$assetid=''">
									<!--resolved link-->
									<xsl:value-of select="@href"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="concat(translate($assetid, ':.','__'), '.htm')"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="@href='ms-xhelp:///?id=helponhelp.htm'">
									<span color="red">
										<xsl:value-of select="@href"/>
										<xsl:text>&#160;is not a valid link.</xsl:text>
									</span>
								</xsl:when>
								<xsl:when test="@href='install'">
									<xsl:value-of select="concat('ms-xhelp:///?install=2','&amp;product=', $product, '&amp;version=', $version,'&amp;locale=', $locale)"/>
								</xsl:when>
								<xsl:when test="@href='install_setting'">
									<xsl:value-of select="concat('ms-xhelp:///?install=2','&amp;product=', $product, '&amp;version=', $version,'&amp;locale=', $locale,'&amp;settings=')"/>
								</xsl:when>
								<xsl:when test="@class='mtps-external-link' or starts-with(@href,'#') or starts-with(@href,'http:') or starts-with(@href,'https') or starts-with(@href,'www') or starts-with(@href,'mailto')">
									<!--external link or anchor-->
									<xsl:value-of select="@href"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="concat('ms-xhelp:///?method=page&amp;id=', $assetid, '&amp;product=', $product, '&amp;productVersion=', $version,'&amp;topicVersion=', $topicVersion, '&amp;locale=', $locale,'&amp;topicLocale=', $topiclocale)"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
				<xsl:attribute name="target">
					<xsl:choose>
						<xsl:when test="@target">
							<!-- Keep the value specified if already there -->
							<xsl:value-of select="@target"/>
						</xsl:when>
						<xsl:when test="@class='mtps-external-link' or starts-with(@href,'http:') or starts-with(@href,'https') or starts-with(@href,'www') or starts-with(@href,'mailto')">
							<!--external link-->
							<xsl:value-of select="'_blank'"/>
						</xsl:when>
					</xsl:choose>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="node()" />
			<xsl:if test="not(*) and not(text())">
				<xsl:comment/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="xhtml:span[@sdata='link']"
								name="link-span">
		<xsl:apply-templates/>
	</xsl:template>

</xsl:stylesheet>
