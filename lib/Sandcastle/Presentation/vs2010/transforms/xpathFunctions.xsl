<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        >
	<!-- ======================================================================================== -->

	<msxsl:script language="C#" implements-prefix="ddue">
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Globalization"/>
    <msxsl:using namespace="System.Text.RegularExpressions" />
		<msxsl:assembly name="System.Web" />
		<msxsl:using namespace="System.Web" />
		<![CDATA[
        public string GetUrlEncode(string input)
        {
            return HttpUtility.UrlEncode(input);
        }
        public string GetUrlDecode(string input)
        {
            return HttpUtility.UrlDecode(input);
        }
        public string GetHtmlEncode(string input)
        {
            return HttpUtility.HtmlEncode(input);
        }
        public string GetHtmlDecode(string input)
        {
            return HttpUtility.HtmlDecode(input);
        }

				public static string ToUpper(string id)
				{
						return id.Trim().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
				}
				public static string TrimEnd(string input)
				{
						return input.TrimEnd();
				}
				public static string TrimEol(string input)
				{
						return input.TrimEnd('\r','\n','\t',' ');
				}

				//Regular expression to check that a string is in a valid Guid representation.
				private static Regex guidChecker = new Regex("[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}", RegexOptions.None);
				public static string GuidChecker(string id)
				{
						return guidChecker.IsMatch(id).ToString();
				}
      
				public static string CompareDate(string RTMReleaseDate, string changedHistoryDate)
				{
						CultureInfo culture = CultureInfo.InvariantCulture;
						DateTime dt1 = DateTime.MinValue;
						DateTime dt2 = DateTime.MinValue;
        
						try
						{
							dt1 = DateTime.Parse(RTMReleaseDate, culture);
						}
						catch (FormatException)
						{
							Console.WriteLine(string.Format("Error: CompareDate: Unable to convert '{0}' for culture {1}.", RTMReleaseDate, culture.Name));
							return "notValidDate";
						}
        
						try
						{
							dt2 = DateTime.Parse(changedHistoryDate,culture);
						}
						catch (FormatException)
						{
							Console.WriteLine(string.Format("Error: CompareDate: Unable to convert '{0}' for culture {1}.", changedHistoryDate, culture.Name));
							return "notValidDate";
						}
       
						if (DateTime.Compare(dt2, dt1) > 0) return changedHistoryDate;
						else return RTMReleaseDate;
				}

				public static string IsValidDate(string dateString)
				{
						CultureInfo culture = CultureInfo.InvariantCulture;
						DateTime dt = DateTime.MinValue;
        
						try
						{
							dt = DateTime.Parse(dateString, culture);
						}
						catch (FormatException)
						{
							Console.WriteLine(string.Format("Error: IsValidDate: Unable to convert '{0}' for culture {1}.", dateString, culture.Name));
							return "false";
						}
						return "true";
				}

    ]]>
  </msxsl:script>
</xsl:stylesheet>