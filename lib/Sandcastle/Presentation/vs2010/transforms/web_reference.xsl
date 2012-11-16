<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
                xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
                xmlns:MSHelp="http://msdn.microsoft.com/mshelp" 
                xmlns:asp="http://temp.uri/asp.net" 
                xmlns:xlink="http://www.w3.org/1999/xlink">
	<!-- ======================================================================================== -->

	<!-- WebDocs overrides some of the default Manifold transforms -->
	<xsl:import href="main_sandcastle.xsl"/>

	<xsl:output method="xml"
							indent="no"
							encoding="utf-8"
							omit-xml-declaration="yes"/>

	<xsl:namespace-alias stylesheet-prefix="xlink" result-prefix="#default"/>

	<!-- ======================================================================================== -->

	<xsl:template match="/">
    <root>
			<xsl:call-template name="t_insertFilename" />
			<metadata>
			  <topic id="{$key}">
				  <title><xsl:call-template name="t_topicTitlePlain"/></title>
				  <pageUrl>api/<xsl:value-of select="/document/reference/file/@name"/>.aspx</pageUrl>
			  </topic>
			</metadata>
      <content>
        <xsl:processing-instruction name="literal-text">
          <xsl:text><![CDATA[<%@Page Language="C#" MasterPageFile="~/site.master" %>]]>&#x0a;</xsl:text>
          <xsl:text>&#x0a;</xsl:text>
        </xsl:processing-instruction>
        <asp:Content ID="MainBodyContent" ContentPlaceHolderID="MainBody" Runat="Server">
					<div class="topic">
						<xsl:call-template name="t_bodyTitle"/>
						<xsl:call-template name="t_bodyMain"/>
					</div>
				</asp:Content>
		  </content>
    </root>
	</xsl:template>

	<xsl:template name="t_insertFilename">
		<xsl:attribute name="fileName">
			<xsl:value-of select="/document/reference/file/@name" />
		</xsl:attribute>
	</xsl:template>

</xsl:stylesheet>
