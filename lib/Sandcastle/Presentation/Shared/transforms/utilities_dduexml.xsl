<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
		xmlns:xlink="http://www.w3.org/1999/xlink"
		xmlns:mshelp="http://msdn.microsoft.com/mshelp" >

	<!-- sections -->

    <xsl:template match="ddue:summary">
      <xsl:if test="not(@abstract='true')">
      <!-- The ddue:summary element is redundant since it's optional in
           the MAML schema but ddue:introduction is not.  Using abstract='true'
           will prevent the summary from being included in the topic; however,
           the first child para element in the summary will still be used as
           the abstract for the topic's Help 2.x metadata. -->
        <div class="summary">
          <xsl:apply-templates />
        </div>
      </xsl:if>
    </xsl:template>

  <xsl:template match="@address">
    <a name="{string(.)}"><xsl:text> </xsl:text></a>
  </xsl:template>

  <!-- block elements -->

	<xsl:template match="ddue:para">
		<p><xsl:apply-templates /></p>
	</xsl:template>

	<xsl:template match="ddue:list">
		<xsl:choose>
			<xsl:when test="@class='bullet'">
				<ul>
					<xsl:apply-templates select="ddue:listItem" />
				</ul>
			</xsl:when>
			<xsl:when test="@class='ordered'">
				<ol>
                  <xsl:if test="@start">
                    <xsl:attribute name="start"><xsl:value-of select="@start"/></xsl:attribute>
                  </xsl:if>
                  <xsl:apply-templates select="ddue:listItem" />
				</ol>
			</xsl:when>
			<xsl:otherwise>
				<ul class="nobullet">
					<xsl:apply-templates select="ddue:listItem" />
				</ul>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:listItem">
		<li><xsl:apply-templates select="@address" /><xsl:apply-templates /></li>
	</xsl:template>

	<!-- inline elements -->

	<xsl:template match="ddue:parameterReference">
    <xsl:if test="normalize-space(.)">
      <span class="parameter" sdata="paramReference">
        <xsl:value-of select="." />
      </span>
    </xsl:if>	</xsl:template>

	<xsl:template match="ddue:ui">
    <xsl:if test="normalize-space(.)">
      <span class="ui"><xsl:value-of select="." /></span>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:userInput | ddue:userInputLocalizable">
    <xsl:if test="normalize-space(.)">
      <span class="input"><xsl:value-of select="." />
      </span>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:newTerm">
    <xsl:if test="normalize-space(.)">
      <span class="term"><xsl:value-of select="." /></span>
    </xsl:if>
	</xsl:template>

    <xsl:template match="ddue:math">
      <xsl:if test="normalize-space(.)">
        <span class="math"><xsl:apply-templates/></span>
      </xsl:if>
    </xsl:template>

    <xsl:template match="ddue:command">
      <xsl:if test="normalize-space(.)">
        <span class="command"><xsl:apply-templates /></span>
      </xsl:if>
    </xsl:template>

    <xsl:template match="ddue:replaceable">
      <xsl:if test="normalize-space(.)">
        <span class="parameter"><xsl:apply-templates /></span>
      </xsl:if>
    </xsl:template>

    <xsl:template match="ddue:literal">
      <xsl:if test="normalize-space(.)">
        <span class="literalValue"><xsl:apply-templates /></span>
      </xsl:if>
    </xsl:template>

    <xsl:template match="ddue:codeInline|ddue:computerOutputInline|ddue:environmentVariable">
      <xsl:if test="normalize-space(.)">
        <span class="code"><xsl:value-of select="." /></span>
      </xsl:if>
    </xsl:template>

	<xsl:template match="ddue:subscript | ddue:subscriptType">
    <xsl:if test="normalize-space(.)">
      <sub><xsl:value-of select="." /></sub>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:superscript | ddue:superscriptType">
    <xsl:if test="normalize-space(.)">
      <sup><xsl:value-of select="." /></sup>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyBold">
    <xsl:if test="normalize-space(.)">
      <b><xsl:apply-templates /></b>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyItalic">
    <xsl:if test="normalize-space(.)">
      <i><xsl:apply-templates /></i>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyUnderline">
    <xsl:if test="normalize-space(.)">
      <u><xsl:apply-templates /></u>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:embeddedLabel">
    <xsl:if test="normalize-space(.)">
      <span class="label"><xsl:apply-templates/></span>
    </xsl:if>
  </xsl:template>

    <xsl:template match="ddue:errorInline|ddue:fictitiousUri|ddue:localUri">
      <xsl:if test="normalize-space(.)">
        <span class="italic"><xsl:value-of select="." /></span>
      </xsl:if>
    </xsl:template>

  <xsl:template match="ddue:quote">
    <xsl:if test="normalize-space(.)">
      <blockQuote><xsl:apply-templates/></blockQuote>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:quoteInline">
    <xsl:if test="normalize-space(.)">
      <q><xsl:apply-templates/></q>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:date">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:foreignPhrase">
    <xsl:if test="normalize-space(.)">
      <span class="foreignPhrase"><xsl:apply-templates/></span>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:phrase">
    <xsl:if test="normalize-space(.)">
      <span class="phrase"><xsl:apply-templates/></span>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:system|ddue:hardware|ddue:application|ddue:database">
    <xsl:if test="normalize-space(.)">
      <b><xsl:apply-templates/></b>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:placeholder">
    <xsl:if test="normalize-space(.)">
      <span class="placeholder"><xsl:apply-templates/></span>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:copyright">
    <!-- <p>{0} &copy;{1}{2}. All rights reserved.</p> -->
    <include item="copyrightNotice">
      <parameter>
        <xsl:value-of select="ddue:trademark" />
      </parameter>
      <parameter>
        <xsl:for-each select="ddue:year">
          <xsl:if test="position() = 1">
            <xsl:text> </xsl:text>
          </xsl:if>
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:for-each>
      </parameter>
      <parameter>
        <xsl:for-each select="ddue:holder">
          <xsl:if test="position() = 1">
            <xsl:text> </xsl:text>
          </xsl:if>
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:for-each>
      </parameter>
    </include>
  </xsl:template>

  <xsl:template match="ddue:corporation">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:country">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:unmanagedCodeEntityReference">
    <xsl:if test="normalize-space(.)">
      <b><xsl:apply-templates/></b>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:localizedText">
    <xsl:apply-templates/>
  </xsl:template>

  <!-- links -->

  <xsl:template match="ddue:externalLink">
    <a>
      <xsl:attribute name="href"><xsl:value-of select="normalize-space(ddue:linkUri)" /></xsl:attribute>
      <xsl:if test="normalize-space(ddue:linkAlternateText)">
        <xsl:attribute name="title"><xsl:value-of select="normalize-space(ddue:linkAlternateText)" /></xsl:attribute>
      </xsl:if>
      <xsl:attribute name="target">
        <xsl:choose>
          <xsl:when test="normalize-space(ddue:linkTarget)">
            <xsl:value-of select="normalize-space(ddue:linkTarget)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>_blank</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:value-of select="normalize-space(ddue:linkText)" />
    </a>
  </xsl:template>

  <xsl:template match="ddue:link">
    <span sdata="link">
    <xsl:choose>
      <xsl:when test="starts-with(@xlink:href,'#')">
        <!-- in-page link -->
        <a href="{@xlink:href}">
          <xsl:apply-templates />
        </a>
      </xsl:when>
      <xsl:otherwise>
        <!-- verified, external link -->
        <conceptualLink target="{@xlink:href}">
          <xsl:apply-templates />
        </conceptualLink>
      </xsl:otherwise>
    </xsl:choose>
    </span>
  </xsl:template>

  <xsl:template match="ddue:legacyLink">
    <xsl:choose>
      <xsl:when test="starts-with(@xlink:href,'#')">
        <!-- in-page link -->
        <a href="{@xlink:href}">
          <xsl:apply-templates />
        </a>
      </xsl:when>
      <xsl:otherwise>
        <!-- unverified, external link -->
        <mshelp:link keywords="{@xlink:href}" tabindex="0">
          <xsl:apply-templates />
        </mshelp:link>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ddue:codeEntityReference">
    <span sdata="cer" target="{normalize-space(string(.))}">
    <referenceLink target="{normalize-space(string(.))}">
      <xsl:if test="@qualifyHint">
        <xsl:attribute name="show-container">
          <xsl:value-of select="@qualifyHint" />
        </xsl:attribute>
        <xsl:attribute name="show-parameters">
          <xsl:value-of select="@qualifyHint" />
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@autoUpgrade">
        <xsl:attribute name="prefer-overload">
          <xsl:value-of select="@autoUpgrade" />
        </xsl:attribute>
      </xsl:if>
    </referenceLink>
    </span>
  </xsl:template>
  <!-- capture authored glossary <link> nodes -->
  <!-- LEAVE THIS TEMPORARILY to support oldstyle GTMT link tagging -->
  <xsl:template match="ddue:link[starts-with(.,'GTMT#')]">
    <!-- not supporting popup definitions; just show the display text -->
    <span sdata="link">
      <xsl:value-of select="substring-after(.,'GTMT#')"/>
    </span>
  </xsl:template>

  <!-- capture authored glossary <link> nodes -->
  <!-- THIS IS THE NEW STYLE GTMT link tagging -->
  <xsl:template match="ddue:legacyLink[starts-with(@xlink:href,'GTMT#')]">
    <!-- not supporting popup definitions; just show the display text -->
    <xsl:value-of select="."/>
  </xsl:template>
  
	<!-- fail if any unknown elements are encountered -->
<!--
	<xsl:template match="*">
		<xsl:message terminate="yes">
			<xsl:text>An unknown element was encountered.</xsl:text>
		</xsl:message>
	</xsl:template>
-->

  <!-- Glossary document type support -->
  <xsl:variable name="allUpperCaseLetters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
  <xsl:variable name="allLowerCaseLetters">abcdefghijklmnopqrstuvwxyz</xsl:variable>
  
  <xsl:key name="glossaryTermFirstLetters" match="//ddue:glossaryEntry"
           use="translate(substring(ddue:terms/ddue:term/text(),1,1),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ ')"/>
  
  <xsl:template match="ddue:glossary">
    <xsl:if test="ddue:title">
      <h1 class="glossaryTitle"><xsl:value-of select="normalize-space(ddue:title)" /></h1>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="ddue:glossaryDiv">
        <!-- Organized glossary with glossaryDiv elements -->
        <br/>
        <xsl:for-each select="ddue:glossaryDiv">
          <xsl:if test="ddue:title">
            <xsl:choose>
              <xsl:when test="@address">
                <a>
                  <!-- Keep this on one line or the spaces preceeding the "#" end up in the anchor name -->
                  <xsl:attribute name="href">#<xsl:value-of select="@address"/></xsl:attribute>
                  <xsl:value-of select="ddue:title" />
                </a>
              </xsl:when>
              <xsl:otherwise>
                  <xsl:value-of select="ddue:title" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="position() != last()">
            <xsl:text> | </xsl:text>
          </xsl:if>
        </xsl:for-each>

        <xsl:apply-templates select="ddue:glossaryDiv"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- Simple glossary consisting of nothing by glossaryEntry elements -->
        <br/>
        <xsl:call-template name="glossaryLetterBar"/>
        <br/>
        <xsl:call-template name="glossaryGroupByEntriesTermFirstLetter"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="ddue:glossaryDiv">
    <xsl:if test="@address">
      <a>
        <!-- Keep this on one line or the spaces preceeding the address end up in the anchor name -->
        <xsl:attribute name="name"><xsl:value-of select="@address"/></xsl:attribute>
      </a>
    </xsl:if>
    <div class="glossaryDiv">
      <xsl:if test="ddue:title">
        <h2 class="glossaryDivHeading"><xsl:value-of select="ddue:title"/></h2>
      </xsl:if>
      <hr class="glossaryRule"/>
      <xsl:call-template name="glossaryLetterBar">
        <xsl:with-param name="sectionPrefix" select="generate-id()"/>
      </xsl:call-template>
      <br/>
      <xsl:call-template name="glossaryGroupByEntriesTermFirstLetter">
        <xsl:with-param name="sectionPrefix" select="generate-id()"/>
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="glossaryGroupByEntriesTermFirstLetter">
    <xsl:param name="sectionPrefix" select="''"/>
    <xsl:variable name="div" select="."/>
    <!-- Group entries by the first letter of their terms using the Muenchian method.
         http://www.jenitennison.com/xslt/grouping/muenchian.html -->
    <xsl:for-each select="ddue:glossaryEntry[generate-id() = 
                  generate-id(key('glossaryTermFirstLetters',
                  translate(substring(ddue:terms/ddue:term[1]/text(),1,1),$allLowerCaseLetters,concat($allUpperCaseLetters, ' ')))
                  [parent::node() = $div][1])]">
      <xsl:sort select="ddue:terms/ddue:term[1]" />
      <xsl:variable name="letter"
                    select="translate(substring(ddue:terms/ddue:term[1]/text(),1,1),$allLowerCaseLetters,concat($allUpperCaseLetters, ' '))"/>

      <xsl:call-template name="glossaryEntryGroup">
        <xsl:with-param name="link" select="concat($sectionPrefix,$letter)"/>
        <xsl:with-param name="name" select="$letter"/>
        <xsl:with-param name="nodes" select="key('glossaryTermFirstLetters',
                        translate($letter,$allLowerCaseLetters,concat($allUpperCaseLetters, ' ')))
                        [parent::node() = $div]"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="glossaryEntryGroup">
    <xsl:param name="link"/>
    <xsl:param name="name"/>
    <xsl:param name="nodes"/>
    <div class="glossaryGroup">
      <a>
        <xsl:attribute name="name"><xsl:value-of select="$link"/></xsl:attribute>
      </a>
      <h3 class="glossaryGroupHeading"><xsl:value-of select="$name"/></h3>
      <dl class="glossaryGroupList">
        <xsl:apply-templates select="$nodes">
          <xsl:sort select="ddue:terms/ddue:term"/>
        </xsl:apply-templates>
      </dl>
    </div>
  </xsl:template>

  <xsl:template match="ddue:glossaryEntry">
    <dt class="glossaryEntry"><xsl:apply-templates select="@address" />
      <xsl:for-each select="ddue:terms/ddue:term">
        <xsl:sort select="normalize-space(.)" />

        <xsl:if test="@termId">
          <a>
            <!-- Keep this on one line or the spaces preceeding the address end up in the anchor name -->
            <xsl:attribute name="name"><xsl:value-of select="@termId"/></xsl:attribute>
          </a>
        </xsl:if>

        <xsl:value-of select="normalize-space(.)" />
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
    </dt>
    <dd class="glossaryEntry">
      <xsl:apply-templates select="ddue:definition/*"/>

      <xsl:if test="ddue:relatedEntry">
        <div class="relatedEntry">
          <include item="relatedEntries" />&#160;

          <xsl:for-each select="ddue:relatedEntry">
            <xsl:variable name="id" select="@termId" />
            <a>
              <!-- Keep this on one line or the spaces preceeding the address end up in the anchor name -->
              <xsl:attribute name="href">#<xsl:value-of select="@termId"/></xsl:attribute>
              <xsl:value-of select="//ddue:term[@termId=$id]"/>
            </a>
            <xsl:if test="position() != last()">
              <xsl:text>, </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </div>
      </xsl:if>
    </dd>
  </xsl:template>

  <xsl:template name="glossaryLetterBar">
    <xsl:param name="sectionPrefix" select="''"/>
    <div class="glossaryLetterBar">
      <xsl:call-template name="glossaryLetterBarLinkRecursive">
        <xsl:with-param name="sectionPrefix" select="$sectionPrefix"/>
        <xsl:with-param name="bar" select="$allUpperCaseLetters"/>
        <xsl:with-param name="characterPosition" select="1"/>
      </xsl:call-template>
    </div>
  </xsl:template>
 
  <xsl:template name="glossaryLetterBarLinkRecursive">
    <xsl:param name="sectionPrefix"/>
    <xsl:param name="bar"/>
    <xsl:param name="characterPosition"/>
    <xsl:variable name="letter" select="substring($bar,$characterPosition,1)"/>
    <xsl:if test="$letter">
      <xsl:choose>
        <xsl:when test="ddue:glossaryEntry[ddue:terms/ddue:term[1]
                  [translate(substring(text(),1,1),$allLowerCaseLetters,concat($allUpperCaseLetters, ' ')) = $letter]]">
          <xsl:call-template name="glossaryLetterBarLink">
            <xsl:with-param name="link" select="concat($sectionPrefix,$letter)"/>
            <xsl:with-param name="name" select="$letter"/>
          </xsl:call-template>
          <xsl:if test="not($characterPosition = string-length($bar))">
            <xsl:text> | </xsl:text>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="glossaryLetterBarLink">
            <xsl:with-param name="name" select="$letter"/>
          </xsl:call-template>
          <xsl:if test="not($characterPosition = string-length($bar))">
            <xsl:text> | </xsl:text>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:call-template name="glossaryLetterBarLinkRecursive">
        <xsl:with-param name="sectionPrefix" select="$sectionPrefix"/>
        <xsl:with-param name="bar" select="$bar"/>
        <xsl:with-param name="characterPosition" select="$characterPosition + 1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="glossaryLetterBarLink">
    <xsl:param name="link"/>
    <xsl:param name="name"/>
    <xsl:choose>
      <xsl:when test="$link">
        <a>
          <xsl:attribute name="href">#<xsl:value-of select="$link"/></xsl:attribute>
          <xsl:value-of select="$name"/>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <span class="nolink">
          <xsl:value-of select="$name"/>
        </span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Pass through a chunk of markup.  This will allow build components
       to add HTML to a pre-transformed document.  You can also use it in
       topics to support things such as video or image maps that aren't
       addressed by the MAML schema and the Sandcastle transforms. -->
  <xsl:template match="ddue:markup">
    <xsl:copy-of select="node()"/>
  </xsl:template>

</xsl:stylesheet>
