<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl branding"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
								xmlns:opensearch="http://a9.com/-/spec/opensearch/1.1/"
								xmlns:mshelp="http://help.microsoft.com"
								xmlns:branding="urn:FH-Branding"
								xmlns:xs="http://www.w3.org/2001/XMLSchema"
>
	<xsl:import href="Identity.xslt"/>
	<xsl:import href="ps-body.xslt"/>
	<xsl:import href="ps-head.xslt"/>
	<xsl:import href="ps-foot.xslt"/>

	<xsl:output version ="1.0"
							encoding="utf-8"
							method="xml"
							indent="no"
							omit-xml-declaration="yes"/>

	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="downscale-browser"
						 select="false()"/>
	<xsl:param name="pre-branding"
						 select="false()"/>
	<xsl:param name="catalogProductFamily"
						 select="'VS'"/>
	<xsl:param name="catalogProductVersion"
						 select="'100'"/>
	<xsl:param name="catalogLocale"
						 select="'en-US'"/>
	<xsl:param name="branding-package"
						 select="'Dev10.mshc'"/>
	<xsl:param name="launchingApp"
						 select="''"/>
	<xsl:param name="content-path"
						 select="'./'"/>

	<!-- ============================================================================================
	Global variables
	============================================================================================= -->

	<xsl:variable name="version">
		<xsl:choose>
			<xsl:when test="$catalogProductVersion">
				<xsl:value-of select="$catalogProductVersion"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:meta[@name='Microsoft.Help.TopicVersion']/@content"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="topicVersion">
		<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:meta[@name='Microsoft.Help.TopicVersion']/@content"/>
	</xsl:variable>

	<xsl:variable name="product">
		<xsl:choose>
			<xsl:when test="$catalogProductFamily">
				<xsl:value-of select="$catalogProductFamily"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="substring-before(/xhtml:html/xhtml:head/xhtml:meta[@name='Microsoft.Help.TopicVersion']/@content, '.')"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="topiclocale">
		<xsl:value-of select="translate(/xhtml:html/xhtml:head/xhtml:meta[@name='Microsoft.Help.TopicLocale']/@content, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
	</xsl:variable>
	<xsl:variable name="locale">
		<xsl:choose>
			<xsl:when test="$catalogLocale">
				<xsl:value-of select="translate($catalogLocale, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$topiclocale"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="actuallocale"
								select="branding:GetLocale($locale)"></xsl:variable>

	<xsl:variable name="topic-id"
								select="/xhtml:html/xhtml:head/xhtml:meta[@http-equiv='Content-Location']/@content" />
	<xsl:variable name="topic-id2">
		<xsl:value-of select="substring-after($topic-id,'/content/')"/>
	</xsl:variable>
	<xsl:variable name="topic-id1">
		<xsl:value-of select="substring-before($topic-id2,'/')"/>
	</xsl:variable>
	<xsl:variable name="title">
		<xsl:choose>
			<xsl:when test="/xhtml:html/xhtml:head/xhtml:meta[@name='Title']/@content">
				<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:meta[@name='Title']/@content"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:title"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="title1">
		<xsl:choose>
			<xsl:when test="contains($title,'#')">
				<xsl:value-of select="concat(substring-before($title,'#'),'%23',substring-after($title,'#'))"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$title"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="self-branded">
		<xsl:choose>
			<xsl:when test="/xhtml:html/xhtml:head/xhtml:meta[@name='SelfBranded']/@content">
				<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:meta[@name='SelfBranded']/@content"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/xhtml:html/xhtml:head/xhtml:meta[@name='Microsoft.Help.SelfBranded']/@content"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="js-file"
								select="'branding.js'"/>
	<xsl:variable name="css-file"
								select="concat('branding-',branding:GetLocale($locale),'.css')" />
	<xsl:variable name="contentFolder"
								select="branding:GetDirectoryName($content-path)"/>
	<xsl:variable name="brandingPath">
		<xsl:choose>
			<xsl:when test="$downscale-browser">
				<xsl:value-of select="concat(branding:BackslashesToFrontslashes($contentFolder), '/../branding')"/>
			</xsl:when>
			<xsl:when test="$pre-branding">
				<xsl:value-of select="concat('ms.help?',branding:BackslashesToFrontslashes($branding-package),';')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat('ms.help?',branding:EscapeBackslashes($branding-package),';')"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="mtps"
								select="'http://msdn2.microsoft.com/mtps'"/>
	<xsl:variable name="xhtml"
								select="'http://www.w3.org/1999/xhtml'"/>

	<!-- The following variables are not used but are retained to ensure all references are resolved -->
	<xsl:variable name="sp1-error-page"
								select="/xhtml:html/xhtml:head/xhtml:meta[@name='SP1ErrorPage']/@content"></xsl:variable>
	<xsl:variable name="error-page"
								select="/xhtml:html/xhtml:body/xhtml:errorLink"/>
	<xsl:variable name="f1-error-page"
								select="/xhtml:html/xhtml:body/xhtml:rss/opensearch:totalResults">
	</xsl:variable>
	<xsl:variable name="errorTitle"
								select="''"/>
	<xsl:variable name="localehelp"
								select="concat('help-',$actuallocale,'.htm')" />
	<xsl:variable name="contentnotfound"
								select="concat('contentnotfound-',$actuallocale,'.htm')" />

	<!-- ============================================================================================
	MAIN ENTRY POINT
	============================================================================================= -->

	<xsl:template match="xhtml:html"
								name="html">
		<xsl:copy>
			<xsl:apply-templates select="xhtml:head"/>
			<xsl:apply-templates select="xhtml:body"/>
		</xsl:copy>
	</xsl:template>

	<!-- ============================================================================================
	Null templates
	============================================================================================= -->

	<xsl:template match="xhtml:base"
								name="branding-base"/>

	<xsl:template match="xhtml:link[@rel='stylesheet']"
								name="branding-stylesheet">
		<xsl:if test="$self-branded != 'false'">
			<xsl:element name="link"
									 namespace="{$xhtml}">
				<xsl:attribute name="type">text/css</xsl:attribute>
				<xsl:attribute name="rel">stylesheet</xsl:attribute>
				<xsl:attribute name="href">
					<xsl:choose>
						<xsl:when test="$downscale-browser">
							<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/',@href))"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="branding:BuildContentPath($contentFolder,@href)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
			</xsl:element>
		</xsl:if>
	</xsl:template>

	<xsl:template match="xhtml:script"
								name="branding-script">
		<xsl:if test="$self-branded != 'false'">
			<xsl:copy>
				<xsl:copy-of select="@*"/>
				<xsl:comment/>
			</xsl:copy>
		</xsl:if>
	</xsl:template>

	<xsl:template match="mtps:MemberLink"
								name="branding-MemberLink"/>
	<xsl:template match="mtps:*"
								priority="-10"
								name="unSupportedControl">
		<xsl:call-template name="comment-mtps"/>
	</xsl:template>

	<!-- ============================================================================================
	Helper templates
	============================================================================================= -->

	<xsl:template name="ms-xhelp" >
		<xsl:param name="ref" select="@href|@src"/>
		<xsl:choose>
			<xsl:when test="$downscale-browser">
				<xsl:value-of select="branding:BackslashesToFrontslashes(concat($contentFolder,'/../icons/',$ref))"/>
			</xsl:when>
			<xsl:when test="$pre-branding">
				<xsl:value-of select="branding:BackslashesToFrontslashes(concat('icons/',$ref))"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat($brandingPath,branding:BackslashesToFrontslashes($ref))"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- generates comment to replace mtps control element and keep its original information-->
	<xsl:template name="comment-mtps">
		<xsl:comment>
			<xsl:value-of select="concat('[',local-name(namespace::*[.=namespace-uri(current())]),':',local-name())" />
			<xsl:apply-templates mode="cmt"
													 select="@*" />
			<xsl:value-of select="']'" />
		</xsl:comment>
		<xsl:text> </xsl:text>
	</xsl:template>

	<!-- adds attribute to a comment for an mtps control-->
	<xsl:template match="@*"
								mode="cmt"
								name="cmt-mode">
		<xsl:value-of select="concat(' ',local-name(),'=&quot;',.,'&quot;')" />
	</xsl:template>

	<!-- The following templates are not used but are retained to ensure all references are resolved -->
	<xsl:template name="MTDisclaimer">
	</xsl:template>

	<!-- ============================================================================================
	Function scripts
	============================================================================================= -->

	<msxsl:script language="C#"
								implements-prefix="branding">
		<msxsl:assembly name="System" />
		<msxsl:using namespace="System" />
		<msxsl:using namespace="System.Collections.Generic" />
		<msxsl:using namespace="System.Collections.Specialized" />
		<msxsl:using namespace="System.ComponentModel" />
		<msxsl:using namespace="System.IO" />
		<msxsl:using namespace="System.Globalization" />
		<msxsl:assembly name="System.Web" />
		<msxsl:using namespace="System.Web" />
		<![CDATA[
        string CSKeywordMatch(Match match){
            return " <span xmlns='http://www.w3.org/1999/xhtml' class='CSLanguage'>" + match.Groups[1].Value + " </span>";
        }

        Regex rep =
        new Regex(
            @"(\b(abstract|event|new|struct|as|explicit|null|switch|base|extern|object|this|bool|false|operator|throw|break|finally|out|true|byte|fixed|override|try|case|float|params|typeof|catch|for|private|uint|char|foreach|protected|ulong|checked|goto|public|unchecked|class|if|readonly|unsafe|const|implicit|ref|ushort|continue|in|return|using|decimal|int|sbyte|virtual|default|interface|sealed|volatile|delegate|internal|short|void|do|is|sizeof|while|double|lock|stackalloc|else|long|static|enum|namespace|string)\b)");
        public XPathNodeIterator CSKeyword(string input)
        {
          XmlDocument xdoc = new XmlDocument();
          XmlElement ele = xdoc.CreateElement("branding:Root", "urn:FH-Brandng");
          ele.InnerText = input;
          xdoc.AppendChild(ele);
          String s = ele.OuterXml;
          string s1 = rep.Replace(s, CSKeywordMatch);
          xdoc.LoadXml(s1);
          XPathNavigator nav = xdoc.CreateNavigator();
          return nav.Select("/*/node()");
        }
        public string EscapeBackslashes(string input)
        {
          return input.Replace(@"\",@"\\");
        }
        public string BackslashesToFrontslashes(string input)
        {
          return input.Replace(@"\",@"/");
        }
        public string GetUrlEncode(string input)
        {
            //return HttpUtility.UrlEncode(input);
            return input;
        }
        // Replaces no-op GetUrlEncode - fix this!
        public string GetUrlEncode2(string input)
        {
            return HttpUtility.UrlEncode(input);
        }
        public string GetUrlDecode(string input)
        {
            return HttpUtility.UrlDecode(input);
        }
        public string GetHtmlDecode(string input)
        {
            return HttpUtility.HtmlDecode(input);
        }
        public string replace(string source,string replaceto,string replacewith)
        {
           return source.Replace(replaceto,replacewith);
        }
        public string GetSubString(string source,int index)
        {
          string[] subStrings = source.Split(new String[]{"<a>","</a>"}, StringSplitOptions.None);
          if (index < subStrings.Length)
          {
            return subStrings[index];
          }
          else
          {
            return "";
          }
        }
        public string GetAnchorTagValue(string source,string value)
        {
          MatchCollection matches = Regex.Matches(source, @"<a\shref=""(?<url>.*?)"">(?<text>.*?)</a>");
          if(value == "tag")
             return matches[0].Groups["url"].Value;
          else if (value== "text")
            return matches[0].Groups["text"].Value;
          else if (value== "preText")
             return source.Substring(0, source.IndexOf("<"));
          else
             return "";
              
        }
       public string GetLocale(string locale)
        {
            string[] locales = { "ja-jp", "de-de", "es-es", "fr-fr", "it-it", "ko-kr", "pt-br", "ru-ru", "zh-cn", "zh-tw", "cs-cz", "pl-pl", "tr-tr" };
            int i = 0;
            while (i < locales.Length)
            {
                if (string.Equals(locale, locales[i], StringComparison.CurrentCultureIgnoreCase))
                    return locale;
                i++;
            }
            return "en-US";
        }
        public string GetID(string href)
        {
            string str = String.Empty;
            href = href.ToUpper();
            if (href.ToUpper().Contains("?ID"))
            {
                str = href.Substring(href.IndexOf("?ID=") + 4, href.Length - href.IndexOf("?ID=") - 4);
                if (str.Contains("&"))
                {
                    str = str.Substring(0, str.IndexOf("&"));
                }
            }
           return str;
        }
        public string GetOnlineID(string href)
        {
          if(href.Contains("appId=Dev10IDEF1"))
            return href;
          int idStart = href.LastIndexOf('/');
          if (idStart > -1)
          {
            idStart++;
            string urlPre = href.Substring(0, idStart);
            string urlPost = href.Substring(idStart);

            // Remove type prefix
            int pos = urlPost.IndexOf(':');
            if (pos > -1)
            {
              urlPost = urlPost.Substring(pos + 1);
            }

            // Remove product version
            int verStart = urlPost.LastIndexOf('(');
            int verEnd = urlPost.LastIndexOf(')');
            if (verStart > -1
             && verEnd > -1)
            {
              urlPost = urlPost.Remove(verStart, verEnd - verStart + 1);
            }
            return urlPre + urlPost;
          }
          return href;
        }
        public string GetDirectoryName(string filePath)
        {
          if (String.IsNullOrEmpty(filePath)) return String.Empty;

          return BackslashesToFrontslashes(Path.GetDirectoryName(filePath));
        }
        public string BuildContentPath(string directoryName,string filePath)
        {
          string tempDirName = directoryName;
          if (tempDirName[0] == '/') tempDirName = tempDirName.Substring(1);
          string tempPath = filePath;
          int pathSeparator = tempPath.IndexOf("../");
          while (pathSeparator != -1)
          {
            int dirSeparator = tempDirName.LastIndexOf("/");
            tempDirName = (dirSeparator != -1)?tempDirName.Substring(0,dirSeparator):"";
            string firstPart = tempPath.Substring(0,pathSeparator);
            string secondPart = tempPath.Substring(pathSeparator+2);
            tempPath = firstPart + tempDirName + secondPart;
            pathSeparator = tempPath.IndexOf("../");
          }
          return tempPath;
        }
    ]]>
	</msxsl:script>

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
