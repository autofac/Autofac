<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                
        exclude-result-prefixes="msxsl"
>

  <xsl:template name="insertKeywordsF1Metadata">

    <!-- authored K -->
    <xsl:variable name="docset" select="translate(/document/metadata/attribute[@name='DocSet'][1]/text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz ')"/>
    <xsl:for-each select="/document/metadata/keyword[@index='K']">
      <xsl:variable name="nestedKeywordText">
        <xsl:call-template name="nestedKeywordText"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="not(contains(text(),'[')) and ($docset='avalon' or $docset='wpf' or $docset='wcf' or $docset='windowsforms')">
          <meta name="System.Keywords">
            <includeAttribute name="content" item="kIndexTermWithTechQualifier">
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
          </meta>
        </xsl:when>
        <xsl:otherwise>
          <meta name="System.Keywords" content="{concat(text(),$nestedKeywordText)}" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>

    <!-- authored F -->
    <xsl:for-each select="/document/metadata/keyword[@index='F']">
      <meta name="Microsoft.Help.F1">
        <xsl:attribute name="content">
          <xsl:value-of select="text()" />
          <xsl:for-each select="keyword[@index='F']">
            <xsl:text>, </xsl:text>
            <xsl:value-of select="text()"/>
          </xsl:for-each>
        </xsl:attribute>
      </meta>
    </xsl:for-each>

    <!-- authored B -->
    <xsl:for-each select="/document/metadata/keyword[@index='B']">
      <meta name="Microsoft.Help.F1">
        <xsl:attribute name="content">
          <xsl:value-of select="text()" />
          <xsl:for-each select="keyword[@index='B']">
            <xsl:text>, </xsl:text>
            <xsl:value-of select="text()"/>
          </xsl:for-each>
        </xsl:attribute>
      </meta>
    </xsl:for-each>

  </xsl:template>

  <xsl:template name="insert30Metadata">

    <!-- System.Language -->
    <meta name="Language">
      <includeAttribute name="content" item="locale" />
    </meta>

    <!-- System.Title -->
    <!-- <title> is set elsewhere (eg, main_conceptual.xsl, utilities_reference.xsl) -->

    <!-- System.Keywords -->
    <xsl:call-template name="insertKeywordsF1Metadata" />

    <!-- Microsoft.Help.Id -->
    <meta name="Microsoft.Help.Id" content="{/document/topic/@id}" />

    <!-- Microsoft.Help.Description -->
    <xsl:variable name="abstract" select="normalize-space(string(/document/topic//ddue:para[1]))" />
    <xsl:if test="$abstract">
      <meta name="Description">
        <xsl:attribute name="content">
          <xsl:call-template name="trimAtPeriod">
            <xsl:with-param name="string" select="$abstract" />
          </xsl:call-template>
        </xsl:attribute>
      </meta>
    </xsl:if>

    <!-- Microsoft.Help.TocParent -->
    <xsl:if test="/document/metadata/attribute[@name='TOCParent']">
      <meta name="Microsoft.Help.TocParent" content="{/document/metadata/attribute[@name='TOCParent']}" />      
    </xsl:if>
    <xsl:if test="/document/metadata/attribute[@name='TOCOrder']">
      <meta name="Microsoft.Help.TocOrder" content="{/document/metadata/attribute[@name='TOCOrder']}" />
    </xsl:if>

    <!-- Microsoft.Help.Product -->
    <!-- Added by MTPS -->

    <!-- Microsoft.Help.ProductVersion -->
    <!-- Added by MTPS -->

    <!-- Microsoft.Help.Category -->
    <xsl:for-each select="/document/metadata/attribute[@name='Category']">
      <meta name="Microsoft.Help.Category" content="{.}" />
    </xsl:for-each>

    <!-- Microsoft.Help.ContentFilter -->
    <xsl:for-each select="/document/metadata/attribute[@name='ContentFilter']">
      <meta name="Microsoft.Help.ContentFilter" content="{.}" />
    </xsl:for-each>

    <!-- Microsoft.Help.ContentType -->
    <xsl:variable name="contentTypeDocStudio">
      <xsl:variable name="lookupValue">
        <xsl:value-of select="local-name(/document/topic/*[1])"/>
      </xsl:variable>
      <xsl:value-of select="msxsl:node-set($topicTypes)/topic[@name = $lookupValue]/text()"/>
    </xsl:variable>

    <xsl:variable name="contentTypeTopicType">
      <xsl:variable name="lookupValue">
        <xsl:value-of select="translate(/document/metadata/topicType/@id, 
                      'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
      </xsl:variable>
      <xsl:value-of select="msxsl:node-set($topicTypes)/topic[@guid = $lookupValue]/text()"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$contentTypeDocStudio">
        <meta name="Microsoft.Help.ContentType" content="{$contentTypeDocStudio}" />
      </xsl:when>
      <xsl:when test="$contentTypeTopicType">
        <meta name="Microsoft.Help.ContentType" content="{$contentTypeTopicType}" />
      </xsl:when>
    </xsl:choose>

    <!-- Microsoft.Package.Book -->
    <xsl:variable name="Book" select="/document/metadata/attribute[@name='Book']/text()" />
    <xsl:if test="$Book">
      <meta name="Microsoft.Package.Book" content="{$Book}" />
    </xsl:if>

    <!-- Source -->
    <xsl:for-each select="/document/metadata/attribute[@name='Source']">
      <meta name="Source" content="{.}" />
    </xsl:for-each>

  </xsl:template>


  <xsl:variable name="topicTypes">
    <topic guid="EF7DDB37-8ED3-4DFA-B38D-5A3CC1906034" name="">Concepts</topic>
    <topic guid="1FE70836-AA7D-4515-B54B-E10C4B516E50" name="developerConceptualDocument">Concepts</topic>
    <topic guid="B137C930-7BF7-48A2-A329-3ADCAEF8868E" name="developerOrientationDocument">Concepts</topic>
    <topic guid="68F07632-C4C5-4645-8DFA-AC87DCB4BD54" name="developerSDKTechnologyOverviewArchitectureDocument">Concepts</topic>
    <topic guid="CDB8C120-888F-447B-8AF8-F9540562E7CA" name="developerSDKTechnologyOverviewOrientationDocument">Concepts</topic>
    <topic guid="356C57C4-384D-4AF2-A637-FDD6F088A033" name="developerSDKTechnologyOverviewScenariosDocument">Concepts</topic>
    <topic guid="19F1BB0E-F32A-4D5F-80A9-211D92A8A715" name="developerSDKTechnologyOverviewTechnologySummaryDocument">Concepts</topic>
    <topic guid="56DB00EC-28BA-4C0D-8694-28E8B244E236" name="developerWhitePaperDocument">Concepts</topic>
    <topic guid="B137C930-7BF7-48A2-A329-3ADCAEF8868E" name="developerOrientationDocument">Concepts</topic>

    <topic guid="DAC3A6A0-C863-4E5B-8F65-79EFC6A4BA09" name="developerHowToDocument">How To</topic>
    <topic guid="4779DD54-5D0C-4CC3-9DB3-BF1C90B721B3" name="developerWalkthroughDocument">How To</topic>

    <topic guid="A635375F-98C2-4241-94E7-E427B47C20B6" name="developerErrorMessageDocument">Reference</topic>
    <topic guid="95DADC4C-A2A6-447A-AA36-B6BE3A4F8DEC" name="developerReferenceWithSyntaxDocument">Reference</topic>
    <topic guid="F9205737-4DEC-4A58-AA69-0E621B1236BD" name="developerReferenceWithoutSyntaxDocument">Reference</topic>
    <topic guid="38C8E0D1-D601-4DBA-AE1B-5BEC16CD9B01" name="developerTroubleshootingDocument">Reference</topic>
    <topic guid="B8ED9F21-39A4-4967-928D-160CD2ED9DCE" name="developerUIReferenceDocument">Reference</topic>
    <topic guid="3272D745-2FFC-48C4-9E9D-CF2B2B784D5F" name="developerXmlReference">Reference</topic>
    <topic guid="A689E19C-2687-4881-8CE1-652FF60CF46C" name="developerGlossaryDocument">Reference</topic>

    <topic guid="069EFD88-412D-4E2F-8848-2D5C3AD56BDE" name="developerSampleDocument">Samples</topic>
    <topic guid="4BBAAF90-0E5F-4C86-9D31-A5CAEE35A416" name="developerSDKTechnologyOverviewCodeDirectoryDocument">Samples</topic>
    <topic guid="4A273212-0AC8-4D72-8349-EC11CD2FF8CD" name="">Samples</topic>
  </xsl:variable>

</xsl:stylesheet>
