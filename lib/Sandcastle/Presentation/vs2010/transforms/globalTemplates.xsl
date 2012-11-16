<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
								xmlns:mshelp="http://msdn.microsoft.com/mshelp"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:xlink="http://www.w3.org/1999/xlink"
	>
	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="metadata">false</xsl:param>
	<xsl:param name="languages">false</xsl:param>
	<xsl:param name="minimal-spacing">code;alert;listItem;table;tableHeader;definedTerm;definition;</xsl:param>

	<!-- ============================================================================================
	Globals
	============================================================================================= -->

	<xsl:variable name="xhtml"
								select="'http://www.w3.org/1999/xhtml'"/>
	<xsl:variable name="ddue"
								select="'http://ddue.schemas.microsoft.com/authoring/2003/5'"/>
	<xsl:variable name="mtps"
								select="'http://msdn2.microsoft.com/mtps'"/>
	<xsl:variable name="g_allUpperCaseLetters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
	<xsl:variable name="g_allLowerCaseLetters">abcdefghijklmnopqrstuvwxyz</xsl:variable>

	<!-- ============================================================================================
	String formatting
	============================================================================================= -->

	<!-- Gets the substring after the last occurence of a period in a given string -->
	<xsl:template name="t_getTrimmedLastPeriod">
		<xsl:param name="p_string" />

		<xsl:choose>
			<xsl:when test="contains($p_string, '.')">
				<xsl:call-template name="t_getTrimmedLastPeriod">
					<xsl:with-param name="p_string"
													select="substring-after($p_string, '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$p_string" />
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template name="t_getTrimmedAtPeriod">
		<xsl:param name="p_string" />

		<xsl:variable name="v_trimmedString"
									select="substring(normalize-space($p_string), 1, 256)" />
		<xsl:choose>
			<xsl:when test="normalize-space($p_string) != $v_trimmedString">
				<xsl:choose>
					<xsl:when test="not(contains($v_trimmedString, '.'))">
						<xsl:value-of select="$v_trimmedString"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="t_getSubstringAndLastPeriod">
							<xsl:with-param name="p_string"
															select="$v_trimmedString" />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="normalize-space($p_string)"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_getSubstringAndLastPeriod">
		<xsl:param name="p_string" />

		<xsl:if test="contains($p_string, '.')">
			<xsl:variable name="v_after"
										select="substring-after($p_string, '.')" />
			<xsl:value-of select="concat(substring-before($p_string, '.'),'.')" />
			<xsl:if test="contains($v_after, '.')">
				<xsl:call-template name="t_getSubstringAndLastPeriod">
					<xsl:with-param name="p_string"
													select="$v_after" />
				</xsl:call-template>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- indent by 2*n spaces -->
	<xsl:template name="t_putIndent">
		<xsl:param name="p_count" />
		<xsl:if test="$p_count &gt; 1">
			<xsl:text>&#160;&#160;</xsl:text>
			<xsl:call-template name="t_putIndent">
				<xsl:with-param name="p_count"
												select="$p_count - 1" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Minimal spacing tests

	The minimal spacing feature skips the <p></p> translation for the first <para> element in
	selected parent elements (eg. alerts, list items, table entries, definition items, etc.)

	These templates check if minimal spacing should be applied in a specified context. 

	The first check determines if minimal spacing is enabled for the current context by matching
	the $p_spacingType parameter agains the $minimal-spacing global parameter.

	The next check traverses up the element tree and checks the @minimal-spacing attribute at each
	level.
	- If any element has a @minimal-spacing attribute of 'false', this disables the feature
	  for that element and its descendants.
	- If any element has a @minimal-spacing attribute of 'true', this enables the feature
	  for that element and its descendants.
	- If and element does not have a @minimal-spacing attribute, it inherits its parent's setting.
	- The root is assumed to have the feature enabled.
	
	The last check uses the $p_parentLevel pameter to traverse up the element tree and then
	back down again to check each descendant.  The purpose is to determine if any	descendant
	element satisfies conditions that would block the minimal spacing feature, as follows:
	- If an element contains more than one <para> element, the feature is blocked.
	- If an element contains any element other than <para>, the feature is blocked.
	- However, if the element has @minimal-spacing='true' the feature is not blocked, no matter
	  what the element's contents.

	Note - for the <code> element, minimal spacing does not apply to <para> elements.  Instead, 
	it causes all leading and trailing blank lines to be removed.
	============================================================================================= -->

	<xsl:template name="t_checkMinimalSpacing">
		<xsl:param name="p_spacingType"
							 select="'any'"/>
		<xsl:param name="p_parentLevel"
							 select="0"/>
		<xsl:if test="$p_spacingType='any' or contains($minimal-spacing,$p_spacingType)">
			<xsl:variable name="v_minimalSpacing">
				<xsl:call-template name="t_getMinimalSpacing"/>
			</xsl:variable>
			<xsl:if test="$v_minimalSpacing='' or starts-with($v_minimalSpacing,'true')">
				<xsl:variable name="v_levelCheck">
					<xsl:call-template name="t_checkMinimalParent">
						<xsl:with-param name="p_parentLevel"
														select="$p_parentLevel"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$v_levelCheck=''">
						<xsl:value-of select="'true'"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$v_levelCheck"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_getMinimalSpacing">
		<xsl:choose>
			<xsl:when test="@minimal-spacing">
				<xsl:value-of select="@minimal-spacing"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="parent::*">
					<xsl:call-template name="t_getMinimalSpacing"/>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_checkMinimalParent">
		<xsl:param name="p_parentLevel"/>
		<xsl:param name="p_childLevel"
							 select="0"/>
		<xsl:choose>
			<xsl:when test="$p_parentLevel = 1">
				<xsl:for-each select="parent::*">
					<xsl:call-template name="t_checkMinimalChild">
						<xsl:with-param name="p_childLevel"
														select="$p_childLevel"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
			<xsl:when test="$p_parentLevel &gt; 1">
				<xsl:for-each select="parent::*">
					<xsl:call-template name="t_checkMinimalParent">
						<xsl:with-param name="p_parentLevel"
														select="$p_parentLevel - 1"/>
						<xsl:with-param name="p_childLevel"
														select="$p_childLevel + 1"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_checkMinimalChild">
		<xsl:param name="p_childLevel"/>
		<xsl:choose>
			<xsl:when test="$p_childLevel = 0">
				<xsl:if test="not(@minimal-spacing) or (@minimal-spacing!='true')">
					<xsl:choose>
						<xsl:when test="child::*[local-name()!='para']">
							<xsl:value-of select="'sibling!=para'"/>
						</xsl:when>
						<xsl:when test="(child::*[local-name()='para'])[2]">
							<xsl:value-of select="'sibling para[2]'"/>
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</xsl:when>
			<xsl:when test="$p_childLevel &gt; 0">
				<xsl:for-each select="child::*">
					<xsl:call-template name="t_checkMinimalChild">
						<xsl:with-param name="p_childLevel"
														select="$p_childLevel - 1"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	LanguageSpecific text

	NOTE - the MSHCComponent recognizes these bits and post-processes them into the format used
	       by the MS Help Viewer.
	============================================================================================= -->

	<xsl:template name="t_decoratedNameSep">
		<span class="languageSpecificText">
			<span class="cs">.</span>
			<span class="vb">.</span>
			<span class="cpp">::</span>
			<span class="fs">.</span>
			<span class="nu">.</span>
		</span>
	</xsl:template>

	<xsl:template name="t_nullKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">null</span>
						<span class="vb">Nothing</span>
						<span class="cpp">nullptr</span>
						<span class="fs">unit</span>
						<span class="nu">null</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_nullKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_staticKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">static</span>
						<span class="vb">Shared</span>
						<span class="cpp">static</span>
						<span class="fs">static</span>
						<span class="nu">static</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_staticKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_virtualKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">virtual</span>
						<span class="vb">Overridable</span>
						<span class="cpp">virtual</span>
						<span class="fs">virtual</span>
						<span class="nu">virtual</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_virtualKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_trueKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">true</span>
						<span class="vb">True</span>
						<span class="cpp">true</span>
						<span class="fs">true</span>
						<span class="nu">true</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_trueKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_falseKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">false</span>
						<span class="vb">False</span>
						<span class="cpp">false</span>
						<span class="fs">false</span>
						<span class="nu">false</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_falseKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_abstractKeyword">
		<xsl:param name="p_syntaxKeyword"
							 select="''"/>
		<xsl:choose>
			<xsl:when test="$p_syntaxKeyword">
				<span class="keyword">
					<span class="languageSpecificText">
						<span class="cs">abstract</span>
						<span class="vb">MustInherit</span>
						<span class="cpp">abstract</span>
						<span class="fs">abstract</span>
						<span class="nu">abstract</span>
					</span>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<include item="devlang_abstractKeyword"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_inKeyword">
		<span class="keyword">
			<span class="languageSpecificText">
				<span class="cs">in</span>
				<span class="vb">In</span>
				<span class="cpp">in</span>
				<span class="fs"></span>
				<span class="nu">in</span>
			</span>
		</span>
	</xsl:template>

	<xsl:template name="t_outKeyword">
		<span class="keyword">
			<span class="languageSpecificText">
				<span class="cs">out</span>
				<span class="vb">Out</span>
				<span class="cpp">out</span>
				<span class="fs"></span>
				<span class="nu">out</span>
			</span>
		</span>
	</xsl:template>

	<!-- ============================================================================================
	Common metadata
	============================================================================================= -->

	<xsl:template name="t_insertMetadata">
		<xsl:element name="xml">
			<xsl:attribute name="id">BrandingData</xsl:attribute>
			<string id="BrandingProductTitle">
				<include item="productTitle"/>
			</string>

			<string id="BrandingCopyrightText">
				<include item="copyright_text"/>
			</string>
			<string id="BrandingCopyrightLink">
				<include item="copyright_link"/>
			</string>
			<string id="BrandingCopyrightInfo">
				<include item="copyright_info"/>
			</string>
			<string id="BrandingHeader">
				<include item="boilerplate_pageTitle">
					<parameter>
						<xsl:call-template name="t_topicTitleDecorated"/>
					</parameter>
				</include>
			</string>

			<string id="BrandingFooterText">
				<include item="footer_text"/>
			</string>
			<string id="BrandingFeedbackAlias">
				<include item="fb_alias"/>
			</string>
			<string id="BrandingFeedbackSubject">
				<include item="fb_product"/>
			</string>
			<string id="BrandingFeedbackText">
				<include item="fb_text"/>
			</string>
			<string id="BrandingFeedbackFooterTo">
				<include item="fb_footer_to"/>
			</string>
			<string id="BrandingFeedbackFooterText">
				<include item="fb_footer_text"/>
			</string>
			<string id="BrandingFeedbackFooterTextTo">
				<include item="fb_footer_text_to"/>
			</string>
			<string id="BrandingFeedbackBody">
				<include item="fb_body"/>
			</string>

			<xsl:if test="$languages/language">
				<list id="BrandingLanguages">
					<xsl:for-each select="$languages/language">
						<xsl:variable name="v_devlangId">
							<xsl:call-template name="t_codeLangTitleId">
								<xsl:with-param name="p_codeLang"
																select="@name"/>
							</xsl:call-template>
						</xsl:variable>
						<xsl:if test="$v_devlangId!=''">
							<xsl:element name="value">
								<include item="{$v_devlangId}"/>
							</xsl:element>
						</xsl:if>
					</xsl:for-each>
				</list>
			</xsl:if>
			<xsl:if test="/document/syntax/div[@codeLanguage]">
				<list id="BrandingSyntaxLanguages">
					<xsl:for-each select="/document/syntax/div[@codeLanguage and not(div[@class='nonXamlAssemblyBoilerplate'])]">
						<xsl:variable name="v_codeLangId">
							<xsl:call-template name="t_codeLangTitleId">
								<xsl:with-param name="p_codeLang"
																select="@codeLanguage"/>
							</xsl:call-template>
						</xsl:variable>
						<xsl:if test="$v_codeLangId!=''">
							<xsl:element name="value">
								<include item="{$v_codeLangId}"/>
							</xsl:element>
						</xsl:if>
					</xsl:for-each>
				</list>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="t_insertNoIndexNoFollow">
		<xsl:if test="/document/metadata/attribute[@name='NoSearch']">
			<META NAME="ROBOTS"
						CONTENT="NOINDEX, NOFOLLOW" />
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Running header
	============================================================================================= -->

	<xsl:template name="t_bodyTitle">
		<div class="majorTitle">
			<!---->
		</div>
		<div class="title">
			<include item="boilerplate_pageTitle">
				<parameter>
					<xsl:call-template name="t_topicTitleDecorated"/>
				</parameter>
			</include>
		</div>

		<!-- This table is required for runningHeader/logo placement. It will be used and removed during branding. -->
		<!-- It basically fakes the formatting of older presentation styles so common logic can be used in -->
		<!-- the PostTransformComponent. -->
		<table id="topTable"
					 style="display:none">
			<tr>
				<td id="titleColumn"></td>
			</tr>
			<tr>
				<td id="runningHeaderColumn"
						align="right">
					<xsl:call-template name="t_runningHeader" />
				</td>
			</tr>
		</table>
	</xsl:template>

	<!-- ============================================================================================
	SeeAlso links
	============================================================================================= -->

	<xsl:template name="t_autogenSeeAlsoLinks">

		<!-- a link to the containing type on all list and member topics -->
		<xsl:if test="($g_apiTopicGroup='member' or $g_apiTopicGroup='list')">
			<xsl:variable name="v_typeTopicId">
				<xsl:choose>
					<xsl:when test="/document/reference/topicdata/@typeTopicId">
						<xsl:value-of select="/document/reference/topicdata/@typeTopicId"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="/document/reference/containers/type/@api"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<div class="seeAlsoStyle">
				<referenceLink target="{$v_typeTopicId}"
											 display-target="format">
					<include item="boilerplate_seeAlsoTypeLink">
						<parameter>{0}</parameter>
						<parameter>
							<xsl:choose>
								<xsl:when test="/document/reference/topicdata/@typeTopicId">
									<xsl:value-of select="/document/reference/apidata/@subgroup"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="/document/reference/containers/type/apidata/@subgroup"/>
								</xsl:otherwise>
							</xsl:choose>
						</parameter>
					</include>
				</referenceLink>
			</div>
		</xsl:if>

		<!-- a link to the type's All Members list -->
		<xsl:variable name="v_allMembersId">
			<xsl:choose>
				<xsl:when test="/document/reference/topicdata/@allMembersTopicId">
					<xsl:value-of select="/document/reference/topicdata/@allMembersTopicId"/>
				</xsl:when>
				<xsl:when test="$g_apiTopicGroup='member' or ($g_apiTopicGroup='list' and $g_apiTopicSubGroup='overload')">
					<xsl:value-of select="/document/reference/containers/type/topicdata/@allMembersTopicId"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="normalize-space($v_allMembersId) and not($v_allMembersId=$key)">
			<div class="seeAlsoStyle">
				<referenceLink target="{$v_allMembersId}"
											 display-target="format">
					<include item="boilerplate_seeAlsoMembersLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</div>
		</xsl:if>

		<!-- a link to the overload topic -->
		<xsl:variable name="v_overloadId">
			<xsl:value-of select="/document/reference/memberdata/@overload"/>
		</xsl:variable>
		<xsl:if test="normalize-space($v_overloadId)">
			<div class="seeAlsoStyle">
				<referenceLink target="{$v_overloadId}"
											 display-target="format"
											 show-parameters="false">
					<include item="boilerplate_seeAlsoOverloadLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</div>
		</xsl:if>

		<!-- a link to the namespace topic -->
		<xsl:variable name="v_namespaceId">
			<xsl:value-of select="/document/reference/containers/namespace/@api"/>
		</xsl:variable>
		<xsl:if test="normalize-space($v_namespaceId)">
			<div class="seeAlsoStyle">
				<referenceLink target="{$v_namespaceId}"
											 display-target="format">
					<include item="boilerplate_seeAlsoNamespaceLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</div>
		</xsl:if>

	</xsl:template>

	<!-- ============================================================================================
	Section headers
	============================================================================================= -->

	<xsl:template name="t_putSection">
		<xsl:param name="p_title" />
		<xsl:param name="p_content" />
		<xsl:param name="p_toplink"
							 select="false()" />

		<xsl:element name="mtps:CollapsibleArea">
			<xsl:attribute name="runat">
				<xsl:value-of select="'server'" />
			</xsl:attribute>
			<xsl:attribute name="Title">
				<xsl:value-of select="$p_title" />
			</xsl:attribute>
			<xsl:element name="xml">
				<xsl:element name="string">
					<xsl:attribute name="id">
						<xsl:value-of select="'Title'" />
					</xsl:attribute>
					<xsl:copy-of select="$p_title" />
				</xsl:element>
			</xsl:element>

			<xsl:copy-of select="$p_content" />

			<xsl:if test="boolean($p_toplink)">
				<xsl:element name="a">
					<xsl:attribute name="href">
						<xsl:value-of select="'#mainBody'" />
					</xsl:attribute>
					<include item="top"/>
				</xsl:element>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="t_putSectionInclude">
		<xsl:param name="p_titleInclude" />
		<xsl:param name="p_content" />
		<xsl:param name="p_toplink"
							 select="false()" />

		<xsl:element name="mtps:CollapsibleArea">
			<xsl:attribute name="runat">
				<xsl:value-of select="'server'" />
			</xsl:attribute>
			<includeAttribute name="Title"
												item="{$p_titleInclude}"/>
			<xsl:element name="xml">
				<xsl:element name="string">
					<xsl:attribute name="id">
						<xsl:value-of select="'Title'" />
					</xsl:attribute>
					<include item="{$p_titleInclude}"/>
				</xsl:element>
			</xsl:element>

			<xsl:copy-of select="$p_content" />

			<xsl:if test="boolean($p_toplink)">
				<xsl:element name="a">
					<xsl:attribute name="href">
						<xsl:value-of select="'#mainBody'" />
					</xsl:attribute>
					<include item="top"/>
				</xsl:element>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="t_putSubSection">
		<xsl:param name="p_title" />
		<xsl:param name="p_content" />

		<xsl:element name="h4">
			<xsl:attribute name="class">
				<xsl:value-of select="'subHeading'"/>
			</xsl:attribute>
			<xsl:copy-of select="$p_title" />
		</xsl:element>
		<xsl:copy-of select="$p_content" />
	</xsl:template>

	<!-- ============================================================================================
	Alerts
	============================================================================================= -->

	<xsl:template name="t_putAlert">
		<xsl:param name="p_alertClass"
							 select="@class"/>
		<xsl:param name="p_alertContent"
							 select="''"/>
		<xsl:variable name="v_title">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note'">
					<xsl:text>alert_title_note</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='tip'">
					<xsl:text>alert_title_tip</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_title_caution</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_title_security</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_title_important</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_title_visualBasic</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_title_visualC#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_title_visualC++</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
					<xsl:text>alert_title_visualJ#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='implement'">
					<xsl:text>text_NotesForImplementers</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caller'">
					<xsl:text>text_NotesForCallers</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='inherit'">
					<xsl:text>text_NotesForInheritors</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_title_note</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="v_altTitle">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note' or $p_alertClass='implement' or $p_alertClass='caller' or $p_alertClass='inherit'">
					<xsl:text>alert_altText_note</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='tip'">
					<xsl:text>alert_altText_tip</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_altText_caution</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_altText_security</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_altText_important</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_altText_visualBasic</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_altText_visualC#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_altText_visualC++</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
					<xsl:text>alert_altText_visualJ#</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_altText_note</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="v_noteImg">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note' or $p_alertClass='tip' or $p_alertClass='implement' or $p_alertClass='caller' or $p_alertClass='inherit'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_security.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
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
					<th>
						<img class="mtps-img-src">
							<includeAttribute item="iconPath"
																name="src">
								<parameter>
									<xsl:value-of select="$v_noteImg"/>
								</parameter>
							</includeAttribute>
							<includeAttribute name="alt"
																item="{$v_altTitle}"/>
						</img>
						<xsl:text>&#160;</xsl:text>
						<include item="{$v_title}"/>
					</th>
				</tr>
				<tr>
					<td>
						<xsl:choose>
							<xsl:when test="$p_alertContent=''">
								<xsl:apply-templates/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:copy-of select="$p_alertContent"/>
							</xsl:otherwise>
						</xsl:choose>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<!-- ============================================================================================
	Debugging template for showing an element in comments
	============================================================================================= -->

	<xsl:template name="t_dumpContent">
		<xsl:param name="indent"
							 select="''"/>
		<xsl:param name="content"
							 select="."/>
		<xsl:for-each select="msxsl:node-set($content)">
			<xsl:choose>
				<xsl:when test="self::text()">
					<xsl:comment>
						<xsl:value-of select="$indent"/>
						<xsl:value-of select="."/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<xsl:comment>
						<xsl:value-of select="$indent"/>
						<xsl:value-of select="'«'"/>
						<xsl:value-of select="name()"/>
						<xsl:for-each select="@*">
							<xsl:text xml:space="preserve"> </xsl:text>
							<xsl:value-of select="name()"/>
							<xsl:value-of select="'='"/>
							<xsl:value-of select="."/>
						</xsl:for-each>
						<xsl:choose>
							<xsl:when test="./node()">
								<xsl:value-of select="'»'"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="'/»'"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:comment>
					<xsl:for-each select="node()">
						<xsl:call-template name="t_dumpContent">
							<xsl:with-param name="indent"
															select="concat($indent,'  ')"/>
						</xsl:call-template>
					</xsl:for-each>
					<xsl:if test="./node()">
						<xsl:comment>
							<xsl:value-of select="$indent"/>
							<xsl:value-of select="'«/'"/>
							<xsl:value-of select="name()"/>
							<xsl:value-of select="'»'"/>
						</xsl:comment>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
