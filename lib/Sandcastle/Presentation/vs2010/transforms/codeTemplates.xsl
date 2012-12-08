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

	<xsl:import href="globalTemplates.xsl"/>

	<!-- ============================================================================================
	Code languages
	============================================================================================= -->

	<xsl:template name="t_codeLang">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangLC"
									select="translate($p_codeLang,$g_allUpperCaseLetters,$g_allLowerCaseLetters)"/>
		<xsl:choose>
			<xsl:when test="$v_codeLangLC = 'vbs' or $v_codeLangLC = 'vbscript'">
				<xsl:text>VBScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vb' or $v_codeLangLC = 'vb#' or $v_codeLangLC = 'vb.net' or $v_codeLangLC = 'vbnet' or $v_codeLangLC = 'kblangvb' or $v_codeLangLC = 'visualbasic'" >
				<xsl:text>VisualBasic</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'visualbasicdeclaration'" >
				<xsl:text>VisualBasicDeclaration</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'visualbasicusage'" >
				<xsl:text>VisualBasicUsage</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'c#' or $v_codeLangLC = 'cs' or $v_codeLangLC = 'csharp'" >
				<xsl:text>CSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vb-c#' or $v_codeLangLC = 'visualbasicandcsharp'">
				<xsl:text>visualbasicANDcsharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'cpp' or $v_codeLangLC = 'cpp#' or $v_codeLangLC = 'c' or $v_codeLangLC = 'c++' or $v_codeLangLC = 'kblangcpp' or $v_codeLangLC = 'managedcplusplus'" >
				<xsl:text>ManagedCPlusPlus</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'j#' or $v_codeLangLC = 'jsharp'">
				<xsl:text>JSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'f#' or $v_codeLangLC = 'fs' or $v_codeLangLC = 'fsharp'">
				<xsl:text>FSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'js' or $v_codeLangLC = 'jscript#' or $v_codeLangLC = 'jscript' or $v_codeLangLC = 'kbjscript'">
				<xsl:text>JScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'javascript'">
				<xsl:text>JavaScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'xml' or $v_codeLangLC = 'xmllang'">
				<xsl:text>xmlLang</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'html'">
				<xsl:text>html</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'xaml'">
				<xsl:text>XAML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'aspnet'">
				<xsl:text>AspNet</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'pshell'">
				<xsl:text>pshell</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'sql'">
				<xsl:text>sql</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'none'">
				<xsl:value-of select="$v_codeLangLC"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>other</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_codeLangName">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangUnique">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang"
												select="$p_codeLang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$v_codeLangUnique = 'VBScript'">
				<xsl:text>VBScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'VisualBasic' or $v_codeLangUnique = 'VisualBasicUsage' or $v_codeLangUnique = 'VisualBasicDeclaration'" >
				<xsl:text>VB</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'CSharp'" >
				<xsl:text>CSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'visualbasicANDcsharp'" >
				<xsl:text>VB and C#</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'ManagedCPlusPlus'" >
				<xsl:text>C++</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JSharp'">
				<xsl:text>VJ#</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'FSharp'" >
				<xsl:text>FSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JScript'">
				<xsl:text>JScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JavaScript'">
				<xsl:text>JavaScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'XAML'">
				<xsl:text>XAML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'xmlLang'">
				<xsl:text>XML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'html'">
				<xsl:text>HTML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'AspNet'">
				<xsl:text>ASP.NET</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'pshell'">
				<xsl:text>PowerShell</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'sql'">
				<xsl:text>SQL</xsl:text>
			</xsl:when>
			<xsl:otherwise/>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_codeLangTitleId">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangUnique">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang"
												select="$p_codeLang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$v_codeLangUnique = 'VisualBasic'" >
				<xsl:value-of select="'devlang_VisualBasic'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'VBScript'">
				<xsl:value-of select="'devlang_VBScript'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'VisualBasicDeclaration'" >
				<xsl:value-of select="'devlang_VisualBasicDeclaration'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'VisualBasicUsage'" >
				<xsl:value-of select="'devlang_VisualBasicUsage'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'CSharp'" >
				<xsl:value-of select="'devlang_CSharp'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'visualbasicANDcsharp'" >
				<xsl:value-of select="'devlang_visualbasicANDcsharp'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'ManagedCPlusPlus'" >
				<xsl:value-of select="'devlang_ManagedCPlusPlus'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JSharp'">
				<xsl:value-of select="'devlang_JSharp'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'FSharp'" >
				<xsl:value-of select="'devlang_FSharp'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JScript'">
				<xsl:value-of select="'devlang_JScript'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JavaScript'">
				<xsl:value-of select="'devlang_JavaScript'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'XAML'">
				<xsl:value-of select="'devlang_XAML'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'xmlLang'">
				<xsl:value-of select="'devlang_xmlLang'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'html'">
				<xsl:value-of select="'devlang_html'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'AspNet'">
				<xsl:value-of select="'devlang_AspNet'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'pshell'">
				<xsl:value-of select="'devlang_pshell'"/>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'sql'">
				<xsl:value-of select="'devlang_sql'"/>
			</xsl:when>
			<xsl:otherwise/>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_mshelpCodelangAttributes">
		<xsl:param name="snippets"/>
		<xsl:for-each select="$snippets">

			<xsl:if test="not(@language=preceding::*/@language)">
				<xsl:variable name="v_codeLang">
					<xsl:choose>
						<xsl:when test="@language = 'VBScript' or @language = 'vbs'">
							<xsl:text>VBScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'VisualBasic' or @language = 'vb' or @language = 'vb#' or @language = 'VB' or @language = 'kbLangVB'" >
							<xsl:text>kbLangVB</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'CSharp' or @language = 'c#' or @language = 'cs' or @language = 'C#'" >
							<xsl:text>CSharp</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'ManagedCPlusPlus' or @language = 'cpp' or @language = 'cpp#' or @language = 'c' or @language = 'c++' or @language = 'C++' or @language = 'kbLangCPP'" >
							<xsl:text>kbLangCPP</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JSharp' or @language = 'j#' or @language = 'jsharp' or @language = 'VJ#'">
							<xsl:text>VJ#</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JScript' or @language = 'js' or @language = 'jscript#' or @language = 'jscript' or @language = 'JScript' or @language = 'kbJScript'">
							<xsl:text>kbJScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'XAML' or @language = 'xaml'">
							<xsl:text>XAML</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JavaScript' or @language = 'javascript'">
							<xsl:text>JavaScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'xml'">
							<xsl:text>xml</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'html'">
							<xsl:text>html</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'vb-c#'">
							<xsl:text>visualbasicANDcsharp</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>other</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$v_codeLang='other'"/>
					<!-- If $v_codeLang is already authored, then do nothing -->
					<xsl:when test="/document/metadata/attribute[@name='codelang']/text() = $v_codeLang"/>
					<xsl:otherwise>
						<xsl:call-template name="t_codeLangAttr">
							<xsl:with-param name="p_codeLang"
															select="$v_codeLang"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>

		</xsl:for-each>
	</xsl:template>

	<xsl:template name="t_codeLangAttr">
		<xsl:param name="p_codeLang"/>
		<MSHelp:Attr Name="codelang"
								 Value="{$p_codeLang}"/>
	</xsl:template>

	<!-- ============================================================================================
	Code sections
	============================================================================================= -->

	<xsl:template name="t_putCodeSections">
		<xsl:param name="p_codeNodes"/>
		<xsl:param name="p_nodeCount"/>
		<xsl:param name="p_codeLangAttr"
							 select="''"/>
		<xsl:param name="p_transformCode"
							 select="false()"/>
		<xsl:param name="p_enableCopyCode"
							 select="true()"/>

		<xsl:for-each select="msxsl:node-set($p_codeNodes)">
			<xsl:choose>
				<xsl:when test="$p_codeLangAttr!=''">
					<xsl:variable name="v_codeLang">
						<xsl:call-template name="t_codeLang">
							<xsl:with-param name="p_codeLang">
								<xsl:for-each select="@*">
									<xsl:if test="name() = $p_codeLangAttr">
										<xsl:value-of select="."/>
									</xsl:if>
								</xsl:for-each>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:variable>
					<xsl:call-template name="t_putCodeSection">
						<xsl:with-param name="p_codeLang"
														select="$v_codeLang"/>
						<xsl:with-param name="p_transformCode"
														select="$p_transformCode"/>
						<xsl:with-param name="p_enableCopyCode"
														select="$p_enableCopyCode"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="t_putCodeSection">
						<xsl:with-param name="p_transformCode"
														select="$p_transformCode"/>
						<xsl:with-param name="p_enableCopyCode"
														select="$p_enableCopyCode"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="t_putCodeSection">
		<xsl:param name="p_codeLang">
			<xsl:choose>
				<xsl:when test="@lang">
					<xsl:choose>
						<xsl:when test="(@title!='') and (normalize-space(@title)='')">
							<xsl:value-of select="'none'"/>
						</xsl:when>
						<xsl:when test="(normalize-space(@lang)='none') and (normalize-space(@title)!='')">
							<xsl:value-of select="'other'"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@lang"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@language"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:param>
		<xsl:param name="p_codeTitle">
			<xsl:value-of select="@title"/>
		</xsl:param>
		<xsl:param name="p_transformCode"
							 select="false()"/>
		<xsl:param name="p_enableCopyCode"
							 select="true()"/>

		<xsl:variable name="v_codeLangUnique">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang"
												select="$p_codeLang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="v_codeLangTitle">
			<xsl:if test="(normalize-space($p_codeTitle)!='') and (normalize-space($p_codeTitle)!=$v_codeLangUnique)">
				<xsl:value-of select="$p_codeTitle"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="v_codeLangTitleId">
			<xsl:call-template name="t_codeLangTitleId">
				<xsl:with-param name="p_codeLang"
												select="$v_codeLangUnique"/>
			</xsl:call-template>
		</xsl:variable>
		<!--<xsl:comment xml:space="preserve">p_codeLang[<xsl:value-of select="$p_codeLang"/>]</xsl:comment>
		<xsl:comment xml:space="preserve">p_codeTitle[<xsl:value-of select="$p_codeTitle"/>]</xsl:comment>
		<xsl:comment xml:space="preserve">v_codeLangUnique[<xsl:value-of select="$v_codeLangUnique"/>]</xsl:comment>
		<xsl:comment xml:space="preserve">v_codeLangTitle[<xsl:value-of select="$v_codeLangTitle"/>]</xsl:comment>
		<xsl:comment xml:space="preserve">v_codeLangTitleId[<xsl:value-of select="$v_codeLangTitleId"/>]</xsl:comment>
		<xsl:comment xml:space="preserve">p_transformCode[<xsl:value-of select="$p_transformCode"/>]</xsl:comment>-->

		<!-- Post-branding treats 'other' and 'none' alike, so avoid using 'none' as the Language. The result is: -->
		<!--   If Language is 'other' a tab is formatted using DisplayLanguage. -->
		<!--   If Language is '' no tab is formatted. -->
		<xsl:element name="mtps:CodeSnippet"
								 namespace="{$mtps}">
			<xsl:attribute name="runat">
				<xsl:value-of select="'server'"/>
			</xsl:attribute>
			<xsl:attribute name="ContainsMarkup">
				<xsl:choose>
					<xsl:when test="starts-with(normalize-space(.),'@@_')">
						<xsl:value-of select="'true'"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="'false'"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="EnableCopyCode">
				<xsl:value-of select="string($p_enableCopyCode)"/>
			</xsl:attribute>
			<xsl:choose>
				<xsl:when test="$v_codeLangUnique='none'">
					<xsl:attribute name="Language"/>
					<xsl:attribute name="DisplayLanguage"/>
				</xsl:when>
				<xsl:when test="$v_codeLangUnique='other'">
					<xsl:attribute name="DisplayLanguage"/>
					<xsl:choose>
						<xsl:when test="$v_codeLangTitle!=''">
							<xsl:attribute name="Language">
								<xsl:value-of select="$v_codeLangTitle"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="$v_codeLangTitleId!=''">
							<includeAttribute name="Language"
																item="{$v_codeLangTitleId}"/>
						</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="Language">
						<xsl:value-of select="$v_codeLangUnique"/>
					</xsl:attribute>
					<xsl:choose>
						<xsl:when test="$v_codeLangTitle!=''">
							<xsl:attribute name="DisplayLanguage">
								<xsl:value-of select="$v_codeLangTitle"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="$v_codeLangTitleId!=''">
							<includeAttribute name="DisplayLanguage"
																item="{$v_codeLangTitleId}"/>
						</xsl:when>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>

			<xsl:choose>
				<xsl:when test="starts-with(normalize-space(.),'@@_')">
					<!-- MS Help Viewer has code to show the code colorized or plain.  We'll ignore their colorizer and insert our own colorized text later. -->
					<xsl:element name="div">
						<xsl:attribute name="class">
							<xsl:value-of select="'code'"/>
						</xsl:attribute>
						<xsl:value-of select="."/>
					</xsl:element>
				</xsl:when>
				<xsl:otherwise>
						<xsl:choose>
						<xsl:when test="$p_transformCode">
							<xsl:element name="pre"
													 namespace="{$xhtml}"
													 xml:space="preserve"><xsl:call-template name="t_tranformCodeContainer"/></xsl:element>
						</xsl:when>
						<xsl:otherwise>
							<xsl:element name="pre"
													 namespace="{$xhtml}"
													 xml:space="preserve"><xsl:call-template name="t_copyCodeContainer"/></xsl:element>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="*"
								mode="transformCode"
								name="t_translateCodeElement">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="code|pre|div"
								mode="transformCode"
								name="t_tranformCodeContainer">
		<xsl:apply-templates mode="transformCode"/>
	</xsl:template>

	<xsl:template match="text()"
								mode="transformCode"
								name="t_translateCodeText">
		<xsl:call-template name="t_copyCodeText"/>
	</xsl:template>

	<!-- ======================================================================================== -->

	<!-- MAML elements are transformed, even if the code is not.  This supports the ddue:legacy* elements -->
	<xsl:template match="ddue:*"
								mode="copyCode"
								name="t_copyCodeDdueElement">
		<xsl:apply-templates select="."/>
	</xsl:template>

	<xsl:template match="*"
								mode="copyCode"
								name="t_copyCodeElement">
		<xsl:choose>
			<!-- The span element can be interpreted as xhtml:span or ddue:span so special processing is required -->
			<xsl:when test="local-name()='span'">
				<xsl:element name="span"
										 namespace="{$xhtml}">
					<xsl:copy-of select="@*"/>
					<xsl:apply-templates mode="copyCode"/>
					<xsl:if test="not(node())">
						<xsl:value-of select="''"/>
					</xsl:if>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy>
					<xsl:copy-of select="@*"/>
					<xsl:apply-templates mode="copyCode"/>
					<xsl:if test="not(node()) and not(self::br) and not(self::hr)">
						<xsl:value-of select="''"/>
					</xsl:if>
				</xsl:copy>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="code|pre|div"
								mode="copyCode"
								name="t_copyCodeContainer">
		<xsl:choose>
			<xsl:when test="contains($minimal-spacing,'code')">
				<xsl:for-each select="node()">
					<xsl:choose>
						<xsl:when test="self::ddue:*">
							<xsl:call-template name="t_copyCodeDdueElement"/>
						</xsl:when>
						<xsl:when test="self::*">
							<xsl:call-template name="t_copyCodeElement"/>
						</xsl:when>
						<xsl:when test="self::text() and normalize-space(.)='' and contains(.,'&#10;') and not(preceding-sibling::node())">
						</xsl:when>
						<xsl:when test="self::text() and normalize-space(.)='' and contains(.,'&#10;') and not(following-sibling::*)">
						</xsl:when>
						<xsl:when test="self::text() and normalize-space(.)!='' and contains(.,'&#10;') and not(following-sibling::*)">
							<xsl:call-template name="t_copyCodeText">
								<xsl:with-param name="p_text"
																select="ddue:TrimEol(.)"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="t_copyCodeText"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates mode="copyCode"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="text()"
								mode="copyCode"
								name="t_copyCodeText">
		<xsl:param name="p_text"
							 select="."/>
		<xsl:choose>
			<xsl:when test="$p_text=' ' or $p_text='&#160;'">
				<xsl:value-of select="'&#160;'"/>
			</xsl:when>
			<xsl:when test="normalize-space($p_text)='' and contains($p_text,'&#10;')">
				<xsl:value-of select="concat('&#160;','&#10;',substring-after(translate($p_text,' &#13;','&#160;'),'&#10;'))"/>
			</xsl:when>
			<xsl:when test="normalize-space($p_text)='' and contains($p_text,'&#13;')">
				<xsl:value-of select="concat('&#160;','&#10;',substring-after(translate($p_text,' ','&#160;'),'&#13;'))"/>
			</xsl:when>
			<xsl:when test=".!='' and normalize-space(.)=''">
				<xsl:value-of select="translate(.,' ','&#160;')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$p_text"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
