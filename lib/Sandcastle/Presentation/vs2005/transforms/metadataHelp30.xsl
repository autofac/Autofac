<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
   >

	<xsl:template name="authoredMetadata30">

		<xsl:for-each select="/document/metadata/keyword[@index='K']">
			<meta name="System.Keywords">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='K']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
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

	<xsl:template name="helpMetadata30">
		<!-- F keywords -->
		<xsl:choose>

			<!-- namespace pages get the namespace keyword, if it exists -->
			<xsl:when test="$group='namespace'">
				<xsl:variable name="namespace" select="/document/reference/apidata/@name" />
				<xsl:if test="$namespace != ''">
					<meta name="Microsoft.Help.F1" content="{$namespace}" />
				</xsl:if>
			</xsl:when>

			<!-- type memberlist topics do NOT get F keywords -->
			<xsl:when test="$group='list' and $subgroup='members'"/>

			<!-- type overview pages get namespace.type keywords -->
			<xsl:when test="$group='type'">
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="type">
					<xsl:for-each select="/document/reference[1]">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:call-template name="writeF1WithApiName">
					<xsl:with-param name="namespace" select="$namespace" />
					<xsl:with-param name="type" select="$type" />
					<xsl:with-param name="member" select="''" />
				</xsl:call-template>

				<!-- for enums, write F1 keywords for each enum member -->
				<xsl:if test="$subgroup = 'enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<xsl:call-template name="writeF1WithApiName">
							<xsl:with-param name="namespace" select="$namespace" />
							<xsl:with-param name="type" select="$type" />
							<xsl:with-param name="member" select="apidata/@name" />
						</xsl:call-template>

					</xsl:for-each>
				</xsl:if>


				<!-- Insert additional F1 keywords to support XAML for class, struct, and enum topics in a set of namespaces. -->
				<xsl:call-template name="xamlMSHelpFKeywords30"/>
			</xsl:when>

			<!-- overload list pages get namespace.type.member keyword -->
			<xsl:when test="$group='list' and $subgroup='overload'">
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="type">
					<xsl:for-each select="/document/reference[1]/containers">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>

				<xsl:variable name="containingTypeId" select="/document/reference/containers/type[1]/@api" />
				<!-- do not write F1 keyword for overload list topics that contain only inherited members -->
				<xsl:if test="/document/reference/elements//element/containers/type[1][@api=$containingTypeId]">

					<!-- Generate a result tree fragment with all of the names for this overload page, TFS 856956, 864173-->
					<xsl:variable name="F1Names">
						<xsl:choose>
							<xsl:when test="/document/reference/apidata[@subgroup='constructor']">
								<name>
									<xsl:text>#ctor</xsl:text>
								</name>
								<name>
									<xsl:value-of select="/document/reference/containers/type[1]/apidata/@name" />
								</name>
							</xsl:when>
							<xsl:otherwise>
								<name>
									<xsl:value-of select="/document/reference/apidata/@name" />
								</name>
								<xsl:for-each select="/document/reference/elements/element[templates and containers/type[1][@api=$containingTypeId]]">
									<name>
										<xsl:value-of select="apidata/@name" />
										<xsl:text>``</xsl:text>
										<xsl:value-of select="count(templates/template)" />
									</name>
								</xsl:for-each>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>

					<xsl:for-each select="msxsl:node-set($F1Names)//name[not(. = preceding::name)]">
						<xsl:sort select="." />
						<xsl:call-template name="writeF1WithApiName">
							<xsl:with-param name="namespace" select="$namespace" />
							<xsl:with-param name="type" select="$type" />
							<xsl:with-param name="member" select="." />
						</xsl:call-template>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>

			<!-- member pages -->
			<xsl:when test="$group='member'">
				<xsl:choose>
					<!-- no F1 help entries for overload signature topics -->
					<xsl:when test="/document/reference/memberdata/@overload"/>

					<!-- no F1 help entries for explicit interface implementation members -->
					<xsl:when test="/document/reference[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]"/>

					<!-- Property pages -->
					<xsl:when test="$subgroup = 'property'">

						<xsl:variable name="type">
							<xsl:for-each select="/document/reference[1]/containers">
								<xsl:call-template name="typeNameWithTicks" />
							</xsl:for-each>
						</xsl:variable>

						<xsl:for-each select="document/reference/apidata/@name | document/reference/getter/@name | document/reference/setter/@name">
							<xsl:call-template name="writeF1WithApiName">
								<xsl:with-param name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
								<xsl:with-param name="type" select="$type" />
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:for-each>
					</xsl:when>

					<!-- other member pages get namespace.type.member keywords -->
					<xsl:otherwise>
						<xsl:call-template name="memberF1KeywordsHelp30"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

		</xsl:choose>
	</xsl:template>

	<xsl:template name="writeF1WithApiName">
		<xsl:param name="namespace"/>
		<xsl:param name="type" />
		<xsl:param name="member" />

		<!-- Make versions of namespace and member that are joinable. -->

		<xsl:variable name="namespaceJoinable">
			<xsl:choose>
				<xsl:when test="$namespace = ''">
					<xsl:value-of select="''" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat($namespace, '.')" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="memberJoinable">
			<xsl:choose>
				<xsl:when test="$member = ''">
					<xsl:value-of select="''" />
				</xsl:when>
				<xsl:when test="substring($type, string-length($type)) = '.'">
					<xsl:value-of select="$member" />
				</xsl:when>
				<xsl:when test="substring($member, string-length($member)) = '.'">
					<xsl:value-of select="substring($member, string-length($member) - 1)" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat('.', $member)" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>


		<xsl:variable name="apiName" select="concat($namespaceJoinable, $type, $memberJoinable)" />


		<xsl:if test="not($namespaceJoinable != '' and $type = '' and $memberJoinable != '') and $apiName != ''">
			<meta name="Microsoft.Help.F1" content="{concat($namespaceJoinable, $type, $memberJoinable)}" />
		</xsl:if>

	</xsl:template>

	<xsl:template name="memberF1KeywordsHelp30">
		<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
		<xsl:variable name="type">
			<xsl:for-each select="/document/reference/containers/type[1]">
				<xsl:call-template name="typeNameWithTicks" />
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="member">
			<xsl:choose>
				<!-- if the member is a constructor, use "#ctor" as the member name -->
				<xsl:when test="/document/reference/apidata[@subgroup='constructor']">#ctor</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/document/reference/apidata/@name"/>
					<!-- for generic members, include tick notation for number of generic template parameters. -->
					<xsl:if test="/document/reference/templates/template">
						<xsl:text>``</xsl:text>
						<xsl:value-of select="count(/document/reference/templates/template)"/>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:call-template name="writeF1WithApiName">
			<xsl:with-param name="namespace" select="$namespace" />
			<xsl:with-param name="type" select="$type" />
			<xsl:with-param name="member" select="$member" />
		</xsl:call-template>

		<!-- Write the constructor again as type.type -->
		<xsl:if test="/document/reference/apidata[@subgroup='constructor']">
			<xsl:call-template name="writeF1WithApiName">
				<xsl:with-param name="namespace" select="$namespace" />
				<xsl:with-param name="type" select="$type" />
				<xsl:with-param name="member" select="/document/reference/containers/type/apidata/@name" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- 
     Insert additional F1 keywords for class, struct, and enum topics in a set of WPF namespaces. 
     The keyword prefixes and the WPF namespaces are hard-coded in variables.
 -->
	<!--  <xsl:variable name="var_wpf_f1index_prefix_1">http://schemas.microsoft.com/winfx/2006/xaml/presentation#</xsl:variable>
  <xsl:variable name="var_wpf_f1index_prefix_1_namespaces">N:System.Windows.Controls#N:System.Windows.Documents#N:System.Windows.Shapes#N:System.Windows.Navigation#N:System.Windows.Data#N:System.Windows#N:System.Windows.Controls.Primitives#N:System.Windows.Media.Animation#N:System.Windows.Annotations#N:System.Windows.Annotations.Anchoring#N:System.Windows.Annotations.Storage#N:System.Windows.Media#N:System.Windows.Media.Animation#N:System.Windows.Media.Media3D#N:</xsl:variable> -->

	<xsl:template name="xamlMSHelpFKeywords30">
		<xsl:if test="$subgroup='class' or $subgroup='enumeration' or $subgroup='structure'">
			<xsl:if test="boolean(contains($var_wpf_f1index_prefix_1_namespaces, concat('#',/document/reference/containers/namespace/@api,'#'))
                           or starts-with($var_wpf_f1index_prefix_1_namespaces, concat(/document/reference/containers/namespace/@api,'#')))">
				<meta name="Microsoft.Help.F1" content="{concat($var_wpf_f1index_prefix_1, /document/reference/apidata/@name)}"/>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- Index Logic -->

	<xsl:template name="indexMetadata30">
		<xsl:choose>
			<!-- namespace topics get one unqualified index entry -->
			<xsl:when test="$topic-group='api' and $api-group='namespace'">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<meta name="System.Keywords">
					<includeAttribute name="content" item="namespaceIndexEntry">
						<parameter>
							<xsl:value-of select="msxsl:node-set($names)/name" />
						</parameter>
					</includeAttribute>
				</meta>
			</xsl:when>
			<!-- type overview topics get qualified and unqualified index entries, and an about index entry -->
			<xsl:when test="$topic-group='api' and $api-group='type'">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:for-each select="msxsl:node-set($names)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content" item="{$api-subgroup}IndexEntry">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</meta>
					<xsl:if test="boolean($namespace != '')">
						<meta name="System.Keywords">
							<includeAttribute name="content" item="{$api-subgroup}IndexEntry">
								<parameter>
									<xsl:value-of select="$namespace"/>
									<xsl:text>.</xsl:text>
									<xsl:copy-of select="." />
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:if>
					<!-- multi-topic types (not delegates and enumerations) get about entries, too-->
					<xsl:if test="$api-subgroup='class' or $api-subgroup='structure' or $api-subgroup='interface'">
						<meta name="System.Keywords">
							<includeAttribute name="content" item="aboutTypeIndexEntry">
								<parameter>
									<include item="{$api-subgroup}IndexEntry">
										<parameter>
											<xsl:copy-of select="."/>
										</parameter>
									</include>
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:if>
				</xsl:for-each>
				<!-- enumerations get the index entries for their members -->
				<xsl:if test="$api-subgroup='enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<meta name="System.Keywords">
							<includeAttribute name="content" item="{$api-subgroup}MemberIndexEntry">
								<parameter>
									<xsl:value-of select="apidata/@name" />
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>
			<!-- all member lists get unqualified entries, qualified entries, and unqualified sub-entries -->
			<xsl:when test="$topic-group='list' and $topic-subgroup='members'">
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($names)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content" item="{$api-subgroup}IndexEntry">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</meta>
					<meta name="System.Keywords">
						<includeAttribute name="content" item="membersIndexEntry">
							<parameter>
								<include item="{$api-subgroup}IndexEntry">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
				<xsl:variable name="qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="boolean($namespace != '')">
					<xsl:for-each select="msxsl:node-set($qnames)/name">
						<meta name="System.Keywords">
							<includeAttribute name="content" item="{$api-subgroup}IndexEntry">
								<parameter>
									<xsl:value-of select="." />
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>
			<!-- other member list pages get unqualified sub-entries -->
			<xsl:when test="$topic-group='list' and not($topic-subgroup = 'overload')">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$topic-subgroup='Operators'">
						<xsl:variable name="operators" select="document/reference/elements/element[not(apidata[@name='Explicit' or @name='Implicit'])]"/>
						<xsl:variable name="conversions" select="document/reference/elements/element[apidata[@name='Explicit' or @name='Implicit']]" />
						<xsl:variable name="entryType">
							<xsl:choose>
								<!-- operators + type conversions -->
								<xsl:when test="count($operators) &gt; 0 and count($conversions) &gt; 0">
									<xsl:value-of select="'operatorsAndTypeConversions'" />
								</xsl:when>
								<!-- no operators + type conversions -->
								<xsl:when test="not(count($operators) &gt; 0) and count($conversions) &gt; 0">
									<xsl:value-of select="'typeConversions'" />
								</xsl:when>
								<!-- operators + no type conversions -->
								<xsl:otherwise>
									<xsl:value-of select="$topic-subgroup" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($names)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$entryType}IndexEntry">
									<parameter>
										<include item="{$api-subgroup}IndexEntry">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="msxsl:node-set($names)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$subgroup}IndexEntry">
									<parameter>
										<include item="{$api-subgroup}IndexEntry">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- constructor (or constructor overload) topics get unqualified sub-entries using the type names -->
			<xsl:when test="($topic-group='api' and $api-subgroup='constructor' and not(/document/reference/memberdata/@overload)) or ($topic-subgroup='overload' and $api-subgroup = 'constructor')">
				<xsl:variable name="typeSubgroup" select="/document/reference/containers/type/apidata/@subgroup" />
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference/containers/type">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($names)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content" item="constructorIndexEntry">
							<parameter>
								<include item="{$typeSubgroup}IndexEntry">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
				<xsl:variable name="qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($qnames)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content" item="constructorTypeIndexEntry">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
			</xsl:when>
			<!-- op_explicit and op_implicit members -->
			<xsl:when test="$topic-group='api' and $api-subsubgroup='operator' and (document/reference/apidata/@name='Explicit' or document/reference/apidata/@name='Implicit')">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="operatorTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($names)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content" item="conversionOperatorIndexEntry">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
			</xsl:when>
			<!-- other member (or overload) topics get qualified and unqualified entries using the member names -->
			<xsl:when test="($topic-group='api' and $api-group='member' and not(/document/reference/memberdata/@overload)) or $topic-subgroup='overload'">

				<xsl:choose>
					<!-- overload op_explicit and op_implicit topics -->
					<xsl:when test="$api-subsubgroup='operator' and (document/reference/apidata/@name='Explicit' or document/reference/apidata/@name='Implicit')">
					</xsl:when>
					<!-- explicit interface implementation -->
					<xsl:when test="/document/reference/proceduredata/@virtual='true' and /document/reference/memberdata/@visibility='private'">
						<xsl:variable name="entryType">
							<xsl:choose>
								<xsl:when test="string($subsubgroup)">
									<xsl:value-of select="$subsubgroup" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$subgroup='overload'">
											<xsl:value-of select="/document/reference/apidata/@subgroup"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="$subgroup" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="names">
							<xsl:for-each select="/document/reference/implements/member">
								<xsl:call-template name="textNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($names)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$entryType}ExplicitIndexEntry">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
						<xsl:variable name="qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($qnames)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$entryType}ExplicitIndexEntry">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="entryType">
							<xsl:choose>
								<xsl:when test="string($subsubgroup)">
									<xsl:value-of select="$subsubgroup" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$api-subsubgroup='operator'">
											<xsl:value-of select="$api-subsubgroup"/>
										</xsl:when>
										<xsl:when test="$subgroup='overload'">
											<xsl:value-of select="/document/reference/apidata/@subgroup"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="$subgroup" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="names">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="textNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($names)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$entryType}IndexEntry">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
						<xsl:variable name="qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($qnames)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content" item="{$entryType}IndexEntry">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>

			</xsl:when>
			<!-- derived type lists get unqualified sub-entries -->
		</xsl:choose>
	</xsl:template>

	<xsl:template name="insert30Metadata">

		<!-- System.Language -->
		<meta name="Language">
			<includeAttribute name="content" item="locale" />
		</meta>

		<!-- System.Title -->
		<!-- <title> is set elsewhere (eg, main_conceptual.xsl, utilities_reference.xsl) -->

		<!-- System.Keywords -->
		<!-- Microsoft.Help.F1 -->
		<xsl:call-template name="indexMetadata30" />
		<xsl:call-template name="helpMetadata30" />
		<xsl:call-template name="authoredMetadata30" />

		<!-- Microsoft.Help.Id -->
		<meta name="Microsoft.Help.Id" content="{$key}" />

		<!-- Microsoft.Help.Description -->
		<xsl:if test="$abstractSummary">
			<meta name="Description">
				<xsl:attribute name="content">
					<xsl:call-template name="trimAtPeriod">
						<xsl:with-param name="string" select="$abstractSummary" />
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
		<meta name="Microsoft.Help.ContentType" content="Reference" />

		<!-- Microsoft.Package.Book -->
		<xsl:variable name="Book" select="/document/metadata/attribute[@name='Book']/text()" />
		<xsl:if test="$Book">
			<meta name="Microsoft.Package.Book" content="{$Book}" />
		</xsl:if>



	</xsl:template>



</xsl:stylesheet>
