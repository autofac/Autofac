<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5">
	<!-- ======================================================================================== -->

	<xsl:import href="xpathFunctions.xsl"/>
	<xsl:import href="globalTemplates.xsl"/>
	<xsl:import href="codeTemplates.xsl"/>
	<xsl:import href="utilities_reference.xsl"/>
	<xsl:import href="utilities_bibliography.xsl"/>

	<xsl:output method="xml" omit-xml-declaration="yes" indent="no" encoding="utf-8"/>

	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="bibliographyData" select="'../Data/bibliography.xml'"/>
	<xsl:param name="omitXmlnsBoilerplate" select="'false'"/>
	<xsl:param name="omitVersionInformation" select="'false'"/>

	<!-- ============================================================================================
	Global Variables
	============================================================================================= -->

	<xsl:variable name="g_abstractSummary" select="/document/comments/summary"/>
	<xsl:variable name="g_hasSeeAlsoSection"
		select="boolean((count(/document/comments//seealso[not(ancestor::overloads)] |
		/document/comments/conceptualLink |
		/document/reference/elements/element/overloads//seealso) > 0)  or 
    ($g_apiTopicGroup='type' or $g_apiTopicGroup='member' or $g_apiTopicGroup='list'))"/>

	<!-- ============================================================================================
	Body
	============================================================================================= -->

	<xsl:template name="t_body">

		<!-- auto-inserted info -->
		<xsl:apply-templates select="/document/comments/preliminary"/>
		<xsl:apply-templates select="/document/comments/summary"/>
		<xsl:if test="$g_apiTopicSubGroup='overload'">
			<xsl:apply-templates select="/document/reference/elements" mode="overloadSummary"/>
		</xsl:if>

		<!-- inheritance -->
		<xsl:apply-templates select="/document/reference/family"/>

		<!-- assembly information -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='root' or $g_apiTopicGroup='namespace')">
			<xsl:call-template name="t_putRequirementsInfo"/>
		</xsl:if>

		<!-- syntax -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='namespace')">
			<xsl:apply-templates select="/document/syntax"/>
		</xsl:if>

		<!-- members -->
		<xsl:choose>
			<xsl:when test="$g_apiTopicGroup='root'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="root"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='namespace'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="namespace"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicSubGroup='enumeration'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="enumeration"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='type'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="type"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='list'">
				<xsl:choose>
					<xsl:when test="$g_apiTopicSubGroup='overload'">
						<xsl:apply-templates select="/document/reference/elements"
																 mode="overload"/>
					</xsl:when>
					<xsl:when test="$g_apiTopicSubGroup='DerivedTypeList'">
						<xsl:apply-templates select="/document/reference/elements"
																 mode="derivedType"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="/document/reference/elements"
																 mode="member"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
		</xsl:choose>

		<!-- events -->
		<xsl:call-template name="t_events"/>
		<!-- exceptions -->
		<xsl:call-template name="t_exceptions"/>
		<!-- remarks -->
		<xsl:apply-templates select="/document/comments/remarks"/>
		<!-- examples -->
		<xsl:apply-templates select="/document/comments/example"/>

		<!-- contracts -->
		<xsl:call-template name="t_contracts"/>
		<!--versions-->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='root' )">
			<xsl:apply-templates select="/document/reference/versions"/>
		</xsl:if>
		<!-- permissions -->
		<xsl:call-template name="t_permissions"/>
		<!-- threadsafety -->
		<xsl:apply-templates select="/document/comments/threadsafety"/>

		<!-- bibliography -->
		<xsl:call-template name="t_bibliography"/>
		<!-- see also -->
		<xsl:call-template name="t_putSeeAlsoSection"/>

	</xsl:template>

	<!-- ============================================================================================
	Inline tags
	============================================================================================= -->

	<!-- pass through html tags -->
	<xsl:template match="p|ol|ul|li|dl|dt|dd|table|tr|th|td|a|img|b|i|strong|em|del|sub|sup|br|hr|h1|h2|h3|h4|h5|h6|pre|div|span|blockquote|abbr|acronym|u|font|map|area">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="para"
								name="t_para">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>

	<xsl:template match="c"
								name="t_codeInline">
		<span class="code">
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="preliminary" name="t_preliminary">
		<div class="preliminary">
			<xsl:choose>
				<xsl:when test="normalize-space(.)">
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:otherwise>
					<include item="preliminaryText" />
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="paramref"
								name="t_paramref">
		<span class="parameter">
			<xsl:value-of select="@name"/>
		</span>
	</xsl:template>

	<xsl:template match="typeparamref"
								name="t_typeparamref">
		<span class="typeparameter">
			<xsl:value-of select="@name"/>
		</span>
	</xsl:template>

	<!-- ============================================================================================
	Block sections
	============================================================================================= -->

	<xsl:template match="summary"
								name="t_summary">
		<div class="summary">
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="value"
								name="t_value">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<include item="title_fieldValue"/>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="returns"
								name="t_returns">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<include item="title_methodValue"/>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="remarks"
								name="t_remarks">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_remarks'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="example"
								name="t_example">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_examples'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="code"
								name="t_code">
		<xsl:call-template name="t_putCodeSection"/>
	</xsl:template>

	<!-- Details (nonstandard) -->

	<xsl:template match="details"
								name="t_details">
		<xsl:variable name="sectionTitle">
			<xsl:value-of select="(child::h1 | child::h2 | child::h3 | child::h4 | child::h5 | child::h6)[1]"/>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="$sectionTitle = ''">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="sectionAddress">
					<xsl:choose>
						<xsl:when test="@name != ''">
							<xsl:value-of select="@name"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="generate-id(.)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:call-template name="t_putSection">
					<xsl:with-param name="p_title">
						<xsl:value-of select="$sectionTitle"/>
					</xsl:with-param>
					<xsl:with-param name="p_content">
						<xsl:choose>
							<xsl:when test="$sectionTitle != ''">
								<xsl:apply-templates select="(child::h1 | child::h2 | child::h3 | child::h4 | child::h5 | child::h6)[1]/following-sibling::*"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="syntax"
								name="t_syntax">
		<xsl:if test="count(*) > 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_syntax'"/>
				<xsl:with-param name="p_content">
					<div id="snippetGroup_Syntax"
							 class="code">
						<xsl:call-template name="t_putCodeSections">
							<xsl:with-param name="p_codeNodes"
															select="./div[@codeLanguage]"/>
							<xsl:with-param name="p_nodeCount"
															select="count(./div[@codeLanguage])"/>
							<xsl:with-param name="p_codeLangAttr"
															select="'codeLanguage'"/>
						</xsl:call-template>
					</div>
					<!-- parameters & return value -->
					<xsl:apply-templates select="/document/reference/parameters"/>
					<xsl:apply-templates select="/document/reference/templates"/>
					<xsl:apply-templates select="/document/comments/value"/>
					<xsl:apply-templates select="/document/comments/returns"/>
					<xsl:apply-templates select="/document/reference/implements"/>
					<!-- usage note for extension methods -->
					<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'] and boolean($g_apiSubGroup='method')">
						<xsl:call-template name="t_putSubSection">
							<xsl:with-param name="p_title">
								<include item="title_extensionUsage"/>
							</xsl:with-param>
							<xsl:with-param name="p_content">
								<include item="text_extensionUsage">
									<parameter>
										<xsl:apply-templates select="/document/reference/parameters/parameter[1]/type"
																				 mode="link"/>
									</parameter>
								</include>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="overloads"
								mode="summary"
								name="t_overloadsSummary">
		<xsl:choose>
			<xsl:when test="count(summary) > 0">
				<xsl:apply-templates select="summary"/>
			</xsl:when>
			<xsl:otherwise>
				<div class="summary">
					<xsl:apply-templates/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="overloads"
								mode="sections"
								name="t_overloadsSections">
		<xsl:apply-templates select="remarks"/>
		<xsl:apply-templates select="example"/>
	</xsl:template>

	<xsl:template match="templates"
								name="t_templates">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_templates'"/>
			<xsl:with-param name="p_content">
				<dl>
					<xsl:for-each select="template">
						<xsl:variable name="templateName"
													select="@name"/>
						<dt>
							<span class="parameter">
								<xsl:value-of select="$templateName"/>
							</span>
						</dt>
						<dd>
							<xsl:apply-templates select="/document/comments/typeparam[@name=$templateName]"/>
						</dd>
					</xsl:for-each>
				</dl>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_events">
		<xsl:if test="count(/document/comments/event) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_events'"/>
				<xsl:with-param name="p_content">
					<div class="tableSection">
						<table>
							<tr>
								<th>
									<include item="header_eventType"/>
								</th>
								<th>
									<include item="header_eventReason"/>
								</th>
							</tr>
							<xsl:for-each select="/document/comments/event">
								<tr>
									<td>
										<referenceLink target="{@cref}" qualified="true"/>
									</td>
									<td>
										<xsl:apply-templates select="."/>
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_exceptions">
		<xsl:if test="count(/document/comments/exception) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_exceptions'"/>
				<xsl:with-param name="p_content">
					<div class="tableSection">
						<table>
							<tr>
								<th class="ps_exceptionNameColumn">
									<include item="header_exceptionName"/>
								</th>
								<th class="ps_exceptionConditionColumn">
									<include item="header_exceptionCondition"/>
								</th>
							</tr>
							<xsl:for-each select="/document/comments/exception">
								<tr>
									<td>
										<referenceLink target="{@cref}"
																	 qualified="true"/>
									</td>
									<td>
										<xsl:apply-templates select="."/>
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="threadsafety"
								name="t_threadsafety">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_threadSafety'"/>
			<xsl:with-param name="p_content">
				<xsl:choose>
					<xsl:when test="normalize-space(.)">
						<xsl:apply-templates/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="@static='true'">
							<include item="text_staticThreadSafe"/>
						</xsl:if>
						<xsl:if test="@static='false'">
							<include item="text_staticNotThreadSafe"/>
						</xsl:if>
						<xsl:if test="@instance='true'">
							<include item="text_instanceThreadSafe"/>
						</xsl:if>
						<xsl:if test="@instance='false'">
							<include item="text_instanceNotThreadSafe"/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_permissions">
		<xsl:if test="count(/document/comments/permission) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_permissions'"/>
				<xsl:with-param name="p_content">
					<div class="tableSection">
						<table>
							<tr>
								<th class="ps_permissionNameColumn">
									<include item="header_permissionName"/>
								</th>
								<th class="ps_permissionDescriptionColumn">
									<include item="header_permissionDescription"/>
								</th>
							</tr>
							<xsl:for-each select="/document/comments/permission">
								<tr>
									<td>
										<referenceLink target="{@cref}"
																	 qualified="true"/>
									</td>
									<td>
										<xsl:apply-templates select="."/>
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_contracts">
		<xsl:variable name="v_requires"
									select="/document/comments/requires"/>
		<xsl:variable name="v_ensures"
									select="/document/comments/ensures"/>
		<xsl:variable name="v_ensuresOnThrow"
									select="/document/comments/ensuresOnThrow"/>
		<xsl:variable name="v_invariants"
									select="/document/comments/invariant"/>
		<xsl:variable name="v_setter"
									select="/document/comments/setter"/>
		<xsl:variable name="v_getter"
									select="/document/comments/getter"/>
		<xsl:variable name="v_pure"
									select="/document/comments/pure"/>
		<xsl:if test="$v_requires or $v_ensures or $v_ensuresOnThrow or $v_invariants or $v_setter or $v_getter or $v_pure">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_contracts'"/>
				<xsl:with-param name="p_content">
					<!--Purity-->
					<xsl:if test="$v_pure">
						<xsl:text>This method is pure.</xsl:text>
					</xsl:if>
					<!--Contracts-->
					<div class="tableSection">
						<xsl:if test="$v_getter">
							<xsl:variable name="v_getterRequires"
														select="$v_getter/requires"/>
							<xsl:variable name="v_getterEnsures"
														select="$v_getter/ensures"/>
							<xsl:variable name="v_getterEnsuresOnThrow"
														select="$v_getter/ensuresOnThrow"/>
							<xsl:call-template name="t_putSubSection">
								<xsl:with-param name="p_title">
									<include item="title_getter"/>
								</xsl:with-param>
								<xsl:with-param name="p_content">
									<xsl:if test="$v_getterRequires">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_requiresName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_getterRequires"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$v_getterEnsures">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_ensuresName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_getterEnsures"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$v_getterEnsuresOnThrow">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_ensuresOnThrowName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_getterEnsuresOnThrow"/>
										</xsl:call-template>
									</xsl:if>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$v_setter">
							<xsl:variable name="v_setterRequires"
														select="$v_setter/requires"/>
							<xsl:variable name="v_setterEnsures"
														select="$v_setter/ensures"/>
							<xsl:variable name="v_setterEnsuresOnThrow"
														select="$v_setter/ensuresOnThrow"/>
							<xsl:call-template name="t_putSubSection">
								<xsl:with-param name="p_title">
									<include item="title_setter"/>
								</xsl:with-param>
								<xsl:with-param name="p_content">
									<xsl:if test="$v_setterRequires">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_requiresName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_setterRequires"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$v_setterEnsures">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_ensuresName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_setterEnsures"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$v_setterEnsuresOnThrow">
										<xsl:call-template name="t_contractsTable">
											<xsl:with-param name="p_title">
												<include item="header_ensuresOnThrowName"/>
											</xsl:with-param>
											<xsl:with-param name="p_contracts"
																			select="$v_setterEnsuresOnThrow"/>
										</xsl:call-template>
									</xsl:if>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$v_requires">
							<xsl:call-template name="t_contractsTable">
								<xsl:with-param name="p_title">
									<include item="header_requiresName"/>
								</xsl:with-param>
								<xsl:with-param name="p_contracts"
																select="$v_requires"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$v_ensures">
							<xsl:call-template name="t_contractsTable">
								<xsl:with-param name="p_title">
									<include item="header_ensuresName"/>
								</xsl:with-param>
								<xsl:with-param name="p_contracts"
																select="$v_ensures"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$v_ensuresOnThrow">
							<xsl:call-template name="t_contractsTable">
								<xsl:with-param name="p_title">
									<include item="header_ensuresOnThrowName"/>
								</xsl:with-param>
								<xsl:with-param name="p_contracts"
																select="$v_ensuresOnThrow"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$v_invariants">
							<xsl:call-template name="t_contractsTable">
								<xsl:with-param name="p_title">
									<include item="header_invariantsName"/>
								</xsl:with-param>
								<xsl:with-param name="p_contracts"
																select="$v_invariants"/>
							</xsl:call-template>
						</xsl:if>
					</div>
					<!--Contracts link-->
					<div class="ps_contractsLink">
						<a>
							<xsl:attribute name="target">
								<xsl:text>_blank</xsl:text>
							</xsl:attribute>
							<xsl:attribute name="href">
								<xsl:text>http://msdn.microsoft.com/en-us/devlabs/dd491992.aspx</xsl:text>
							</xsl:attribute>
							<xsl:text>Learn more about contracts</xsl:text>
						</a>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_contractsTable">
		<xsl:param name="p_title"/>
		<xsl:param name="p_contracts"/>
		<table>
			<tr>
				<th class="ps_contractsNameColumn">
					<xsl:copy-of select="$p_title"/>
				</th>
			</tr>
			<xsl:for-each select="$p_contracts">
				<tr>
					<td>
						<div class="code"
								 style="margin-bottom: 0pt; white-space: pre-wrap;">
							<pre xml:space="preserve" style="margin-bottom: 0pt"><xsl:value-of select="."/></pre>
						</div>
						<xsl:if test="@description or @inheritedFrom or @exception">
							<div style="font-size:95%; margin-left: 10pt;
                        margin-bottom: 0pt">
								<table
									class="contractaux"
									width="100%"
									frame="void"
									rules="none"
									border="0">
									<colgroup>
										<col width="10%"/>
										<col width="90%"/>
									</colgroup>
									<xsl:if test="@description">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Description: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<xsl:value-of select="@description"/>
											</td>
										</tr>
									</xsl:if>
									<xsl:if test="@inheritedFrom">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Inherited From: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<referenceLink target="{@inheritedFrom}">
													<xsl:value-of select="@inheritedFromTypeName"/>
												</referenceLink>
											</td>
										</tr>
									</xsl:if>
									<xsl:if test="@exception">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Exception: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<referenceLink target="{@exception}"
																			 qualified="true"/>
											</td>
										</tr>
									</xsl:if>
								</table>
							</div>
						</xsl:if>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_putSeeAlsoSection">
		<xsl:if test="$g_hasSeeAlsoSection">
			<xsl:element name="a">
				<xsl:attribute name="name">seeAlsoSection</xsl:attribute>
			</xsl:element>
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_relatedTopics'"/>
				<xsl:with-param name="p_content">
					<xsl:call-template name="t_autogenSeeAlsoLinks"/>
					<xsl:for-each select="/document/comments//seealso[not(ancestor::overloads)] | /document/reference/elements/element/overloads//seealso">
						<div class="seeAlsoStyle">
							<xsl:apply-templates select=".">
								<xsl:with-param name="displaySeeAlso"
																select="true()"/>
							</xsl:apply-templates>
						</div>
					</xsl:for-each>
					<!-- Copy conceptualLink elements as-is -->
					<xsl:for-each select="/document/comments/conceptualLink">
						<div class="seeAlsoStyle">
							<xsl:copy-of select="."/>
						</div>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Lists
	============================================================================================= -->

	<xsl:template match="list[@type='nobullet' or @type='']"
								name="t_plainList">
		<ul style="list-style-type:none;">
			<xsl:for-each select="item">
				<xsl:apply-templates select="."
														 mode="plain"/>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="item"
								mode="plain"
								name="t_plainListItem">
		<li>
			<xsl:choose>
				<xsl:when test="term or description">
					<xsl:if test="term">
						<xsl:apply-templates select="term"/>
						<br/>
					</xsl:if>
					<xsl:apply-templates select="description"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
				</xsl:otherwise>
			</xsl:choose>
		</li>
	</xsl:template>

	<xsl:template match="list[@type='bullet']"
								name="t_bulletList">
		<ul>
			<xsl:for-each select="item">
				<xsl:apply-templates select="."
														 mode="bullet"/>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="item"
								mode="bullet"
								name="t_bulletListItem">
		<li>
			<xsl:choose>
				<xsl:when test="term or description">
					<xsl:if test="term">
						<xsl:apply-templates select="term"/>
						<br/>
					</xsl:if>
					<xsl:apply-templates select="description"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
				</xsl:otherwise>
			</xsl:choose>
		</li>
	</xsl:template>

	<xsl:template match="list[@type='number']"
								name="t_numberList">
		<ol>
			<xsl:if test="@start">
				<xsl:attribute name="start">
					<xsl:value-of select="@start"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:for-each select="item">
				<xsl:apply-templates select="."
														 mode="number"/>
			</xsl:for-each>
		</ol>
	</xsl:template>

	<xsl:template match="item"
								mode="number"
								name="t_numberListItem">
		<li>
			<xsl:choose>
				<xsl:when test="term or description">
					<xsl:if test="term">
						<xsl:apply-templates select="term"/>
						<br/>
					</xsl:if>
					<xsl:apply-templates select="description"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
				</xsl:otherwise>
			</xsl:choose>
		</li>
	</xsl:template>

	<xsl:template match="list[@type='table']"
								name="t_tableList">
		<div class="tableSection">
			<table>
				<xsl:for-each select="listheader">
					<tr>
						<xsl:for-each select="*">
							<th>
								<xsl:apply-templates/>
							</th>
						</xsl:for-each>
					</tr>
				</xsl:for-each>
				<xsl:for-each select="item">
					<xsl:apply-templates select="."
															 mode="table"/>
				</xsl:for-each>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="item"
								mode="table"
								name="t_tableListItem">
		<tr>
			<xsl:for-each select="*">
				<td>
					<xsl:apply-templates/>
				</td>
			</xsl:for-each>
		</tr>
	</xsl:template>

	<xsl:template match="list[@type='definition']"
								name="t_definitionList">
		<dl class="authored">
			<xsl:for-each select="item">
				<xsl:apply-templates select="."
														 mode="definition"/>
			</xsl:for-each>
		</dl>
	</xsl:template>

	<xsl:template match="item"
								mode="definition"
								name="t_definitionListItem">
		<dt>
			<xsl:apply-templates select="term"/>
		</dt>
		<dd>
			<xsl:apply-templates select="description"/>
		</dd>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="item/para[1][ancestor::list[@type!='table' and @type!='definition']]"
								name="t_listItemPara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'listItem'"/>
				<xsl:with-param name="p_parentLevel"
												select="1"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_listItemPara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="item/para[1][ancestor::list[@type='table'] and not(ancestor::listheader)]"
								name="t_tableItemPara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'table'"/>
				<xsl:with-param name="p_parentLevel"
												select="1"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_tableItemPara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="item/para[1][ancestor::list[@type='table'] and ancestor::listheader]"
								name="t_tableHeaderItemPara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'tableHeader'"/>
				<xsl:with-param name="p_parentLevel"
												select="1"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_tableHeaderItemPara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="item/term/para[1]"
								name="t_listItemTermPara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'definedTerm'"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_listItemTermPara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="item/description/para[1]"
								name="t_listItemDescriptionPara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'definition'"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_listItemDescriptionPara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Inline tags
	============================================================================================= -->

	<xsl:template match="conceptualLink">
		<xsl:choose>
			<xsl:when test="normalize-space(.)">
				<conceptualLink class="mtps-internal-link"
												target="{@target}">
					<xsl:apply-templates/>
				</conceptualLink>
			</xsl:when>
			<xsl:otherwise>
				<conceptualLink class="mtps-internal-link"
												target="{@target}"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@cref]"
								name="t_seeCRef">
		<xsl:choose>
			<xsl:when test="starts-with(@cref,'O:')">
				<referenceLink target="{concat('Overload:',substring(@cref,3))}"
											 display-target="format"
											 show-parameters="false"
											 class="mtps-internal-link">
					<xsl:choose>
						<xsl:when test="normalize-space(.)">
							<xsl:value-of select="." />
						</xsl:when>
						<xsl:otherwise>
							<include item="boilerplate_seeAlsoOverloadLink">
								<parameter>{0}</parameter>
							</include>
						</xsl:otherwise>
					</xsl:choose>
				</referenceLink>
			</xsl:when>
			<xsl:when test="normalize-space(.)">
				<referenceLink target="{@cref}"
											 class="mtps-internal-link">
					<xsl:apply-templates/>
				</referenceLink>
			</xsl:when>
			<xsl:otherwise>
				<referenceLink target="{@cref}"
											 class="mtps-internal-link"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@href]"
								name="t_seeHRef">
		<xsl:call-template name="t_hyperlink">
			<xsl:with-param name="p_content"
											select="."/>
			<xsl:with-param name="p_href"
											select="@href"/>
			<xsl:with-param name="p_target"
											select="@target"/>
			<xsl:with-param name="p_alt"
											select="@alt"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="see[@langword]">
		<span sdata="langKeyword" value="{@langword}">
			<xsl:variable name="v_syntaxKeyword">
				<xsl:if test="/document/syntax">
					<xsl:value-of select="'true'"/>
				</xsl:if>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="@langword='null' or @langword='Nothing' or @langword='nullptr'">
					<xsl:call-template name="t_nullKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@langword='static' or @langword='Shared'">
					<xsl:call-template name="t_staticKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@langword='virtual' or @langword='Overridable'">
					<xsl:call-template name="t_virtualKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@langword='true' or @langword='True'">
					<xsl:call-template name="t_trueKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@langword='false' or @langword='False'">
					<xsl:call-template name="t_falseKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@langword='abstract' or @langword='MustInherit'">
					<xsl:call-template name="t_abstractKeyword">
						<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@langword"/>
				</xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>

	<xsl:template match="seealso[@href]"
								name="t_seealsoHRef">
		<xsl:param name="displaySeeAlso"
							 select="false()"/>
		<xsl:if test="$displaySeeAlso">
			<xsl:call-template name="t_hyperlink">
				<xsl:with-param name="p_content"
												select="."/>
				<xsl:with-param name="p_href"
												select="@href"/>
				<xsl:with-param name="p_target"
												select="@target"/>
				<xsl:with-param name="p_alt"
												select="@alt"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="seealso"
								name="t_seealso">
		<xsl:param name="displaySeeAlso"
							 select="false()"/>
		<xsl:if test="$displaySeeAlso">
			<xsl:choose>
				<xsl:when test="starts-with(@cref,'O:')">
					<referenceLink target="{concat('Overload:',substring(@cref,3))}"
												 display-target="format"
												 show-parameters="false"
												 class="mtps-internal-link">
						<xsl:choose>
							<xsl:when test="normalize-space(.)">
								<xsl:value-of select="." />
							</xsl:when>
							<xsl:otherwise>
								<include item="boilerplate_seeAlsoOverloadLink">
									<parameter>{0}</parameter>
								</include>
							</xsl:otherwise>
						</xsl:choose>
					</referenceLink>
				</xsl:when>
				<xsl:when test="normalize-space(.)">
					<referenceLink target="{@cref}"
												 qualified="true">
						<xsl:value-of select="."/>
					</referenceLink>
				</xsl:when>
				<xsl:otherwise>
					<referenceLink target="{@cref}"
												 qualified="true"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_hyperlink">
		<xsl:param name="p_content"/>
		<xsl:param name="p_href"/>
		<xsl:param name="p_target"/>
		<xsl:param name="p_alt"/>

		<a>
			<xsl:choose>
				<xsl:when test="starts-with($p_href,'ms.help?')">
					<xsl:attribute name="class">
						mtps-internal-link
					</xsl:attribute>
				</xsl:when>
				<xsl:when test="starts-with($p_href,'http:')">
					<xsl:attribute name="class">
						mtps-external-link
					</xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<xsl:attribute name="href">
				<xsl:value-of select="$p_href"/>
			</xsl:attribute>
			<xsl:choose>
				<xsl:when test="normalize-space($p_target)">
					<xsl:attribute name="target">
						<xsl:value-of select="normalize-space($p_target)"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="target">_blank</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="normalize-space($p_alt)">
				<xsl:attribute name="title">
					<xsl:value-of select="normalize-space($p_alt)"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="normalize-space($p_content)">
					<xsl:value-of select="$p_content"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$p_href"/>
				</xsl:otherwise>
			</xsl:choose>
		</a>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="note"
								name="t_note">
		<xsl:call-template name="t_putAlert">
			<xsl:with-param name="p_alertClass"
											select="@type"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="note/para[1]"
								name="t_notePara1">
		<xsl:variable name="v_minimalSpacing">
			<xsl:call-template name="t_checkMinimalSpacing">
				<xsl:with-param name="p_spacingType"
												select="'alert'"/>
				<xsl:with-param name="p_parentLevel"
												select="1"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">t_notePara1[<xsl:value-of select="$v_minimalSpacing"/>]</xsl:comment>-->
		<xsl:choose>
			<xsl:when test="$v_minimalSpacing='true'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="t_para"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_putMemberIntro">
		<xsl:if test="$g_apiTopicSubGroup='members'">
			<p>
				<xsl:apply-templates select="/document/reference/containers/summary"/>
			</p>
		</xsl:if>
		<xsl:call-template name="t_memberIntroBoilerplate"/>
	</xsl:template>

	<xsl:template name="t_codelangAttributes">
		<xsl:call-template name="t_mshelpCodelangAttributes">
			<xsl:with-param name="snippets"
											select="/document/comments/example/code"/>
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_runningHeader">
		<include item="runningHeaderText"/>
	</xsl:template>

	<xsl:template name="t_getParameterDescription">
		<xsl:param name="name"/>
		<xsl:apply-templates select="/document/comments/param[@name=$name]"/>
	</xsl:template>

	<xsl:template name="t_getReturnsDescription">
		<xsl:param name="name"/>
		<xsl:apply-templates select="/document/comments/param[@name=$name]"/>
	</xsl:template>

	<xsl:template name="t_getElementDescription">
		<xsl:apply-templates select="summary[1]"/>
	</xsl:template>

	<xsl:template name="t_getOverloadSummary">
		<xsl:apply-templates select="overloads"
												 mode="summary"/>
	</xsl:template>

	<xsl:template name="t_getOverloadSections">
		<xsl:apply-templates select="overloads"
												 mode="sections"/>
	</xsl:template>

	<xsl:template name="t_getInternalOnlyDescription">

	</xsl:template>

	<!-- ============================================================================================
	Bibliography
	============================================================================================= -->

	<xsl:key name="k_citations"
					 match="//cite"
					 use="text()"/>

	<xsl:variable name="g_hasCitations"
								select="boolean(count(//cite) > 0)"/>

	<xsl:template match="cite"
								name="t_cite">
		<xsl:variable name="v_currentCitation"
									select="text()"/>
		<xsl:for-each select="//cite[generate-id(.)=generate-id(key('k_citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:if test="$v_currentCitation=.">
				<xsl:choose>
					<xsl:when test="document($bibliographyData)/bibliography/reference[@name=$v_currentCitation]">
						<sup class="citation">
							<a>
								<xsl:attribute name="href">
									#cite<xsl:value-of select="position()"/>
								</xsl:attribute>[<xsl:value-of select="position()"/>]
							</a>
						</sup>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="t_bibliography">
		<xsl:if test="$g_hasCitations">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'bibliographyTitle'"/>
				<xsl:with-param name="p_content">
					<xsl:call-template name="t_autogenBibliographyLinks"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_autogenBibliographyLinks">
		<xsl:for-each select="//cite[generate-id(.)=generate-id(key('k_citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:variable name="citation"
										select="."/>
			<xsl:variable name="entry"
										select="document($bibliographyData)/bibliography/reference[@name=$citation]"/>

			<xsl:call-template name="bibliographyReference">
				<xsl:with-param name="number"
												select="position()"/>
				<xsl:with-param name="data"
												select="$entry"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
