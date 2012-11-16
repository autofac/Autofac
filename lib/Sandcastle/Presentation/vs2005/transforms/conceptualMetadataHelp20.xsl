<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl"
>

  <xsl:template name="insertMetadata">
    <xsl:if test="$metadata='true'">
      <xml>
        <!-- mshelp metadata -->

        <!-- insert toctitle -->
        <xsl:if test="normalize-space(/document/metadata/tableOfContentsTitle) and (/document/metadata/tableOfContentsTitle != /document/metadata/title)">
          <MSHelp:TOCTitle Title="{/document/metadata/tableOfContentsTitle}" />
        </xsl:if>

        <!-- link index -->
        <MSHelp:Keyword Index="A" Term="{$key}" />

        <!-- authored NamedUrlIndex -->
        <xsl:for-each select="/document/metadata/keyword[@index='NamedUrlIndex']">
          <MSHelp:Keyword Index="NamedUrlIndex">
            <xsl:attribute name="Term">
              <xsl:value-of select="text()" />
            </xsl:attribute>
          </MSHelp:Keyword>
        </xsl:for-each>

        <!-- authored K -->
        <xsl:variable name="docset" select="translate(/document/metadata/attribute[@name='DocSet'][1]/text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz ')"/>
        <xsl:for-each select="/document/metadata/keyword[@index='K']">
          <xsl:variable name="nestedKeywordText">
            <xsl:call-template name="nestedKeywordText"/>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="not(contains(text(),'[')) and ($docset='avalon' or $docset='wpf' or $docset='wcf' or $docset='windowsforms')">
              <MSHelp:Keyword Index="K">
                <includeAttribute name="Term" item="kIndexTermWithTechQualifier">
                  <parameter>
                    <xsl:value-of select="text()"/>
                  </parameter>
                  <parameter>
                    <xsl:value-of select="$docset"/>
                  </parameter>
                  <parameter>
                    <xsl:value-of select="$nestedKeywordText"/>
                  </parameter>
                </includeAttribute>
              </MSHelp:Keyword>
            </xsl:when>
            <xsl:otherwise>
              <MSHelp:Keyword Index="K" Term="{concat(text(),$nestedKeywordText)}" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>

        <!-- authored S -->
        <xsl:for-each select="/document/metadata/keyword[@index='S']">
          <MSHelp:Keyword Index="S">
            <xsl:attribute name="Term">
              <xsl:value-of select="text()" />
              <xsl:for-each select="keyword[@index='S']">
                <xsl:text>, </xsl:text>
                <xsl:value-of select="text()"/>
              </xsl:for-each>
            </xsl:attribute>
          </MSHelp:Keyword>
          <!-- S index keywords need to be converted to F index keywords -->
          <MSHelp:Keyword Index="F">
            <xsl:attribute name="Term">
              <xsl:value-of select="text()" />
              <xsl:for-each select="keyword[@index='S']">
                <xsl:text>, </xsl:text>
                <xsl:value-of select="text()"/>
              </xsl:for-each>
            </xsl:attribute>
          </MSHelp:Keyword>
        </xsl:for-each>

        <!-- authored F -->
        <xsl:for-each select="/document/metadata/keyword[@index='F']">
          <MSHelp:Keyword Index="F">
            <xsl:attribute name="Term">
              <xsl:value-of select="text()" />
              <xsl:for-each select="keyword[@index='F']">
                <xsl:text>, </xsl:text>
                <xsl:value-of select="text()"/>
              </xsl:for-each>
            </xsl:attribute>
          </MSHelp:Keyword>
        </xsl:for-each>

        <!-- authored B -->
        <xsl:for-each select="/document/metadata/keyword[@index='B']">
          <MSHelp:Keyword Index="B">
            <xsl:attribute name="Term">
              <xsl:value-of select="text()" />
              <xsl:for-each select="keyword[@index='B']">
                <xsl:text>, </xsl:text>
                <xsl:value-of select="text()"/>
              </xsl:for-each>
            </xsl:attribute>
          </MSHelp:Keyword>
        </xsl:for-each>

        <!-- Topic version -->
        <MSHelp:Attr Name="RevisionNumber" Value="{/document/topic/@revisionNumber}" />

        <!-- Asset ID -->
        <MSHelp:Attr Name="AssetID" Value="{/document/topic/@id}" />

        <!-- Abstract -->
        <xsl:variable name="abstract" select="normalize-space(string(/document/topic//ddue:para[1]))" />
        <xsl:choose>
          <xsl:when test="string-length($abstract) &gt; 254">
            <MSHelp:Attr Name="Abstract" Value="{concat(substring($abstract,1,250), ' ...')}" />
          </xsl:when>
          <xsl:when test="string-length($abstract) &gt; 0">
            <MSHelp:Attr Name="Abstract" Value="{$abstract}" />
          </xsl:when>
        </xsl:choose>

        <!-- Autogenerate codeLang attributes based on the snippets -->
        <xsl:call-template name="mshelpCodelangAttributes">
          <xsl:with-param name="snippets" select="/document/topic/*//ddue:snippets/ddue:snippet" />
        </xsl:call-template>

        <!-- authored attributes -->
        <xsl:for-each select="/document/metadata/attribute">
          <MSHelp:Attr Name="{@name}" Value="{text()}" />
        </xsl:for-each>

        <!-- TopicType attribute -->
        <xsl:for-each select="/document/topic/*[1]">
          <MSHelp:Attr Name="TopicType">
            <includeAttribute name="Value" item="TT_{local-name()}"/>
          </MSHelp:Attr>
        </xsl:for-each>

        <!-- Locale attribute -->
        <MSHelp:Attr Name="Locale">
          <includeAttribute name="Value" item="locale"/>
        </MSHelp:Attr>

      </xml>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
