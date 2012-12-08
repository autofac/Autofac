<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
    xmlns:mshelp="http://msdn.microsoft.com/mshelp"
    xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt">

    <xsl:import href="../../shared/transforms/utilities_bibliography.xsl"/>
    <xsl:param name="bibliographyData" select="'../data/bibliography.xml'"/>

	<xsl:output method="xml" indent="no" encoding="utf-8" />

	<xsl:include href="utilities_dduexml.xsl" />

	<!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
  <xsl:param name="languages">false</xsl:param>

	<xsl:template match="/document">
		<html xmlns:xlink="http://www.w3.org/1999/xlink">
			<head>
                <META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=UTF-8"/>
				<title><xsl:call-template name="topicTitle"/></title>
				<xsl:call-template name="insertStylesheets" />
				<xsl:call-template name="insertScripts" />
				<xsl:call-template name="insertMetadata" />
			</head>
			<body>
				<xsl:call-template name="control"/>
				<xsl:call-template name="main"/>
			</body>
		</html>
	</xsl:template>

	<!-- document head -->

	<xsl:template name="insertStylesheets">
		<link rel="stylesheet" type="text/css" href="../styles/presentation.css" />
		<!-- make mshelp links work -->
		<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
	</xsl:template>

	<xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>script_prototype.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>EventUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>StyleUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>SplitScreen.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>ElementCollection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>MemberFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>CollapsibleSection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>LanguageFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>CookieDataStore.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>

  </xsl:template>

	<xsl:template name="insertMetadata">
		<xml>
		<!-- mshelp metadata -->

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
      <xsl:for-each select="/document/metadata/keyword[@index='K']">
        <MSHelp:Keyword Index="K">
          <xsl:attribute name="Term">
            <xsl:value-of select="text()" />
            <xsl:for-each select="keyword[@index='K']">
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
      <xsl:if test="(string-length($abstract) &lt; 255) and (string-length($abstract) &gt; 0)">
        <MSHelp:Attr Name="Abstract" Value="{$abstract}" />
      </xsl:if>
      
      <!-- authored attributes -->
      <xsl:for-each select="/document/metadata/attribute">
        <MSHelp:Attr Name="{@name}" Value="{text()}" />
      </xsl:for-each>
      
    </xml>
	</xsl:template>

	<!-- document body -->

	<!-- control window -->

	<xsl:template name="control">
		<div id="control">
			<span class="productTitle"><include item="productTitle" /></span><br/>
			<span class="topicTitle"><xsl:call-template name="topicTitle" /></span><br/>
      
      <xsl:if test="boolean(($languages != 'false') and (count($languages/language) &gt; 0))">
        <div id="toolbar">
          <span id="languageFilter">
            <select id="languageSelector" onchange="var names = this.value.split(' '); toggleVisibleLanguage(names[1]); switchLanguage(names, this.value);">
              <xsl:for-each select="$languages/language">
                <option value="{@name} {@style}">
                  <include item="{@label}Label" />
                </option>
              </xsl:for-each>
            </select>
          </span>
        </div>
      </xsl:if>
     
<!--
			<div id="toolbar">
				<span class="chickenFeet"><xsl:call-template name="chickenFeet" /></span>
			</div>
-->
		</div>
	</xsl:template>

	<!-- Title in topic -->

	<xsl:template name="topicTitle">
    <xsl:choose>
      <xsl:when test="normalize-space(/document/metadata/title)">
        <xsl:value-of select="normalize-space(/document/metadata/title)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="normalize-space(/document/topic/*/ddue:title)"/>
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

	<!-- Title in TOC -->

	<!-- Index entry -->

	<!-- main window -->

	<xsl:template name="main">
		<div id="main">
			<xsl:call-template name="head" />
			<xsl:call-template name="body" />
			<xsl:call-template name="foot" />
		</div>
	</xsl:template>

	<xsl:template name="head">
		<include item="header" />
	</xsl:template>

	<xsl:template name="body">
		<xsl:apply-templates select="topic" />
	</xsl:template> 

	<!-- sections that behave differently in conceptual and reference -->

	<xsl:template match="ddue:title">
		<!-- don't print title -->
	</xsl:template>

	<xsl:template match="ddue:introduction">
    <xsl:apply-templates select="@address" />
		<div class="introduction">
			<xsl:apply-templates />
		</div>
	</xsl:template>

	<xsl:template match="ddue:parameters">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="parametersTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:returnValue">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="returnValueTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:exceptions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:relatedSections">
		<div class="section">
			<div class="sectionTitle"><include item="relatedSectionsTitle" /></div>
			<div class="sectionContent">
				<xsl:apply-templates />
			</div>
		</div>
	</xsl:template>

	<xsl:template match="ddue:relatedTopics">
		<xsl:if test="count(*) > 0">
		<div id="seeAlsoSection" class="section">
			<div class="sectionTitle"><include item="relatedTopicsTitle" /></div>
			<div class="sectionContent">
        <xsl:for-each select="*">
          <xsl:apply-templates select="." />
          <br />
        </xsl:for-each>
			</div>
		</div>
		</xsl:if>
	</xsl:template>

  <xsl:template match="ddue:codeExample">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="Example" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
	<!-- Footer stuff -->
	
	<xsl:template name="foot">
		<include item="footer" />
	</xsl:template>

  <!-- <autoOutline /> or <autoOutline>[#]</autoOutline>
  Inserts a bullet list of links to the topic's sections or a section's
  sub-sections with optional support for limiting the expansion down to a
  specific level.  Authors can use the tag directly or specify a token
  (defined in a token file) in a topic's introduction to get a bullet list of
  the sections; or in a ddue:section/ddue:content to get a bullet list of the
  section's sub-sections.  If the token is used, the shared content component
  replaces <token>autoOutline</token> with an <autoOutline/> node that you
  specify.  This was the old way of doing it but this version allows it to
  be specified directly like any other MAML tag. Examples:

  <autoOutline/>                Show only top-level topic titles
  <autoOutline>1</autoOutline>  Show top-level titles and titles for one level down
  <autoOutline>3</autoOutline>  Show titles from the top down to three levels -->

  <xsl:template match="autoOutline|ddue:autoOutline">
    <xsl:variable name="maxDepth">
      <xsl:choose>
        <xsl:when test="normalize-space(.)">
            <xsl:value-of select="number(normalize-space(.))" />
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="number(0)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="intro" select="@lead"/>
    <xsl:variable name="outlineType">
        <xsl:choose>
            <xsl:when test="@excludeRelatedTopics = 'true'"><xsl:value-of select="string('topNoRelated')" /></xsl:when>
            <xsl:otherwise><xsl:value-of select="string('toplevel')" /></xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <!--if <autoOutline/> is in introduction, it outlines the topic's toplevel sections-->
      <xsl:when test="ancestor::ddue:introduction">
        <xsl:for-each select="ancestor::ddue:introduction/parent::*">
          <xsl:call-template name="insertAutoOutline">
            <xsl:with-param name="intro"><xsl:value-of select="$intro"/></xsl:with-param>
            <xsl:with-param name="outlineType"><xsl:value-of select="$outlineType" /></xsl:with-param>
            <xsl:with-param name="depth"><xsl:value-of select="number(0)"/></xsl:with-param>
            <xsl:with-param name="maxDepth"><xsl:value-of select="$maxDepth"/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <!--if <autoOutline/> is in section/content, it outlines the section's subsections-->
      <xsl:when test="ancestor::ddue:content[parent::ddue:section]">
        <xsl:for-each select="ancestor::ddue:content/parent::ddue:section/ddue:sections">
          <xsl:call-template name="insertAutoOutline">
            <xsl:with-param name="intro"><xsl:value-of select="$intro"/></xsl:with-param>
            <xsl:with-param name="outlineType">subsection</xsl:with-param>
            <xsl:with-param name="depth"><xsl:value-of select="number(0)"/></xsl:with-param>
            <xsl:with-param name="maxDepth"><xsl:value-of select="$maxDepth"/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="insertAutoOutline">
    <xsl:param name="intro"/>
    <xsl:param name="outlineType"/>
    <xsl:param name="depth"/>
    <xsl:param name="maxDepth"/>
    <!--insert an outline if there are sections with title and address-->
    <xsl:if test="ddue:section[ddue:title[normalize-space(.)!='']]">
      <!--insert a boilerplate intro-->
      <xsl:choose>
        <xsl:when test="normalize-space($intro) = 'none'">
          <xsl:text></xsl:text>
        </xsl:when>
        <xsl:when test="normalize-space($intro)">
          <p><xsl:value-of select="normalize-space($intro)"/></p>
        </xsl:when>
        <xsl:when test="$outlineType='toplevel' or $outlineType='topNoRelated'">
          <p><include item="autoOutlineTopLevelIntro"/></p>
        </xsl:when>
        <xsl:when test="$outlineType='subsection'">
          <p><include item="autoOutlineSubsectionIntro"/></p>
        </xsl:when>
      </xsl:choose>
      <ul class="autoOutline">
        <xsl:for-each select="ddue:section[ddue:title[normalize-space(.)!='']]">
          <xsl:call-template name="outlineSectionEntry" />

          <!-- Expand sub-sections too if wanted -->
          <xsl:if test="$depth &lt; $maxDepth">
            <xsl:for-each select="ddue:sections">
              <xsl:call-template name="insertAutoOutline">
                <xsl:with-param name="outlineType">subsubsection</xsl:with-param>
                <xsl:with-param name="depth"><xsl:value-of select="$depth + 1"/></xsl:with-param>
                <xsl:with-param name="maxDepth"><xsl:value-of select="$maxDepth"/></xsl:with-param>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:if>
        </xsl:for-each>
        <!--for toplevel outlines include a link to See Also-->
        <!-- xsl:if test="starts-with($outlineType,'toplevel') and //ddue:relatedTopics[normalize-space(.)!='']"  -->
        <xsl:if test="starts-with($outlineType,'toplevel') and count(//ddue:relatedTopics/*) > 0">
          <li class="outlineSectionEntry">
            <a>
              <xsl:attribute name="href">#seeAlsoSection</xsl:attribute>
              <include item="RelatedTopicsLinkText"/>
            </a>
          </li>
        </xsl:if>
      </ul>
    </xsl:if>
  </xsl:template>

  <!--a list item in the outline's bullet list-->
  <xsl:template name="outlineSectionEntry">
    <xsl:if test="descendant::ddue:content[normalize-space(.)] or count(ddue:content/*) &gt; 0">
      <li class="outlineSectionEntry">
        <a>
          <xsl:if test="@address">
            <!-- Keep this on one line or the spaces preceeding the "#" end up in the anchor name -->
            <xsl:attribute name="href">#<xsl:value-of select="@address"/></xsl:attribute>
          </xsl:if>
          <xsl:value-of select="ddue:title" />
        </a>
        <xsl:if test="normalize-space(ddue:summary)">
          <div class="outlineSectionEntrySummary">
            <xsl:apply-templates select="ddue:summary/node()"/>
          </div>
        </xsl:if>
      </li>
    </xsl:if>
  </xsl:template>

  <!-- Bibliography --> 
  <xsl:key name="citations" match="//ddue:cite" use="text()" />
 
  <xsl:variable name="hasCitations" select="boolean(count(//ddue:cite) > 0)"/>
  
  <xsl:template match="ddue:cite">
    <xsl:variable name="currentCitation" select="text()"/>
    <xsl:for-each select="//ddue:cite[generate-id(.)=generate-id(key('citations',text()))]"> <!-- Distinct citations only -->
      <xsl:if test="$currentCitation=.">
        <xsl:choose>
          <xsl:when test="document($bibliographyData)/bibliography/reference[@name=$currentCitation]">
            <sup class="citation"><a><xsl:attribute name="href">#cite<xsl:value-of select="position()"/></xsl:attribute>[<xsl:value-of select="position()"/>]</a></sup>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
 
  <xsl:template match="ddue:bibliography">
    <xsl:if test="$hasCitations">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'cite'" />
        <xsl:with-param name="title"><include item="bibliographyTitle"/></xsl:with-param>
        <xsl:with-param name="content">
          <xsl:call-template name="autogenBibliographyLinks"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
 
  <xsl:template name="autogenBibliographyLinks">
    <xsl:for-each select="//ddue:cite[generate-id(.)=generate-id(key('citations',text()))]"> <!-- Distinct citations only -->
      <xsl:variable name="citation" select="."/>
      <xsl:variable name="entry" select="document($bibliographyData)/bibliography/reference[@name=$citation]"/>
 
      <xsl:call-template name="bibliographyReference">
        <xsl:with-param name="number" select="position()"/>
        <xsl:with-param name="data" select="$entry"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
  
</xsl:stylesheet>
