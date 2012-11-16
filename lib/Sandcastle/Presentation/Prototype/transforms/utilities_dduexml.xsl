<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
		xmlns:xlink="http://www.w3.org/1999/xlink"
		xmlns:mshelp="http://msdn.microsoft.com/mshelp" >

  <xsl:import href="../../shared/transforms/utilities_dduexml.xsl" />

  <!-- sections -->

	<xsl:template match="ddue:remarks">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="remarksTitle" /></xsl:with-param>
				<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeExamples">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="examplesTitle" /></xsl:with-param>
				<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:threadSafety">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="threadSafetyTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:notesForImplementers">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="notesForImplementersTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:notesForCallers">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="notesForCallersTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

    <xsl:template match="ddue:schemaHierarchy">
      <xsl:for-each select="ddue:link">
        <xsl:call-template name="indent">
          <xsl:with-param name="count" select="position()"/>
        </xsl:call-template>
        <xsl:apply-templates select="."/>
        <br/>
      </xsl:for-each>
    </xsl:template>

    <!-- indent by 2*n spaces -->
    <xsl:template name="indent">
      <xsl:param name="count" />
      <xsl:if test="$count &gt; 1">
        <xsl:text>&#160;&#160;</xsl:text>
        <xsl:call-template name="indent">
          <xsl:with-param name="count" select="$count - 1" />
        </xsl:call-template>
      </xsl:if>
    </xsl:template>

	<xsl:template match="ddue:syntaxSection">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="syntaxTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:legacySyntax">
		<pre xml:space="preserve"><xsl:copy-of select="."/></pre>
	</xsl:template>

	<xsl:template match="ddue:relatedTopics">
		<xsl:if test="count(*) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="relatedTitle" /></xsl:with-param>
				<xsl:with-param name="content">
					<xsl:for-each select="ddue:codeEntityReference|ddue:link|ddue:legacyLink|ddue:externalLink">
						<xsl:apply-templates select="." />
						<br />
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- just skip over these -->
	<xsl:template match="ddue:content | ddue:codeExample | ddue:legacy">
		<xsl:apply-templates />
	</xsl:template>

	<!-- block elements -->

	<xsl:template match="ddue:table">
      <xsl:if test="normalize-space(ddue:title)">
        <div class="caption">
          <xsl:value-of select="ddue:title"/>
        </div>
      </xsl:if>
      <table class="authoredTable">
	    <xsl:apply-templates />
	  </table>
	</xsl:template>

	<xsl:template match="ddue:tableHeader">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="ddue:row">
		<tr>
			<xsl:apply-templates />
		</tr>
	</xsl:template>

	<xsl:template match="ddue:entry">
		<td><xsl:apply-templates select="@address" /><xsl:apply-templates /></td>
	</xsl:template>

	<xsl:template match="ddue:tableHeader/ddue:row/ddue:entry">
		<th>
			<xsl:apply-templates />
		</th>
	</xsl:template>

    <xsl:template match="ddue:definitionTable">
      <dl class="authored">
        <xsl:apply-templates />
      </dl>
    </xsl:template>

    <xsl:template match="ddue:definedTerm">
      <dt><xsl:apply-templates select="@address" /><xsl:apply-templates /></dt>
    </xsl:template>

    <xsl:template match="ddue:definition">
      <dd>
        <xsl:apply-templates />
      </dd>
    </xsl:template>

	<xsl:template match="ddue:code">
		<div class="code"><pre xml:space="preserve"><xsl:apply-templates /></pre></div>
	</xsl:template>

	<xsl:template match="ddue:sampleCode">
		<div><b><xsl:value-of select="@language"/></b></div>
		<div class="code"><pre xml:space="preserve"><xsl:apply-templates /></pre></div>
	</xsl:template>

	<xsl:template name="composeCode">
		<xsl:copy-of select="." />
		<xsl:variable name="next" select="following-sibling::*[1]" />
		<xsl:if test="boolean($next/@language) and boolean(local-name($next)=local-name())">
			<xsl:for-each select="$next">
				<xsl:call-template name="composeCode" />
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

  <xsl:template match="ddue:alert">
    <xsl:variable name="title">
      <xsl:choose>
        <xsl:when test="@class='note'">
          <xsl:text>noteTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='tip'">
          <xsl:text>tipTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='caution' or @class='warning'">
          <xsl:text>cautionTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='security' or @class='security note'">
          <xsl:text>securityTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='important'">
          <xsl:text>importantTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='vb' or @class='VB' or @class='VisualBasic' or @class='visual basic note'">
          <xsl:text>visualBasicTitle</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cs' or @class='CSharp' or @class='c#' or @class='C#' or @class='visual c# note'">
          <xsl:text>visualC#Title</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cpp' or @class='c++' or @class='C++' or @class='CPP' or @class='visual c++ note'">
          <xsl:text>visualC++Title</xsl:text>
        </xsl:when>
        <xsl:when test="@class='JSharp' or @class='j#' or @class='J#' or @class='visual j# note'">
          <xsl:text>visualJ#Title</xsl:text>
        </xsl:when>
        <xsl:when test="@class='implement'">
          <xsl:text>NotesForImplementers</xsl:text>
        </xsl:when>
        <xsl:when test="@class='caller'">
          <xsl:text>NotesForCallers</xsl:text>
        </xsl:when>
        <xsl:when test="@class='inherit'">
          <xsl:text>NotesForInheritors</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>noteTitle</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="altTitle">
      <xsl:choose>
        <xsl:when test="@class='note' or @class='implement' or @class='caller' or @class='inherit'">
          <xsl:text>noteAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='tip'">
          <xsl:text>tipAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='caution' or @class='warning'">
          <xsl:text>cautionAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='security' or @class='security note'">
          <xsl:text>securityAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='important'">
          <xsl:text>importantAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='vb' or @class='VB' or @class='VisualBasic' or @class='visual basic note'">
          <xsl:text>visualBasicAltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cs' or @class='CSharp' or @class='c#' or @class='C#' or @class='visual c# note'">
          <xsl:text>visualC#AltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cpp' or @class='c++' or @class='C++' or @class='CPP' or @class='visual c++ note'">
          <xsl:text>visualC++AltText</xsl:text>
        </xsl:when>
        <xsl:when test="@class='JSharp' or @class='j#' or @class='J#' or @class='visual j# note'">
          <xsl:text>visualJ#AltText</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>noteAltText</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="noteImg">
      <xsl:choose>
        <xsl:when test="@class='note' or @class='tip' or @class='implement' or @class='caller' or @class='inherit'">
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='caution' or @class='warning'">
          <xsl:text>alert_caution.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='security' or @class='security note'">
          <xsl:text>alert_security.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='important'">
          <xsl:text>alert_caution.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='vb' or @class='VB' or @class='VisualBasic' or @class='visual basic note'">
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cs' or @class='CSharp' or @class='c#' or @class='C#' or @class='visual c# note'">
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='cpp' or @class='c++' or @class='C++' or @class='CPP' or @class='visual c++ note'">
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:when>
        <xsl:when test="@class='JSharp' or @class='j#' or @class='J#' or @class='visual j# note'">
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>alert_note.gif</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="alert">
      <table>
        <tr>
          <th><img>
            <includeAttribute item="iconPath" name="src">
              <parameter>
                <xsl:value-of select="$noteImg"/>
              </parameter>
            </includeAttribute>
            <includeAttribute name="title" item="{$altTitle}" />
          </img>
          <xsl:text> </xsl:text>
          <include item="{$title}" /></th>
        </tr>
        <tr>
          <td>
            <xsl:apply-templates />
          </td>
        </tr>
      </table>
    </div>
  </xsl:template>

  <xsl:template match="ddue:sections">
    <xsl:apply-templates select="ddue:section" />
  </xsl:template>

  <xsl:template match="ddue:section">
    <xsl:apply-templates select="@address" />
	<xsl:call-template name="section">
		<xsl:with-param name="title">
			<xsl:value-of select="ddue:title" />
		</xsl:with-param>
		<xsl:with-param name="content">
			<div class="subsection">
				<xsl:apply-templates select="ddue:content"/>
				<xsl:apply-templates select="ddue:sections" />
			</div>
		</xsl:with-param>
	</xsl:call-template>
  </xsl:template>
<!--
  <xsl:template match="@address">
    <a name="{string(.)}" />
  </xsl:template>
-->
    <xsl:template match="ddue:mediaLink">
      <div>
        <xsl:choose>
          <xsl:when test="ddue:image[@placement='center']">
            <xsl:attribute name="class">mediaCenter</xsl:attribute>
          </xsl:when>
          <xsl:when test="ddue:image[@placement='far']">
            <xsl:attribute name="class">mediaFar</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">mediaNear</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="ddue:caption and not(ddue:caption[@placement='after'])">
          <div class="caption">
            <xsl:if test="ddue:caption[@lead]">
              <span class="captionLead"><xsl:value-of select="normalize-space(ddue:caption/@lead)" />:</span>
            </xsl:if>
            <xsl:apply-templates select="ddue:caption" />
          </div>
        </xsl:if>
        <artLink target="{ddue:image/@xlink:href}" />
        <xsl:if test="ddue:caption and ddue:caption[@placement='after']">
          <div class="caption">
            <xsl:if test="ddue:caption[@lead]">
              <span class="captionLead"><xsl:value-of select="normalize-space(ddue:caption/@lead)" />:</span>
            </xsl:if>
            <xsl:apply-templates select="ddue:caption" />
          </div>
        </xsl:if>
    </div>
    </xsl:template>

    <xsl:template match="ddue:mediaLinkInline">
      <span class="media"><artLink target="{ddue:image/@xlink:href}" /></span>
    </xsl:template>

	<xsl:template match="ddue:procedure">
        <xsl:apply-templates select="@address" />
		<xsl:call-template name="section">
			<xsl:with-param name="title"><xsl:value-of select="ddue:title" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates select="ddue:steps" />
				<xsl:apply-templates select="ddue:conclusion" />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:steps">
		<xsl:choose>
			<xsl:when test="@class='ordered'">
				<ol>
					<xsl:apply-templates select="ddue:step" />
				</ol>
			</xsl:when>
			<xsl:when test="@class='bullet'">
				<ul>
					<xsl:apply-templates select="ddue:step" />
				</ul>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:step">
		<li><xsl:apply-templates select="@address" /><xsl:apply-templates /></li>
	</xsl:template>


	<xsl:template match="ddue:inThisSection">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="inThisSectionTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:buildInstructions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="buildInstructionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:nextSteps">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="nextStepsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:requirements">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="requirementsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- inline elements -->

	<xsl:template match="ddue:languageKeyword">
			<xsl:variable name="word" select="." />
    <span class="keyword" sdata="langKeyword" value="{$word}">
			<xsl:choose>
				<xsl:when test="$word='null' or $word='Nothing' or $word='nullptr'">
					<span class="cs">null</span>
					<span class="vb">Nothing</span>
					<span class="cpp">nullptr</span>
				</xsl:when>
				<xsl:when test="$word='static' or $word='Shared'">
					<span class="cs">static</span>
					<span class="vb">Shared</span>
					<span class="cpp">static</span>
				</xsl:when>
				<xsl:when test="$word='virtual' or $word='Overridable'">
					<span class="cs">virtual</span>
					<span class="vb">Overridable</span>
					<span class="cpp">virtual</span>
				</xsl:when>
				<xsl:when test="$word='true' or $word='True'">
					<span class="cs">true</span>
					<span class="vb">True</span>
					<span class="cpp">true</span>
				</xsl:when>
				<xsl:when test="$word='false' or $word='False'">
					<span class="cs">false</span>
					<span class="vb">False</span>
					<span class="cpp">false</span>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="." />
				</xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>

  <!-- links -->

  <xsl:template match="ddue:dynamicLink[@type='inline']">
    <mshelp:ktable disambiguator='span' indexMoniker='!DefaultDynamicLinkIndex'>
      <xsl:attribute name="keywords">
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </xsl:attribute>
      <includeAttribute name="prefix" item="dynamicLinkInlinePreFixText" />
      <includeAttribute name="postfix" item="dynamicLinkInlinePostFixText" />
      <includeAttribute name="separator" item="dynamicLinkInlineSeperatorText" />
    </mshelp:ktable>
  </xsl:template>

  <xsl:template match="ddue:dynamicLink[@type='table']">
    <include item="mshelpKTable">
      <parameter>
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </parameter>
    </include>
  </xsl:template>

  <xsl:template match="ddue:dynamicLink[@type='bulleted']">
    <mshelp:ktable disambiguator='span' indexMoniker='!DefaultDynamicLinkIndex'>
      <xsl:attribute name="keywords">
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </xsl:attribute>
      <xsl:attribute name="prefix">&lt;ul&gt;&lt;li&gt;</xsl:attribute>
      <xsl:attribute name="postfix">&lt;/li&gt;&lt;/ul&gt;</xsl:attribute>
      <xsl:attribute name="separator">&lt;/li&gt;&lt;li&gt;</xsl:attribute>
    </mshelp:ktable>
  </xsl:template>

  <xsl:template match="ddue:codeFeaturedElement">
    <xsl:if test="normalize-space(.)">
      <b><xsl:apply-templates/></b>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:languageReferenceRemarks">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="remarksTitle" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:attributesandElements">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="attributesAndElements" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:attributes">
    <h4 class="subHeading"><include item="attributes"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:attribute">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:attribute/ddue:title">
    <h4 class="subHeading"><xsl:apply-templates/></h4>
  </xsl:template>

  <xsl:template match="ddue:childElement">
    <h4 class="subHeading"><include item="childElement"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:parentElement">
    <h4 class="subHeading"><include item="parentElement"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:textValue">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="textValue" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:elementInformation">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="elementInformation" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:dotNetFrameworkEquivalent">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="dotNetFrameworkEquivalent" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:prerequisites">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="prerequisites" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:type">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:robustProgramming">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="robustProgramming" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:security">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="securitySection" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:externalResources">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="externalResources" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:demonstrates">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="demonstrates" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:appliesTo">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="appliesTo" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:conclusion">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="conclusion" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:background">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="background" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:whatsNew">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="whatsNew" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
	<xsl:template name="createReferenceLink">
		<xsl:param name="id" />
		<xsl:param name="qualified" select="false()" />
		<referenceLink target="{$id}" qualified="{$qualified}" />
	</xsl:template>


	<!-- this is temporary -->
        <xsl:template match="ddue:snippets">
		<xsl:variable name="codeId" select="generate-id()" />
          <div name="snippetGroup">
		<table class="filter"><tr class="tabs" id="ct_{$codeId}">
			<xsl:for-each select="ddue:snippet">
                  <td class="tab" x-lang="{@language}" onclick="toggleClass('ct_{$codeId}','x-lang','{@language}','activeTab','tab'); toggleStyle('cb_{$codeId}','x-lang','{@language}','display','block','none');"><include item="{@language}Label" /></td>
			</xsl:for-each>
		</tr></table>
		<div id="cb_{$codeId}">
			<xsl:for-each select="ddue:snippet">
				<div class="code" x-lang="{@language}"><pre xml:space="preserve"><xsl:copy-of select="node()" /></pre></div>
			</xsl:for-each>
		</div>
          </div>
	</xsl:template>

	<xsl:template name="section">
		<xsl:param name="title" />
		<xsl:param name="content" />
		<div class="section">
			<div class="sectionTitle" onclick="toggleSection(this.parentNode)">
				<img>
					<includeAttribute name="src" item="iconPath">
						<parameter>collapse_all.gif</parameter>
					</includeAttribute>
				</img>
				<xsl:text> </xsl:text>
				<xsl:copy-of select="$title" />
			</div>
			<div class="sectionContent">
				<xsl:copy-of select="$content" />
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
