<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								exclude-result-prefixes="msxsl"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:cs="urn:code-snippet"
>
	<!-- ============================================================================================
	Parameters

	These parameters are used to remove elements from pre-formatted code snippets when making the
	plain copy used for Copy and Print.
	============================================================================================= -->

	<xsl:param name="plain-remove-element"></xsl:param>
	<xsl:param name="plain-remove-id"></xsl:param>
	<xsl:param name="plain-remove-class"></xsl:param>

  <!-- EFW - Added template to override the default in order to work around a bug in HV 2.0 -->
  <xsl:template name="renderSnippet">
    <xsl:param name="snippetCount" />
    <xsl:param name="snippets"/>
    <xsl:param name="showLanTabs" select="true()" />
    <xsl:param name="unrecognized" select="'false'" />
    <xsl:variable name="owner-id" select="generate-id()"/>
    <xsl:variable name="tabflagtemp" select="''" />
    <!-- this flag shows the visual basic and j# language tabs, the value might contains: usage, declaration, jsharp-->
    <xsl:variable name="tabflag">
      <xsl:for-each select="$snippets">
        <xsl:variable name="lang" select="./@Language"/>
        <xsl:variable name="displang" select="./@DisplayLanguage"/>
        <!-- the var uselang is the real language text that we show in output pages
        if the 'display language' not exist, we use 'language' property
        -->
        <xsl:variable name="uselang">
          <xsl:if test="string-length($displang)>0">
            <xsl:value-of select="$displang"/>
          </xsl:if>
          <xsl:if test="string-length($displang)=0">
            <xsl:value-of select="$lang"/>
          </xsl:if>
        </xsl:variable>
        <!-- we store the lowered text of language so that it is easiler to find languages that we specified-->
        <xsl:variable name="loweredLang" select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
        <xsl:if test="contains($loweredLang, 'basic')">
          <xsl:if test="contains($loweredLang, 'declaration')">
            <xsl:value-of select="concat($tabflagtemp, 'Dec:', position(), ':declaration;')"/>
          </xsl:if>
          <xsl:if test="contains($loweredLang, 'usage')">
            <xsl:value-of select="concat($tabflagtemp, 'Usa:', position(), ':usage;')"/>
          </xsl:if>
        </xsl:if>
        <xsl:if test="contains(translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jsharp')">
          <xsl:value-of select="concat($tabflagtemp, 'JS:', position(), ':jsharp;')"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <!-- the index of visual basic declearation tab in a code snippet-->
    <xsl:variable name="declarationIndex">
      <xsl:if test="contains($tabflag, 'declaration')">
        <xsl:value-of select="substring-after(substring-before($tabflag, ':declaration'), 'Dec:')"/>
      </xsl:if>
    </xsl:variable>
    <!-- the index of visual basic usage tab in a code snippet-->
    <xsl:variable name="usageIndex">
      <xsl:if test="contains($tabflag, 'usage')">
        <xsl:value-of select="substring-after(substring-before($tabflag, ':usage'), 'Usa:')"/>
      </xsl:if>
    </xsl:variable>
    <!-- the index of jsharp tab in a code snippet-->
    <xsl:variable name="jsharpIndex">
      <xsl:if test="contains($tabflag, 'jsharp')">
        <xsl:value-of select="substring-after(substring-before($tabflag, ':jsharp'), 'JS:')"/>
      </xsl:if>
    </xsl:variable>
    <!-- the text of combined visual basic if there are both vb declearation and vb usage tab-->
    <xsl:variable name="needToCombineVBTab">
      <xsl:choose>
        <xsl:when test="string-length($declarationIndex)>0 and string-length($usageIndex)>0">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="id">
        <xsl:value-of select="$owner-id"/>
      </xsl:attribute>
      <xsl:attribute name="class">OH_CodeSnippetContainer</xsl:attribute>

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_CodeSnippetContainerTabs</xsl:attribute>
        <xsl:attribute name="id">
          <xsl:value-of select="concat($owner-id, '_tabs')"/>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="$unrecognized='true'">
            <xsl:variable name="lang" select="./@Language"/>
            <xsl:variable name="displang" select="./@DisplayLanguage"/>
            <xsl:variable name="uselang">
              <xsl:if test="string-length($displang)>0">
                <xsl:value-of select="$displang"/>
              </xsl:if>
              <xsl:if test="string-length($displang)=0">
                <xsl:value-of select="$lang"/>
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="langLower" select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')"/>
            <xsl:choose>
              <xsl:when test="$langLower and not(contains($langLower, 'other')) and not(contains($langLower, 'none'))">
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:attribute name="class">OH_CodeSnippetContainerTabLeft</xsl:attribute>
                  <xsl:attribute name="id">
                    <xsl:value-of select="concat($owner-id, '_tabimgleft')"/>
                  </xsl:attribute>
                  <xsl:value-of select="''"></xsl:value-of>
                </xsl:element>
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:attribute name="id">
                    <xsl:value-of select="concat($owner-id, '_tab1')"/>
                  </xsl:attribute>
                  <xsl:attribute name="class">OH_CodeSnippetContainerTabSolo</xsl:attribute>
                  <xsl:attribute name="EnableCopyCode">
                    <xsl:value-of select="$snippets[1]/@EnableCopyCode"/>
                  </xsl:attribute>
                  <xsl:element name="a">
                    <xsl:value-of select="$uselang"/>
                  </xsl:element>
                </xsl:element>
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:attribute name="class">OH_CodeSnippetContainerTabRight</xsl:attribute>
                  <xsl:attribute name="id">
                    <xsl:value-of select="concat($owner-id, '_tabimgright')"/>
                  </xsl:attribute>
                  <xsl:value-of select="''"></xsl:value-of>
                </xsl:element>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text> </xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:element name="div" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_CodeSnippetContainerTabLeftActive</xsl:attribute>
              <xsl:attribute name="id">
                <xsl:value-of select="concat($owner-id, '_tabimgleft')"/>
              </xsl:attribute>
              <xsl:value-of select="''"></xsl:value-of>
            </xsl:element>
            <xsl:if test="$showLanTabs">
              <xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
                <xsl:variable name="uselang" select="."/>
                <xsl:variable name="uniqueLangIndex" select="position()" />
                <xsl:variable name="leadingVBTabIndex">
                  <xsl:if test="$declarationIndex&lt;$usageIndex">
                    <xsl:value-of select="$declarationIndex"/>
                  </xsl:if>
                  <xsl:if test="$declarationIndex&gt;$usageIndex">
                    <xsl:value-of select="$usageIndex"/>
                  </xsl:if>
                </xsl:variable>
                <xsl:variable name="followingVBTabIndex">
                  <xsl:if test="$declarationIndex=$leadingVBTabIndex">
                    <xsl:value-of select="$usageIndex"/>
                  </xsl:if>
                  <xsl:if test="$usageIndex=$leadingVBTabIndex">
                    <xsl:value-of select="$declarationIndex"/>
                  </xsl:if>
                </xsl:variable>
                <xsl:variable name="pos">
                  <xsl:variable name="postemp">
                    <xsl:for-each select="$snippets">
                      <xsl:variable name="lang" select="./@Language"/>
                      <xsl:variable name="displang" select="./@DisplayLanguage"/>
                      <xsl:variable name="uselangtemp">
                        <xsl:if test="string-length($displang)>0">
                          <xsl:value-of select="$displang"/>
                        </xsl:if>
                        <xsl:if test="string-length($displang)=0">
                          <xsl:value-of select="$lang"/>
                        </xsl:if>
                      </xsl:variable>
                      <xsl:if test="contains($uselangtemp, $uselang)">
                        <xsl:choose>
                          <xsl:when test="($needToCombineVBTab='true') and (position()=$leadingVBTabIndex)">
                            <xsl:value-of select="position()"/>
                          </xsl:when>
                          <xsl:when test="($needToCombineVBTab='true') and (position()=$followingVBTabIndex)" />
                          <xsl:otherwise>
                            <xsl:value-of select="position()"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:variable>
                  <xsl:if test="string-length($postemp)>0">
                    <xsl:value-of select="$postemp"/>
                  </xsl:if>
                  <xsl:if test="string-length($postemp)=0">
                    <xsl:value-of select="$uniqueLangIndex + $uniqueLangTabsSetCount"/>
                  </xsl:if>
                </xsl:variable>
                <xsl:variable name="majorLang">
                  <xsl:call-template name="isMajorLanguage">
                    <xsl:with-param name="lang" select="$snippets[position()=$pos]/@DisplayLanguage"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:attribute name="id">
                    <xsl:value-of select="concat($owner-id, '_tab', $uniqueLangIndex)"/>
                  </xsl:attribute>
                  <xsl:attribute name="class">
                    <xsl:choose>
                      <xsl:when test="($uniqueLangIndex=1 or $uniqueLangTabsSetCount=1) and $pos&lt;=$uniqueLangTabsSetCount">
                        <xsl:text>OH_CodeSnippetContainerTabActive</xsl:text>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:choose>
                          <xsl:when test="($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) and $majorLang='true'">
                            <xsl:text>OH_CodeSnippetContainerTab</xsl:text>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:if test="$uniqueLangIndex=1">
                              <xsl:text>OH_CodeSnippetContainerTabDisabled</xsl:text>
                            </xsl:if>
                            <xsl:if test="$uniqueLangIndex!=1">
                              <xsl:text>OH_CodeSnippetContainerTabDisabledNotFirst</xsl:text>
                            </xsl:if>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                  <xsl:attribute name="EnableCopyCode">
                    <xsl:value-of select="$snippets[1]/@EnableCopyCode"/>
                  </xsl:attribute>
                  <xsl:if test="not($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) or not($majorLang='true')">
                    <xsl:attribute name="disabled">
                      <xsl:text>true</xsl:text>
                    </xsl:attribute>
                  </xsl:if>
                  <xsl:element name="a" namespace="{$xhtml}">
                    <xsl:if test="($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) and $majorLang='true'">
                      <!-- EFW - Use onclick rather than href or HV 2.0 messes up the link -->
                      <xsl:attribute name="href">
                        <xsl:text>#</xsl:text>
                      </xsl:attribute>
                      <xsl:attribute name="onclick">
                        <xsl:text>javascript:ChangeTab('</xsl:text>
                        <xsl:value-of select="$owner-id"/>
                        <xsl:text>','</xsl:text>
                        <xsl:value-of select="$uselang"/>
                        <xsl:text>','</xsl:text>
                        <xsl:value-of select="$uniqueLangIndex" />
                        <xsl:text>','</xsl:text>
                        <xsl:value-of select="$uniqueLangTabsSetCount" />
                        <xsl:text>');return false;</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:choose>
                      <xsl:when test="$uselang='Visual Basic'">
                        <xsl:value-of select="'VB'"/>
                      </xsl:when>
                      <xsl:when test="$uselang='Visual C++'">
                        <xsl:value-of select="'C++'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$uselang"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:element>

                </xsl:element>
              </xsl:for-each>
            </xsl:if>

            <xsl:element name="div" namespace="{$xhtml}">
              <xsl:attribute name="class">
                <xsl:choose>
                  <xsl:when test="$uniqueLangTabsSetCount=1">
                    <xsl:text>OH_CodeSnippetContainerTabRightActive</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>OH_CodeSnippetContainerTabRight</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:attribute name="id">
                <xsl:value-of select="concat($owner-id, '_tabimgright')"/>
              </xsl:attribute>
              <xsl:value-of select="''"></xsl:value-of>
            </xsl:element>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="id">
          <xsl:value-of select="concat($owner-id, '_codecollection')"/>
        </xsl:attribute>
        <xsl:attribute name="class">OH_CodeSnippetContainerCodeCollection</xsl:attribute>
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="class">OH_CodeSnippetToolBar</xsl:attribute>

          <xsl:element name="div" namespace="{$xhtml}">
            <xsl:attribute name="class">OH_CodeSnippetToolBarText</xsl:attribute>
            <xsl:if test="$unrecognized='true'">
              <xsl:call-template name="code-snippet-menu">
                <xsl:with-param name="id" select="$owner-id"/>
                <xsl:with-param name="snippetCount" select="$uniqueLangTabsSetCount"/>
                <xsl:with-param name="enableCopyCode" select="'true'"/>
                <xsl:with-param name="showPrint" select="'true'"/>
              </xsl:call-template>
            </xsl:if>
            <xsl:if test="$unrecognized='false'">
              <xsl:call-template name="code-snippet-menu">
                <xsl:with-param name="id" select="$owner-id"/>
                <xsl:with-param name="snippetCount" select="$uniqueLangTabsSetCount"/>
                <xsl:with-param name="enableCopyCode" select="$snippets[1]/@EnableCopyCode"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:element>
        </xsl:element>

        <xsl:choose>
          <xsl:when test="$unrecognized='true'">
            <xsl:variable name="lang" select="./@Language"/>
            <xsl:variable name="displang" select="./@DisplayLanguage"/>
            <xsl:variable name="uselang">
              <xsl:if test="string-length($displang)>0">
                <xsl:value-of select="$displang"/>
              </xsl:if>
              <xsl:if test="string-length($displang)=0">
                <xsl:value-of select="$lang"/>
              </xsl:if>
            </xsl:variable>
            <xsl:call-template name="snippet-wrapper">
              <xsl:with-param name="id" select="$owner-id"/>
              <xsl:with-param name="pos" select="1"/>
              <xsl:with-param name="snippets" select="$snippets"/>
              <xsl:with-param name="snippet" select="$snippets[1]"/>
              <xsl:with-param name="ContainsMarkup" select="$snippets[1]/@ContainsMarkup"/>
              <xsl:with-param name="lang" select="$uselang"/>
              <xsl:with-param name="unrecognized" select="$unrecognized"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
              <xsl:variable name="uselang" select="."/>
              <xsl:variable name="uniqueLangIndex" select="position()" />
              <xsl:variable name="leadingVBTabIndex">
                <xsl:if test="$declarationIndex&lt;$usageIndex">
                  <xsl:value-of select="$declarationIndex"/>
                </xsl:if>
                <xsl:if test="$declarationIndex&gt;$usageIndex">
                  <xsl:value-of select="$usageIndex"/>
                </xsl:if>
              </xsl:variable>
              <xsl:variable name="followingVBTabIndex">
                <xsl:if test="$declarationIndex=$leadingVBTabIndex">
                  <xsl:value-of select="$usageIndex"/>
                </xsl:if>
                <xsl:if test="$usageIndex=$leadingVBTabIndex">
                  <xsl:value-of select="$declarationIndex"/>
                </xsl:if>
              </xsl:variable>
              <!-- dupe #1 - refactor-->
              <xsl:variable name="pos">
                <xsl:variable name="postemp">
                  <xsl:for-each select="$snippets">
                    <xsl:variable name="lang" select="./@Language"/>
                    <xsl:variable name="displang" select="./@DisplayLanguage"/>
                    <xsl:variable name="uselangtemp">
                      <xsl:if test="string-length($displang)>0">
                        <xsl:value-of select="$displang"/>
                      </xsl:if>
                      <xsl:if test="string-length($displang)=0">
                        <xsl:value-of select="$lang"/>
                      </xsl:if>
                    </xsl:variable>
                    <xsl:if test="contains($uselangtemp, $uselang)">
                      <xsl:choose>
                        <xsl:when test="($needToCombineVBTab='true') and (position()=$leadingVBTabIndex)">
                          <xsl:value-of select="position()"/>
                        </xsl:when>
                        <xsl:when test="($needToCombineVBTab='true') and (position()=$followingVBTabIndex)"/>
                        <xsl:otherwise>
                          <xsl:value-of select="position()"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:variable>
                <xsl:if test="string-length($postemp)>0">
                  <xsl:value-of select="$postemp"/>
                </xsl:if>
                <xsl:if test="string-length($postemp)=0">
                  <xsl:value-of select="$uniqueLangIndex + $uniqueLangTabsSetCount"/>
                </xsl:if>
              </xsl:variable>
              <xsl:variable name="lang" select="$snippets[position()=$pos]/@Language"/>
              <xsl:variable name="displang" select="$snippets[position()=$pos]/@DisplayLanguage"/>
              <xsl:call-template name="snippet-wrapper">
                <xsl:with-param name="id" select="$owner-id"/>
                <xsl:with-param name="pos" select="$pos"/>
                <xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
                <xsl:with-param name="snippets" select="$snippets"/>
                <xsl:with-param name="snippet" select="$snippets[position()=$pos]"/>
                <xsl:with-param name="ContainsMarkup" select="$snippets[position()=$pos]/@ContainsMarkup"/>
                <xsl:with-param name="lang" select="$lang"/>
                <xsl:with-param name="needToCombineVBTab" select="$needToCombineVBTab"/>
                <xsl:with-param name="declarationIndex" select="$declarationIndex"/>
                <xsl:with-param name="usageIndex" select="$usageIndex"/>
                <xsl:with-param name="unrecognized" select="$unrecognized"/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
    </xsl:element>

    <xsl:element name="script" namespace="{$xhtml}">
      <xsl:text>addSpecificTextLanguageTagSet('</xsl:text>
      <xsl:value-of select="$owner-id"/>
      <xsl:text>');</xsl:text>
    </xsl:element>
  </xsl:template>






	<!-- ============================================================================================
	Override code grouping to consistently use the 'isMajorLanguage' template and to better handle
	multiple groups.

	Snippets are rendered as a group only when:
	- They are descendants of mtps:CollapsibleArea or children of a div whose id starts with 'snippetGroup'
	- They are contiguous (no intervening elements that are not snippets)
	- They are unique (duplicates are split into groups)
	- They pass the 'isMajorLanguage' test 
	============================================================================================= -->

	<xsl:template match="mtps:CodeSnippet"
								priority ="2"
								name="codeSnippetOverride">
		<xsl:choose>
			<xsl:when test="ancestor::mtps:CollapsibleArea[count(descendant::mtps:CodeSnippet) > 1] or parent::xhtml:div[starts-with(@id,'snippetGroup') and (count(mtps:CodeSnippet) > 1)]">
				<xsl:variable name="v_currentId">
					<xsl:value-of select="generate-id(.)"/>
				</xsl:variable>
				<xsl:variable name="v_prevPosition">
					<xsl:for-each select="parent::*/child::*">
						<xsl:if test="generate-id(.)=$v_currentId">
							<xsl:number value="position()-1"/>
						</xsl:if>
					</xsl:for-each>
				</xsl:variable>

				<xsl:choose>
					<xsl:when test="name(parent::*/child::*[position()=$v_prevPosition])=name()">
						<!--<xsl:comment xml:space="preserve">skip [<xsl:value-of select="$v_prevPosition + 1" />] [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
					</xsl:when>
					<xsl:when test="following-sibling::*[1]/self::mtps:CodeSnippet">
						<xsl:call-template name="codeSnippetGroup">
							<xsl:with-param name="codeSnippets"
															select=". | following-sibling::mtps:CodeSnippet[not(preceding-sibling::*[generate-id(.)=generate-id((current()/following-sibling::*[not(self::mtps:CodeSnippet)])[1])])]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<!--<xsl:comment xml:space="preserve">standalonesnippet(a) [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
						<xsl:call-template name="standalonesnippet"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<!--<xsl:comment xml:space="preserve">standalonesnippet(b) [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
				<xsl:call-template name="standalonesnippet"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	This template receives a contiguous set of snippets and splits any duplicates into separate sets.
	============================================================================================= -->
	<xsl:template name="codeSnippetGroup">
		<xsl:param name="codeSnippets"/>
		<xsl:param name="codeSnippetCount"
							 select="count($codeSnippets)"/>

		<!--<xsl:variable name="v_languages">
			<xsl:for-each select="$codeSnippets">
				<xsl:value-of select="concat(' [',@Language,'][',@DisplayLanguage,']')" />
			</xsl:for-each>
		</xsl:variable>
		<xsl:comment xml:space="preserve">codeSnippetGroup [<xsl:value-of select="$codeSnippetCount" />]<xsl:value-of select="$v_languages" /></xsl:comment>-->

		<xsl:choose>
			<xsl:when test="$codeSnippetCount = 1">
				<xsl:for-each select="$codeSnippets">
					<!--<xsl:comment xml:space="preserve">standalonesnippet(c) [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
					<xsl:call-template name="standalonesnippet"/>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>

				<!-- Must use a copy of the snippets for this check so that preceding-sibling applies only to THESE snippets. -->
				<!-- Otherwise, preceding-sibling will apply to all snippets with the same parent in the source document. -->
				<xsl:variable name="v_snippetsCopy">
					<xsl:copy-of select="$codeSnippets"/>
				</xsl:variable>
				<xsl:variable name="v_duplicates">
					<xsl:for-each select="msxsl:node-set($v_snippetsCopy)/*">
						<xsl:if test="preceding-sibling::*[@Language = current()/@Language]">
							<xsl:value-of select="concat(position(),';')"/>
						</xsl:if>
					</xsl:for-each>
				</xsl:variable>

				<xsl:choose>
					<xsl:when test="string($v_duplicates)=''">
						<xsl:call-template name="codeSnippetGroupUnique">
							<xsl:with-param name="codeSnippets"
															select="$codeSnippets"/>
							<xsl:with-param name="codeSnippetCount"
															select="$codeSnippetCount"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="v_dupPosition">
							<xsl:value-of select="substring-before(string($v_duplicates),';')"/>
						</xsl:variable>
						<xsl:call-template name="codeSnippetGroup">
							<xsl:with-param name="codeSnippets"
															select="$codeSnippets[position() &lt; $v_dupPosition]"/>
						</xsl:call-template>
						<xsl:call-template name="codeSnippetGroup">
							<xsl:with-param name="codeSnippets"
															select="$codeSnippets[not(position() &lt; $v_dupPosition)]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	This template receives a contiguous, unique set of snippets and checks for 'isMajorLanguage'.
	- If all snippets are for a major language, they are rendered as a group
	- If all snippets are NOT for a major language, they are rendered standalone
	- Otherwise the snippets are separated into consistent sets
	============================================================================================= -->
	<xsl:template name="codeSnippetGroupUnique">
		<xsl:param name="codeSnippets"/>
		<xsl:param name="codeSnippetCount"
							 select="count($codeSnippets)"/>

		<!--<xsl:variable name="v_languages">
			<xsl:for-each select="$codeSnippets">
				<xsl:value-of select="concat(' [',@Language,'][',@DisplayLanguage,']')" />
			</xsl:for-each>
		</xsl:variable>
		<xsl:comment xml:space="preserve">codeSnippetGroupUnique [<xsl:value-of select="$codeSnippetCount" />]<xsl:value-of select="$v_languages" /></xsl:comment>-->

		<xsl:choose>
			<xsl:when test="$codeSnippetCount = 1">
				<xsl:for-each select="$codeSnippets">
					<!--<xsl:comment xml:space="preserve">standalonesnippet(d) [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
					<xsl:call-template name="standalonesnippet"/>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="v_allMajorLanguage">
					<xsl:for-each select="$codeSnippets">
						<xsl:variable name="v_isMajorLanguage">
							<xsl:call-template name="isMajorLanguage">
								<xsl:with-param name="lang"
																select="@DisplayLanguage"/>
							</xsl:call-template>
						</xsl:variable>
						<value>
							<xsl:choose>
								<xsl:when test="$v_isMajorLanguage='true'">
									<xsl:value-of select="'true'"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="'false'"/>
								</xsl:otherwise>
							</xsl:choose>
						</value>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="v_allMajorLanguageSame">
					<xsl:for-each select="msxsl:node-set($v_allMajorLanguage)/*">
						<xsl:if test="preceding-sibling::*[text() != current()/text()]">
							<xsl:value-of select="concat(position(),';')"/>
						</xsl:if>
					</xsl:for-each>
				</xsl:variable>

				<xsl:choose>
					<xsl:when test="string($v_allMajorLanguageSame)='' and not(contains($v_allMajorLanguage,'false'))">
						<!--<xsl:comment xml:space="preserve">renderSnippet [<xsl:value-of select="$codeSnippetCount"/>]</xsl:comment>
						<xsl:for-each select="$codeSnippets">
							<xsl:comment xml:space="preserve">  [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>
						</xsl:for-each>-->
						<xsl:call-template name="renderSnippet">
							<xsl:with-param name="snippetCount"
															select="$codeSnippetCount"/>
							<xsl:with-param name="snippets"
															select="$codeSnippets"/>
							<xsl:with-param name="showLanTabs"
															select="true()" />
							<xsl:with-param name="unrecognized"
															select="'false'" />
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="string($v_allMajorLanguageSame)='' and contains($v_allMajorLanguage,'false')">
						<xsl:for-each select="$codeSnippets">
							<!--<xsl:comment xml:space="preserve">standalonesnippet(e) [<xsl:value-of select="@Language" />] [<xsl:value-of select="@DisplayLanguage" />]</xsl:comment>-->
							<xsl:call-template name="standalonesnippet"/>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="v_dupPosition">
							<xsl:value-of select="substring-before(string($v_allMajorLanguageSame),';')"/>
						</xsl:variable>
						<xsl:call-template name="codeSnippetGroupUnique">
							<xsl:with-param name="codeSnippets"
															select="$codeSnippets[position() &lt; $v_dupPosition]"/>
						</xsl:call-template>
						<xsl:call-template name="codeSnippetGroupUnique">
							<xsl:with-param name="codeSnippets"
															select="$codeSnippets[not(position() &lt; $v_dupPosition)]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Override code formatting to allow for pre-formatted code
	============================================================================================= -->

	<xsl:template name="renderCodeDiv">
		<xsl:param name="id"/>
		<xsl:param name="pos"/>
		<xsl:param name="uniqueLangIndex"
							 select="1"/>
		<xsl:param name="lang"/>
		<xsl:param name="plainCode"
							 select="'false'"/>
		<xsl:param name="snippetCode"/>
		<xsl:param name="unrecognized"
							 select="'false'"/>
		<xsl:param name="ContainsMarkup" />
		<!--<xsl:comment xml:space="preserve">renderCodeDiv [<xsl:value-of select="$id" />] [<xsl:value-of select="$pos" />] [<xsl:value-of select="$uniqueLangIndex" />] [<xsl:value-of select="$lang" />] [<xsl:value-of select="$unrecognized" />] [<xsl:value-of select="$ContainsMarkup" />]</xsl:comment>-->
		<xsl:element name="div"
								 namespace="{$xhtml}">
			<xsl:attribute name="id">
				<xsl:value-of select="$id"/>
			</xsl:attribute>
			<xsl:attribute name="class">OH_CodeSnippetContainerCode</xsl:attribute>
			<xsl:attribute name="style">
				<xsl:choose>
					<xsl:when test="$plainCode='true'">
						<xsl:text>display: none</xsl:text>
					</xsl:when>
					<xsl:when test="$uniqueLangIndex=1">
						<xsl:text>display: block</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>display: none</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:variable name="spacedSnippetCode">
				<xsl:apply-templates select="msxsl:node-set($snippetCode)/node()"
														 mode="codeSpacing"/>
			</xsl:variable>
			<xsl:choose>
				<!-- If the snippet contains any elements, it's pre-formatted -->
				<xsl:when test="msxsl:node-set($spacedSnippetCode)/*">
					<xsl:choose>
						<xsl:when test="$plainCode='true'">
							<xsl:element name="pre"
													 namespace="{$xhtml}">
								<xsl:choose>
									<xsl:when test="$plain-remove-element!='' or $plain-remove-id!='' or $plain-remove-class!=''">
										<xsl:variable name="plainSnippetCode">
											<xsl:apply-templates select="msxsl:node-set($spacedSnippetCode)"
																					 mode="plainCode"/>
										</xsl:variable>
										<xsl:value-of select="cs:plainCode($plainSnippetCode)"
																	disable-output-escaping="yes"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="cs:plainCode($spacedSnippetCode)"
																	disable-output-escaping="yes"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
						</xsl:when>
						<xsl:otherwise>
							<xsl:element name="pre"
													 namespace="{$xhtml}">
								<xsl:copy-of select="$spacedSnippetCode"/>
							</xsl:element>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<!-- Otherwise it's only text -->
				<xsl:otherwise>
					<xsl:choose>
						<xsl:when test="$plainCode='true'">
							<xsl:element name="pre"
													 namespace="{$xhtml}">
								<xsl:value-of select="cs:ConvertWhiteSpace(cs:plainCode($snippetCode))"
															disable-output-escaping="yes"/>
							</xsl:element>
						</xsl:when>
						<xsl:when test="$ContainsMarkup='true'">
							<xsl:element name="pre"
													 namespace="{$xhtml}">
								<xsl:copy-of select="cs:ConvertWhiteSpace($snippetCode)"/>
							</xsl:element>
						</xsl:when>
						<xsl:otherwise>
							<xsl:element name="pre"
													 namespace="{$xhtml}">
								<xsl:value-of select="cs:ConvertWhiteSpace(cs:test($snippetCode, $lang, 'en-us'))"
															disable-output-escaping="yes"/>
							</xsl:element>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="*"
								mode="codeSpacing"
								name="codeSpacingElement">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates mode="codeSpacing"/>
			<xsl:if test="not(node()) and not(self::xhtml:br) and not(self::xhtml:hr)">
				<xsl:value-of select="''"/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="xhtml:pre"
								mode="codeSpacing"
								name="codeSpacingContainer">
		<xsl:apply-templates mode="codeSpacing"/>
	</xsl:template>

	<xsl:template match="text()"
								mode="codeSpacing"
								name="codeSpacingText">
		<xsl:choose>
			<xsl:when test=".=' ' or .='&#160;'">
				<xsl:value-of select="'&#160;'"/>
			</xsl:when>
			<xsl:when test="normalize-space(.)='' and contains(.,'&#10;')">
				<xsl:value-of select="concat('&#160;','&#10;',substring-after(translate(.,' &#13;','&#160;'),'&#10;'))"/>
			</xsl:when>
			<xsl:when test="normalize-space(.)='' and contains(.,'&#13;')">
				<xsl:value-of select="concat('&#160;','&#10;',substring-after(translate(.,' ','&#160;'),'&#13;'))"/>
			</xsl:when>
			<xsl:when test=".!='' and normalize-space(.)=''">
				<xsl:value-of select="translate(.,' ','&#160;')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="."/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="*"
								mode="plainCode"
								name="plainCodeElement">
		<xsl:choose>
			<xsl:when test="contains($plain-remove-element,name()) or (@id and contains($plain-remove-id,@id)) or (@class and contains($plain-remove-class,@class))">
				<!--<xsl:comment xml:space="preserve">skip[<xsl:value-of select="."/>]</xsl:comment>-->
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy>
					<xsl:apply-templates select="@*"/>
					<xsl:apply-templates mode="plainCode"/>
				</xsl:copy>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="text()"
								mode="plainCode"
								name="plainCodeText">
		<xsl:call-template name="codeSpacingText"/>
	</xsl:template>

	<!-- ============================================================================================
	Accumulate default tabs set
	============================================================================================= -->

	<!-- To customize the starter set of grouped code languages, update this list -->
	<xsl:variable name="groupedLanguages">
		<value>C#</value>
		<value>Visual Basic</value>
		<value>Visual C++</value>
		<value>F#</value>
	</xsl:variable>
	<!-- To customize the code languages that should NOT be grouped, update this list -->
	<xsl:variable name="separateLanguages">
		<value>JavaScript</value>
		<value>JScript</value>
		<value>J#</value>
		<value>XML</value>
		<value>XAML</value>
		<value>HTML</value>
		<value>ASP.NET</value>
	</xsl:variable>

	<xsl:variable name="devLanguages">
		<xsl:for-each select="/xhtml:html/xhtml:head/xhtml:xml/xhtml:list[@id='BrandingLanguages']/xhtml:value">
			<value>
				<xsl:value-of select="text()"/>
			</value>
		</xsl:for-each>
	</xsl:variable>
	<xsl:variable name="syntaxLanguages">
		<xsl:for-each select="/xhtml:html/xhtml:head/xhtml:xml/xhtml:list[@id='BrandingSyntaxLanguages']/xhtml:value">
			<value>
				<xsl:value-of select="text()"/>
			</value>
		</xsl:for-each>
	</xsl:variable>

	<xsl:variable name="uniqueLangTabsSet">
		<xsl:for-each select="msxsl:node-set($groupedLanguages)/value">
			<xsl:choose>
				<xsl:when test="msxsl:node-set($separateLanguages)/value[current()/text()=text()]"/>
				<xsl:otherwise>
					<value>
						<xsl:value-of select="text()"/>
					</value>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
		<xsl:for-each select="msxsl:node-set($devLanguages)/value">
			<xsl:choose>
				<xsl:when test="msxsl:node-set($separateLanguages)/value[current()/text()=text()]"/>
				<xsl:when test="msxsl:node-set($groupedLanguages)/value[current()/text()=text()]"/>
				<xsl:otherwise>
					<value>
						<xsl:value-of select="text()"/>
					</value>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
		<xsl:for-each select="msxsl:node-set($syntaxLanguages)/value">
			<xsl:choose>
				<xsl:when test="msxsl:node-set($separateLanguages)/value[current()/text()=text()]"/>
				<xsl:when test="msxsl:node-set($groupedLanguages)/value[current()/text()=text()]"/>
				<xsl:when test="msxsl:node-set($devLanguages)/value[current()/text()=text()]"/>
				<xsl:otherwise>
					<value>
						<xsl:value-of select="text()"/>
					</value>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:variable>
	<xsl:variable name="uniqueLangTabsSetCount"
								select="count(msxsl:node-set($uniqueLangTabsSet)/value)"/>

	<xsl:template match="/xhtml:html/xhtml:head/xhtml:xml/xhtml:list[@id='BrandingLanguages']"/>
	<xsl:template match="/xhtml:html/xhtml:head/xhtml:xml/xhtml:list[@id='BrandingSyntaxLanguages']"/>

	<!-- ============================================================================================
	Override of isMajorLanguage

	The default implementation of this template uses the "contains" function which means that if
	the display language "contains" one of the major language names it matches - even if it's not 
	actually the same.  The intention is to match Visual Basic, Visual Basic Declaration and 
	Visual Basic Usage, but it is applied too broadly.  This implementation is more accurate.
	============================================================================================= -->

	<xsl:template name="isMajorLanguage">
		<xsl:param name="lang"/>
		<xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
			<xsl:choose>
				<xsl:when test="$lang=.">
					<xsl:value-of select="'true'"/>
				</xsl:when>
				<xsl:when test="contains($lang,.)">
					<xsl:variable name="loweredLang"
												select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
					<xsl:if test="contains($loweredLang,'declaration') or contains($loweredLang,'usage')">
						<xsl:value-of select="'true'"/>
					</xsl:if>
				</xsl:when>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
