<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
		xmlns:MSHelp="http://msdn.microsoft.com/mshelp" xmlns:msxsl="urn:schemas-microsoft-com:xslt" >

	<xsl:template name="insertMetadata">
		<xsl:if test="$metadata='true'">
			<xml>
				<MSHelp:Attr Name="AssetID" Value="{$key}" />
				<!-- toc metadata -->
				<xsl:call-template name="linkMetadata" />
				<xsl:call-template name="indexMetadata" />
				<xsl:call-template name="helpMetadata" />
				<MSHelp:Attr Name="TopicType" Value="apiref" />
				<!-- attribute to allow F1 help integration -->
				<MSHelp:Attr Name="TopicType" Value="kbSyntax" />
				<xsl:call-template name="apiTaggingMetadata" />
				<MSHelp:Attr Name="Locale">
					<includeAttribute name="Value" item="locale" />
				</MSHelp:Attr>
				<xsl:choose>
					<xsl:when test="string-length(normalize-space($summary)) &gt; 254">
						<MSHelp:Attr Name="Abstract" Value="{concat(substring(normalize-space($summary),1,250), ' ...')}" />
					</xsl:when>
					<xsl:when test="string-length(normalize-space($summary)) &gt; 0 and $summary != '&#160;'">
						<MSHelp:Attr Name="Abstract" Value="{normalize-space($summary)}" />
					</xsl:when>
				</xsl:choose>
			</xml>
		</xsl:if>
	</xsl:template>

	<xsl:template name="apiTaggingMetadata">
		<xsl:if test="$tgroup='api' and ($group='type' or $group='member')">
			<MSHelp:Attr Name="APIType" Value="Managed" />
			<MSHelp:Attr Name="APILocation" Value="{/document/reference/containers/library/@assembly}.dll" />
			<xsl:choose>
				<xsl:when test="$group='type'">
					<xsl:variable name="apiTypeName">
						<xsl:choose>
							<xsl:when test="/document/reference/containers/namespace/apidata/@name != ''">
								<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/apidata/@name)" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="/document/reference/apidata/@name" />
							</xsl:otherwise>
						</xsl:choose>
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type -->
					<MSHelp:Attr Name="APIName" Value="{$apiTypeName}" />
					<xsl:choose>
						<xsl:when test="boolean($subgroup='delegate')">
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','.ctor')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','Invoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','BeginInvoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','EndInvoke')}" />
						</xsl:when>
						<xsl:when test="$subgroup='enumeration'">
							<xsl:for-each select="/document/reference/elements/element">
								<MSHelp:Attr Name="APIName" Value="{substring(@api,3)}" />
							</xsl:for-each>
							<!-- Namespace + Type + Member for each member -->
						</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$group='member'">
					<xsl:variable name="apiTypeName">
						<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/containers/type/apidata/@name)" />
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type + Member -->
					<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.',/document/reference/apidata/@name)}" />
					<xsl:choose>
						<!-- for properties, add APIName attribute get/set accessor methods -->
						<xsl:when test="boolean($subgroup='property')">
							<xsl:if test="/document/reference/propertydata[@get='true']">
								<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.get_',/document/reference/apidata/@name)}" />
							</xsl:if>
							<xsl:if test="/document/reference/propertydata[@set='true']">
								<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.set_',/document/reference/apidata/@name)}" />
							</xsl:if>
						</xsl:when>
						<!-- for events, add APIName attribute add/remove accessor methods -->
						<xsl:when test="boolean($subgroup='event')">
							<xsl:if test="/document/reference/eventdata[@add='true']">
								<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.add_',/document/reference/apidata/@name)}" />
							</xsl:if>
							<xsl:if test="/document/reference/eventdata[@remove='true']">
								<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.remove_',/document/reference/apidata/@name)}" />
							</xsl:if>
						</xsl:when>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template name="linkMetadata">
		<!-- code entity reference keyword -->
		<MSHelp:Keyword Index="A" Term="{$key}" />
		<!-- frlrf keywords -->
		<xsl:choose>
			<xsl:when test="$group='namespace'">
				<MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/apidata/@name),'.','')}" />
			</xsl:when>
			<!-- types & members, too -->
			<xsl:when test="$group='type'">
				<MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/apidata/@name, 'ClassTopic'),'.','')}" />
				<MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/apidata/@name, 'MembersTopic'),'.','')}" />
			</xsl:when>
			<xsl:when test="$group='member'">
				<MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/containers/type/apidata/@name, 'Class', /document/reference/apidata/@name, 'Topic'),'.','')}" />
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="helpMetadata">
		<!-- F keywords -->
		<xsl:choose>
			<!-- namespace pages get the namespace keyword, if it exists -->
			<xsl:when test="$group='namespace'">
				<xsl:variable name="namespace" select="/document/reference/apidata/@name" />
				<xsl:if test="boolean($namespace)">
					<MSHelp:Keyword Index="F" Term="{$namespace}" />
				</xsl:if>
			</xsl:when>
			<!-- type pages get type and namespace.type keywords -->
			<xsl:when test="$group='type'">
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="type">
					<xsl:for-each select="/document/reference[1]">
						<xsl:call-template name="typeNamePlain">
							<xsl:with-param name="annotate" select="true()" />
						</xsl:call-template>
					</xsl:for-each>
				</xsl:variable>
				<MSHelp:Keyword Index="F" Term="{$type}" />
				<xsl:if test="boolean($namespace)">
					<MSHelp:Keyword Index="F" Term="{concat($namespace,'.',$type)}" />
				</xsl:if>
				<!-- some extra F keywords for some special types in the System namespace -->
				<xsl:if test="$namespace='System'">
					<xsl:choose>
						<xsl:when test="$type='Object'">
							<MSHelp:Keyword Index="F" Term="object" />
						</xsl:when>
						<xsl:when test="$type='String'">
							<MSHelp:Keyword Index="F" Term="string" />
						</xsl:when>
						<xsl:when test="$type='Int32'">
							<MSHelp:Keyword Index="F" Term="int" />
						</xsl:when>
						<xsl:when test="$type='Boolean'">
							<MSHelp:Keyword Index="F" Term="bool" />
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</xsl:when>
			<!-- member pages get member, type.member, and namepsace.type.member keywords -->
			<xsl:when test="$group='member'">
				<xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="type">
					<xsl:for-each select="/document/reference/containers/type[1]">
						<xsl:call-template name="typeNamePlain">
							<xsl:with-param name="annotate" select="true()" />
						</xsl:call-template>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="member">
					<xsl:choose>
						<!-- if the member is a constructor, use the member name for the type name -->
						<xsl:when test="$subgroup='constructor'">
							<xsl:value-of select="$type" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="/document/reference/apidata/@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<MSHelp:Keyword Index="F" Term="{$member}" />
				<MSHelp:Keyword Index="F" Term="{concat($type, '.', $member)}" />
				<xsl:if test="boolean($namespace)">
					<MSHelp:Keyword Index="F" Term="{concat($namespace, '.', $type, '.', $member)}" />
				</xsl:if>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="indexMetadata">
		<xsl:choose>
			<!-- namespace topics get one unqualified index entry -->
			<xsl:when test="$group='namespace'">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<MSHelp:Keyword Index="K">
					<includeAttribute name="Term" item="namespaceIndexEntry">
						<parameter>
							<xsl:value-of select="msxsl:node-set($names)/name" />
						</parameter>
					</includeAttribute>
				</MSHelp:Keyword>
			</xsl:when>
			<!-- type topics get unqualified and qualified index entries -->
			<xsl:when test="$group='type'">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term" item="{$subgroup}IndexEntry">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
				<xsl:variable name="qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($qnames)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term" item="{$subgroup}IndexEntry">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
				<!-- enumeration topics also get entries for each member -->
			</xsl:when>
			<!-- constructor (or constructor overload) topics get unqualified entries using the type names -->
			<xsl:when test="$subgroup='constructor' and not(/document/reference/memberdata/@overload='true')">
				<xsl:variable name="names">
					<xsl:for-each select="/document/reference/containers/type">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term" item="constructorIndexEntry">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
			</xsl:when>
			<!-- other member (or overload) topics get qualified and unqualified entries using the member names -->
			<xsl:when test="$group='member' and not(/document/reference/memberdata/@overload='true')">
				<!-- don't create index entries for explicit interface implementations -->
				<xsl:if test="not(/document/reference/proceduredata/@virtual='true' and /document/reference/memberdata/@visibility='private')">
					<xsl:variable name="entryType">
						<xsl:choose>
							<xsl:when test="$subsubgroup">
								<xsl:value-of select="$subsubgroup" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$subgroup" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:variable name="names">
						<xsl:for-each select="/document/reference">
							<xsl:call-template name="textNames" />
						</xsl:for-each>
					</xsl:variable>
					<xsl:for-each select="msxsl:node-set($names)/name">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term" item="{$entryType}IndexEntry">
								<parameter>
									<xsl:value-of select="." />
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:for-each>
					<xsl:variable name="qnames">
						<xsl:for-each select="/document/reference">
							<xsl:call-template name="qualifiedTextNames" />
						</xsl:for-each>
					</xsl:variable>
					<xsl:for-each select="msxsl:node-set($qnames)/name">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term" item="{$entryType}IndexEntry">
								<parameter>
									<xsl:value-of select="." />
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="typeNameWithTicks">
		<xsl:for-each select="type|(containers/type)">
			<xsl:call-template name="typeNameWithTicks" />
			<xsl:text>.</xsl:text>
		</xsl:for-each>
		<xsl:value-of select="apidata/@name" />
		<xsl:if test="boolean(templates/template)">
			<xsl:text>`</xsl:text>
			<xsl:value-of select="count(templates/template)"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="qualifiedTextNames">
		<xsl:choose>
			<!-- explicit interface implementations -->
			<xsl:when test="memberdata[@visibility='private'] and proceduredata[@virtual = 'true']">
				<xsl:variable name="left">
					<xsl:for-each select="containers/type">
						<xsl:call-template name="textNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:for-each select="implements/member">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<!-- members get qualified by type name -->
			<xsl:when test="apidata/@group='member' and containers/type">
				<xsl:variable name="left">
					<xsl:for-each select="containers/type">
						<xsl:call-template name="textNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<!-- types get qualified by namespace name -->
			<xsl:when test="typedata and containers/namespace/apidata/@name">
				<xsl:variable name="left">
					<xsl:for-each select="containers/namespace">
						<xsl:call-template name="simpleTextNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="textNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- given two XML lists of API names (produced by textNames template below), produces an XML list
  that dot-concatenates them, respecting the @language attributes -->
	<xsl:template name="combineTextNames">
		<xsl:param name="left" />
		<xsl:param name="right" />
		<xsl:param name="concatenateOperator" select="'.'" />

		<xsl:choose>
			<xsl:when test="count($left/name) &gt; 1">
				<xsl:choose>
					<xsl:when test="count($right/name) &gt; 1">
						<!-- both left and right are multi-language -->
						<xsl:for-each select="$left/name">
							<xsl:variable name="language" select="@language" />
							<name language="{$language}">
								<xsl:apply-templates select="." />
								<xsl:copy-of select="$concatenateOperator" />
								<xsl:apply-templates select="$right/name[@language=$language]" />
							</name>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<!-- left is multi-language, right is not -->
						<xsl:for-each select="$left/name">
							<xsl:variable name="language" select="@language" />
							<name language="{$language}">
								<xsl:apply-templates select="." />
								<xsl:if test="$right/name">
									<xsl:copy-of select="$concatenateOperator"/>
								</xsl:if>
								<xsl:value-of select="$right/name"/>
							</name>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="count($right/name) &gt; 1">
						<!-- right is multi-language, left is not -->
						<xsl:for-each select="$right/name">
							<xsl:variable name="language" select="@language" />
							<name language="{.}">
								<xsl:value-of select="$left/name"/>
								<xsl:if test="$left/name">
									<xsl:copy-of select="$concatenateOperator"/>
								</xsl:if>
								<xsl:apply-templates select="." />
							</name>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<!-- neiter is multi-language -->
						<name>
							<xsl:value-of select="$left/name"/>
							<xsl:if test="$left/name and $right/name">
								<xsl:copy-of select="$concatenateOperator"/>
							</xsl:if>
							<xsl:value-of select="$right/name"/>
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- produces an XML list of API names; context is parent of apidata element -->
	<!-- if there are no templates: <name>Blah</name> -->
	<!-- if there are templates: <name langauge="c">Blah<T></name><name language="v">Blah(Of T)</name> -->
	<xsl:template name="simpleTextNames">
		<xsl:choose>
			<xsl:when test="specialization">
				<xsl:apply-templates select="specialization" mode="index">
					<xsl:with-param name="name" select="apidata/@name" />
				</xsl:apply-templates>
			</xsl:when>
			<xsl:when test="templates">
				<xsl:apply-templates select="templates" mode="index">
					<xsl:with-param name="name" select="apidata/@name" />
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<name>
					<xsl:choose>
						<xsl:when test="apidata/@subgroup = 'constructor'">
							<xsl:value-of select="containers/type/apidata/@name"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="apidata/@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</name>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="textNames">
		<xsl:choose>
			<xsl:when test="typedata and (containers/type | type)">
				<xsl:variable name="left">
					<xsl:apply-templates select="type | (containers/type)" mode="index" />
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="type">
				<xsl:variable name="left">
					<xsl:apply-templates select="type" mode="index" />
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="simpleTextNames" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- produces a C#/C++ style generic template parameter list for inclusion in the index -->
	<xsl:template name="csTemplateText">
		<xsl:text>%3C</xsl:text>
		<xsl:call-template name="templateText" />
		<xsl:text>%3E</xsl:text>
	</xsl:template>

	<!-- produces a VB-style generic template parameter list for inclusion in the index -->
	<xsl:template name="vbTemplateText">
		<xsl:text>(Of </xsl:text>
		<xsl:call-template name="templateText" />
		<xsl:text>)</xsl:text>
	</xsl:template>

	<!-- produces a comma-seperated list of generic template parameter names -->
	<!-- comma character is URL-encoded so as not to create sub-index entries -->
	<xsl:template name="templateText">
		<xsl:for-each select="*">
			<xsl:apply-templates select="." mode="index" />
			<xsl:if test="not(position()=last())">
				<xsl:text>%2C </xsl:text>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>


	<xsl:template match="specialization | templates" mode="index" >
		<xsl:param name="name" />
		<name language="c">
			<xsl:value-of select="$name" />
			<xsl:call-template name="csTemplateText" />
		</name>
		<name language="v">
			<xsl:value-of select="$name" />
			<xsl:call-template name="vbTemplateText" />
		</name>
	</xsl:template>

	<xsl:template match="template" mode="index">
		<xsl:value-of select="@name" />
	</xsl:template>

	<xsl:template match="arrayOf" mode="index">
		<name language="c">
			<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
			<xsl:text>[</xsl:text>
			<xsl:if test="number(@rank) &gt; 1">,</xsl:if>
			<xsl:text>]</xsl:text>
		</name>
		<name language="v">
			<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
			<xsl:text>(</xsl:text>
			<xsl:if test="number(@rank) &gt; 1">,</xsl:if>
			<xsl:text>)</xsl:text>
		</name>
	</xsl:template>

	<xsl:template match="pointerTo" mode="index">
		<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
		<xsl:text>*</xsl:text>
	</xsl:template>

	<xsl:template match="referenceTo" mode="index">
		<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
	</xsl:template>

	<xsl:template match="type" mode="index">
		<xsl:call-template name="textNames" />
	</xsl:template>

	<xsl:template match="name/name">
		<xsl:variable name="lang" select="ancestor::*/@language"/>

		<xsl:if test="not(@language) or @language = $lang">
			<xsl:value-of select="."/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="name/text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template name="operatorTextNames">
		<xsl:variable name="left">
			<xsl:if test="parameters/parameter[1]">
				<xsl:choose>
					<xsl:when test="parameters/parameter[1]//specialization | parameters/parameter[1]//templates | parameters/parameter[1]//arrayOf">
						<xsl:apply-templates select="parameters/parameter[1]" mode="index" />
					</xsl:when>
					<xsl:otherwise>
						<name>
							<xsl:apply-templates select="parameters/parameter[1]" mode="index" />
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:variable name="right">
			<xsl:if test="returns[1]">
				<xsl:choose>
					<xsl:when test="returns[1]//specialization | returns[1]//templates | returns[1]//arrayOf">
						<xsl:apply-templates select="returns[1]" mode="index" />
					</xsl:when>
					<xsl:otherwise>
						<name>
							<xsl:apply-templates select="returns[1]" mode="index" />
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:call-template name="combineTextNames">
			<xsl:with-param name="left" select="msxsl:node-set($left)" />
			<xsl:with-param name="right" select="msxsl:node-set($right)" />
			<xsl:with-param name="concatenateOperator">
				<xsl:text> to </xsl:text>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

</xsl:stylesheet>
