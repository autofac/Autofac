<?xml version="1.0"?>
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
	<!-- ======================================================================================== -->

	<xsl:import href="xpathFunctions.xsl" />
	<xsl:import href="globalTemplates.xsl"/>
	<xsl:import href="codeTemplates.xsl"/>
	<xsl:import href="utilities_reference.xsl" />
	<xsl:import href="utilities_dduexml.xsl" />
	<xsl:import href="seealso_dduexml.xsl"/>

	<xsl:output method="xml"
							omit-xml-declaration="yes"
							indent="no"
							encoding="utf-8" />

	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="omitAptcaBoilerplate"/>
	<xsl:param name="changeHistoryOptions" />
	<xsl:param name="omitXmlnsBoilerplate"
						 select="'false'" />
	<xsl:param name="omitVersionInformation"
						 select="'false'" />

	<!-- ============================================================================================
	Global Variables
	============================================================================================= -->

	<xsl:variable name="g_abstractSummary">
		<xsl:for-each select="/document/comments/ddue:dduexml/ddue:summary">
			<xsl:apply-templates select="."
													 mode="abstract" />
		</xsl:for-each>
	</xsl:variable>
	<xsl:variable name="g_hasSeeAlsoSection"
								select="boolean( 
                           (count(/document/comments/ddue:dduexml/ddue:relatedTopics/*) > 0)  or 
                           ($g_apiTopicGroup='type' or $g_apiTopicGroup='member' or $g_apiTopicGroup='list')
                        )"/>
	<xsl:variable name="g_hasSecurityCriticalSection"
								select="boolean(
                          (/document/reference/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
 			                      not(/document/reference/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])) or  
                          (/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
			                      not(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])) or
                          ($g_apiSubGroup='property' and 
                            (((/document/reference/getter and (/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and 
		 	                        (/document/reference/setter and (/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) or
                             ((/document/reference/getter and (/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and not(/document/reference/setter)) or
                             (not(/document/reference/getter) and (/document/reference/setter and (/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) 
                            )) or
                            ($g_apiSubGroup='event' and 
                            (((/document/reference/adder and (/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and 							      
                              (/document/reference/remover and (/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) or
                             ((/document/reference/adder and (/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and	not(/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and not(/document/reference/remover)) or
                             (not(/document/reference/adder) and (/document/reference/remover and (/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))))
                            ))
                        )" />

	<!-- ============================================================================================
	Body
	============================================================================================= -->

	<xsl:template name="t_body">
		<!-- freshness date -->
		<xsl:call-template name="t_writeFreshnessDate">
			<xsl:with-param name="p_changedHistoryDate"
											select="/document/comments/ddue:dduexml//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table/ddue:row[1]/ddue:entry[1] | 
                      /document/comments/ddue:dduexml/ddue:changeHistory/ddue:content/ddue:table/ddue:row[1]/ddue:entry[1]"/>
		</xsl:call-template>

		<!--internalOnly boilerplate -->
		<xsl:if test="not($g_hasSecurityCriticalSection)">
			<xsl:call-template name="internalOnly"/>
		</xsl:if>

		<!-- obsolete boilerplate -->
		<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
			<xsl:call-template name="obsoleteSection" />
		</xsl:if>

		<!-- SecurityCritical boilerplate -->
		<xsl:if test="$g_hasSecurityCriticalSection">
			<xsl:choose>
				<xsl:when test="boolean($g_apiGroup='type')">
					<include item="boilerplate_typeSecurityCritical" />
				</xsl:when>
				<xsl:when test="boolean($g_apiGroup='member')">
					<xsl:choose>
						<xsl:when test="(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
 			                      not(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))">
							<include item="boilerplate_typeSecurityCritical" />
						</xsl:when>
						<xsl:otherwise>
							<include item="boilerplate_memberSecurityCritical" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:if>

		<!-- summary -->
		<!-- useBase boilerplate -->
		<xsl:if test="/document/comments/ddue:dduexml/ddue:useBase and /document/reference/overrides/member">
			<include item="boilerplate_useBase">
				<parameter>
					<xsl:apply-templates select="/document/reference/overrides/member"
															 mode="link"/>
				</parameter>
			</include>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:summary[1]) != ''">
				<span sdata="authoredSummary">
					<xsl:if test="$g_hasSecurityCriticalSection">
						<p>
							<include item="text_securityCritical" />
						</p>
					</xsl:if>
					<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:summary[1]" />
				</span>
			</xsl:when>
			<!-- if no authored summary, and not in primary framework (e.g. netfw), and overrides a base member: show link to base member -->
			<xsl:when test="/document/reference/overrides/member and not(/document/reference/versions/versions[1]//version)">
				<include item="boilerplate_useBaseSummary">
					<parameter>
						<xsl:apply-templates select="/document/reference/overrides/member"
																 mode="link"/>
					</parameter>
				</include>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:summary[2]" />
			</xsl:otherwise>
		</xsl:choose>

		<!-- Flags attribute boilerplate -->
		<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.FlagsAttribute']">
			<p>
				<include item="boilerplate_flagsSummary">
					<parameter>
						<referenceLink target="T:System.FlagsAttribute" />
					</parameter>
				</include>
			</p>
		</xsl:if>

		<!-- Non Cls Compliant boilerplate -->
		<xsl:if test="/document/reference/attributes/attribute[type[@api='T:System.CLSCompliantAttribute']]/argument[value='False']">
			<p/>
			<include item="boilerplate_NotClsCompliant"/>
			<xsl:text>&#160;</xsl:text>
			<xsl:if test="/document/comments/ddue:dduexml/ddue:clsCompliantAlternative">
				<include item="boilerplate_AltClsCompliant">
					<parameter>
						<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:clsCompliantAlternative/ddue:codeEntityReference"/>
					</parameter>
				</include>
			</xsl:if>
		</xsl:if>

		<!-- Overload list page boilerplate -->
		<xsl:if test="$g_apiTopicGroup = 'list' and $g_apiTopicSubGroup = 'overload'">
			<p>
				<include item="boilerplate_overloadSummary" />
			</p>
		</xsl:if>

		<xsl:if test="$g_apiTopicGroup='namespace'">
			<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:remarks" />
		</xsl:if>

		<!-- assembly information -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='root' or $g_apiTopicGroup='namespace')">
			<xsl:call-template name="t_putRequirementsInfo"/>
		</xsl:if>

		<!-- syntax -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='namespace')">
			<xsl:apply-templates select="/document/syntax" />
		</xsl:if>

		<!-- show authored Dependency Property Information section for properties -->
		<xsl:if test="$g_apiTopicSubGroup='property'">
			<xsl:apply-templates select="//ddue:section[starts-with(@address,'dependencyPropertyInfo')]"
													 mode="section"/>
		</xsl:if>

		<!-- show authored Routed Event Information section for events -->
		<xsl:if test="$g_apiTopicSubGroup='event'">
			<xsl:apply-templates select="//ddue:section[starts-with(@address,'routedEventInfo')]"
													 mode="section"/>
		</xsl:if>

		<!-- members -->
		<xsl:choose>
			<xsl:when test="$g_apiTopicGroup='root'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="root" />
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='namespace'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="namespace" />
			</xsl:when>
			<xsl:when test="$g_apiTopicSubGroup='enumeration'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="enumeration" />
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='type'">
				<xsl:apply-templates select="/document/reference/elements"
														 mode="type" />
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='list'">
				<xsl:choose>
					<xsl:when test="$g_apiTopicSubGroup='overload'">
						<xsl:apply-templates select="/document/reference/elements"
																 mode="overload" />
					</xsl:when>
					<xsl:when test="$g_apiTopicSubGroup='DerivedTypeList'">
						<xsl:apply-templates select="/document/reference/elements"
																 mode="derivedType" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="/document/reference/elements"
																 mode="member" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
		</xsl:choose>

		<!-- exceptions -->
		<xsl:if test="not($g_hasSecurityCriticalSection)">
			<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:exceptions" />
		</xsl:if>

		<!-- remarks -->
		<xsl:if test="not($g_apiTopicGroup='namespace') and not($g_hasSecurityCriticalSection)">
			<xsl:choose>
				<xsl:when test="/document/comments/ddue:dduexml/ddue:remarks">
					<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:remarks" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="t_writeRemarksSection">
						<xsl:with-param name="p_node"
														select="/document/comments/ddue:dduexml" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>

		<!-- examples -->
		<xsl:if test="not($g_hasSecurityCriticalSection)">
			<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:codeExamples" />
		</xsl:if>

		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='root')">
			<!--versions-->
			<xsl:apply-templates select="/document/reference/versions" />
		</xsl:if>

		<!-- permissions -->
		<xsl:if test="not($g_hasSecurityCriticalSection)">
			<xsl:call-template name="permissionsSection"/>
		</xsl:if>

		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='root')">
			<!--platforms-->
			<xsl:apply-templates select="/document/reference/platforms" />
		</xsl:if>

		<!--threadsafety-->
		<xsl:if test="$g_apiTopicSubGroup='class' or $g_apiTopicSubGroup='structure'">
			<xsl:call-template name="t_threadSafety" />
		</xsl:if>

		<!-- see also -->
		<xsl:call-template name="t_putSeeAlsoSection"/>

		<!-- changed table section -->
		<xsl:call-template name="t_writeChangeHistorySection" />

	</xsl:template>

	<xsl:template name="obsoleteSection">
		<p>
			<include item="boilerplate_obsolete">
				<parameter>
					<xsl:value-of select="$g_apiTopicSubGroup"/>
				</parameter>
			</include>
			<xsl:for-each select="/document/comments/ddue:dduexml/ddue:obsoleteCodeEntity">
				<xsl:text> </xsl:text>
				<include item="boilerplate_obsoleteNonobsoleteAlternative">
					<parameter>
						<xsl:apply-templates select="ddue:codeEntityReference" />
					</parameter>
				</include>
			</xsl:for-each>
		</p>
	</xsl:template>

	<xsl:template name="internalOnly">
		<xsl:if test="/document/comments/ddue:dduexml/ddue:internalOnly or /document/reference/containers/ddue:internalOnly">
			<div id="internalonly"
					 class="seeAlsoStyle">
				<p/>
				<include item="internalOnly" />
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_getParameterDescription">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:parameters[1]/ddue:parameter[normalize-space(string(ddue:parameterReference))=$name]//ddue:para) != ''">
				<span sdata="authoredParameterSummary">
					<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:parameters[1]/ddue:parameter[normalize-space(string(ddue:parameterReference))=$name]//ddue:para" />
				</span>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:parameters[2]/ddue:parameter[normalize-space(string(ddue:parameterReference))=$name]//ddue:para" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_getReturnsDescription">
		<xsl:choose>
			<xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[1]) != ''">
				<span sdata="authoredValueSummary">
					<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:returnValue[1]" />
				</span>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:returnValue[2]" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="returns">
		<xsl:choose>
			<xsl:when test="$g_apiSubGroup='field' and normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[1]) = '' and normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[2]) = ''"/>
			<xsl:otherwise>
				<div id="returns">
					<xsl:call-template name="t_putSubSection">
						<xsl:with-param name="p_title">
							<include item="title_{$g_apiSubGroup}Value"/>
						</xsl:with-param>
						<xsl:with-param name="p_content">
							<include item="typeLink">
								<parameter>
									<xsl:choose>
										<!-- special case for IsUdtReturn, see 780142 -->
										<xsl:when test="type[@api='T:System.Void']/requiredModifier/type[@api='T:System.Runtime.CompilerServices.IsUdtReturn']">
											<xsl:apply-templates select="../parameters/parameter/referenceTo/type"
																					 mode="link">
												<xsl:with-param name="qualified"
																				select="true()" />
											</xsl:apply-templates>
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="*[1]"
																					 mode="link">
												<xsl:with-param name="qualified"
																				select="true()" />
											</xsl:apply-templates>
										</xsl:otherwise>
									</xsl:choose>
								</parameter>
							</include>
							<br/>
							<xsl:call-template name="t_getReturnsDescription" />
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="templates">
		<div id="genericParameters">
			<xsl:call-template name="t_putSubSection">
				<xsl:with-param name="p_title">
					<include item="title_templates" />
				</xsl:with-param>
				<xsl:with-param name="p_content">
					<xsl:for-each select="template">
						<xsl:variable name="parameterName"
													select="@name" />
						<xsl:variable name="v_contravariant">
							<xsl:if test="variance/@contravariant='true'">
								<xsl:call-template name="t_inKeyword"/>
							</xsl:if>
						</xsl:variable>
						<xsl:variable name="v_covariant">
							<xsl:if test="variance/@covariant='true'">
								<xsl:call-template name="t_outKeyword"/>
							</xsl:if>
						</xsl:variable>
						<dl paramName="{$parameterName}">
							<dt>
								<xsl:copy-of select="$v_contravariant"/>
								<xsl:copy-of select="$v_covariant" />
								<span class="parameter">
									<xsl:value-of select="$parameterName"/>
								</span>
							</dt>
							<dd>
								<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:genericParameters/ddue:genericParameter[string(ddue:parameterReference)=$parameterName]//ddue:para" />
								<xsl:apply-templates select="." />
							</dd>
						</dl>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</div>
	</xsl:template>



	<xsl:template match="template[variance[@covariant='true']][1]">
		<p>
			<include item="text_covariant"/>
			<include item="text_variance" />
		</p>
	</xsl:template>

	<xsl:template match="template[variance[@contravariant='true']][1]">
		<p>
			<include item="text_contravariant" />
			<include item="text_variance" />
		</p>
	</xsl:template>

	<xsl:template name="t_getElementDescription">
		<xsl:choose>
			<xsl:when test="normalize-space(ddue:summary[1]) != ''">
				<span sdata="memberAuthoredSummary">
					<xsl:apply-templates select="ddue:summary[1]/ddue:para/node()" />
				</span>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="ddue:summary[2]/ddue:para/node()" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_getInternalOnlyDescription">
		<xsl:choose>
			<xsl:when test="ddue:internalOnly">
				<include item="infraStructure" />
			</xsl:when>
			<xsl:when test="count(element) &gt; 0">
				<xsl:variable name="internal">
					<xsl:for-each select="element">
						<xsl:if test="not(ddue:internalOnly)">
							<xsl:text>no</xsl:text>
						</xsl:if>
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="not(normalize-space($internal))">
					<include item="infraStructure" />
				</xsl:if>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_getOverloadSummary">

	</xsl:template>

	<xsl:template name="t_getOverloadSections">

	</xsl:template>

	<xsl:template match="syntax">
		<xsl:if test="count(*) > 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_syntax'"/>
				<xsl:with-param name="p_content">
					<div id="snippetGroup_Syntax"
							 class="code">
						<xsl:call-template name="t_putCodeSections">
							<xsl:with-param name="p_codeNodes"
															select="./div[@codeLanguage]" />
							<xsl:with-param name="p_nodeCount"
															select="count(./div[@codeLanguage])" />
							<xsl:with-param name="p_codeLangAttr"
															select="'codeLanguage'" />
						</xsl:call-template>
					</div>
					<xsl:apply-templates select="/document/syntax/div[@codeLanguage=XAML]"/>

					<!-- Show the authored XAML Values section, if any. -->
					<xsl:call-template name="showXamlValuesSection"/>

					<!-- parameters & return value -->
					<xsl:apply-templates select="/document/reference/templates" />
					<xsl:apply-templates select="/document/reference/parameters" />
					<xsl:apply-templates select="/document/reference/returns" />
					<xsl:apply-templates select="/document/reference/implements" />
					<!-- usage note for extension methods -->
					<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'] and boolean($g_apiSubGroup='method')">
						<xsl:call-template name="t_putSubSection">
							<xsl:with-param name="p_title">
								<include item="title_extensionUsage" />
							</xsl:with-param>
							<xsl:with-param name="p_content">
								<include item="text_extensionUsage">
									<parameter>
										<xsl:apply-templates select="/document/reference/parameters/parameter[1]/type"
																				 mode="link" />
									</parameter>
								</include>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>


	<!-- DDUEXML elements that behave differently in conceptual and reference -->

	<xsl:template match="ddue:exceptions">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_exceptions'" />
				<xsl:with-param name="p_content">
					<xsl:choose>
						<xsl:when test="ddue:exception">
							<div class="tableSection">
								<table>
									<tr>
										<th class="ps_exceptionNameColumn">
											<include item="header_exceptionName" />
										</th>
										<th class="ps_exceptionConditionColumn">
											<include item="header_exceptionCondition" />
										</th>
									</tr>
									<xsl:for-each select="ddue:exception">
										<tr>
											<td>
												<xsl:apply-templates select="ddue:codeEntityReference" />
											</td>
											<td>
												<xsl:apply-templates select="ddue:content" />
											</td>
										</tr>
									</xsl:for-each>
								</table>
							</div>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="permissionsSection">
		<!-- the containers/library/noAptca is added to reflection data by the ApplyVsDocModel transform -->
		<xsl:variable name="showAptcaBoilerplate"
									select="boolean(/document/reference/containers/library/noAptca and $omitAptcaBoilerplate!='true')"/>
		<xsl:if test="/document/comments/ddue:dduexml/ddue:permissions[normalize-space(.)] or $showAptcaBoilerplate">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_permissions'" />
				<xsl:with-param name="p_content">
					<ul>
						<xsl:for-each select="/document/comments/ddue:dduexml/ddue:permissions/ddue:permission">
							<li>
								<xsl:apply-templates select="ddue:codeEntityReference"/>&#160;<xsl:apply-templates select="ddue:content"/>
							</li>
						</xsl:for-each>
						<xsl:if test="$showAptcaBoilerplate">
							<li>
								<include item="boilerplate_aptca" />
							</li>
						</xsl:if>
					</ul>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeExample">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template name="t_runningHeader">
		<include item="runningHeaderText" />
	</xsl:template>

	<xsl:template name="t_putMemberIntro">
		<xsl:if test="$g_apiTopicSubGroup='members'">
			<p>
				<xsl:apply-templates select="/document/reference/containers/ddue:summary"/>
			</p>
		</xsl:if>
		<xsl:call-template name="t_memberIntroBoilerplate"/>
	</xsl:template>

	<xsl:template name="t_codelangAttributes">
		<xsl:call-template name="t_mshelpCodelangAttributes">
			<xsl:with-param name="snippets"
											select="/document/comments/ddue:dduexml/ddue:codeExamples/ddue:codeExample/ddue:legacy/ddue:content/ddue:snippets/ddue:snippet" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:codeEntityReference"
								mode="abstract">
		<xsl:call-template name="t_getTrimmedLastPeriod">
			<xsl:with-param name="p_string"
											select="." />
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_putSeeAlsoSection">
		<xsl:if test="$g_hasSeeAlsoSection">
			<xsl:element name="a">
				<xsl:attribute name="name">seeAlsoSection</xsl:attribute>
			</xsl:element>
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_relatedTopics'" />
				<xsl:with-param name="p_content">
					<xsl:choose>
						<xsl:when test="count(/document/comments/ddue:dduexml/ddue:relatedTopics/*) > 0">
							<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:relatedTopics"
																	 mode="seeAlso">
								<xsl:with-param name="p_autoGenerateLinks"
																select="true()" />
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="t_putSubSection">
								<xsl:with-param name="p_title">
									<include item="title_seeAlso_reference"/>
								</xsl:with-param>
								<xsl:with-param name="p_content">
									<xsl:call-template name="t_autogenSeeAlsoLinks"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>

				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
