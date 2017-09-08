using eng_spa.dictionariesSrv.env;
using eng_spa.dictionariesSrv.types;
using eng_spa.env.dictionariesSrv;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace eng_spa.dictionariesSrv.tools
{
  public partial class WebDictEngToSpa
  {
    #region ATTRS
    private static Logger log = LogManager.GetCurrentClassLogger();

    private const string DICT_CONF_FILE= "dictionary_conf.xml";
    private DictionaryConfig dicConf;
    private bool loadConfigFlag;

    #endregion ATTRS

    #region XML_MAPPING_TYPES
    public abstract class Pattern
    {
      [XmlAttribute]
      public string Lang { get; set; } = "";

      [XmlAttribute]
      public string Type { get; set; }= "";

      public string RegularEx { get; set; } = "";

      public string TextFoundPatter { get; set; } = "";

      public string TextFoundNormPatter { get; set; } = "";

      public string TextFoundNormReplace { get; set; } = "";

      [XmlAttribute]
      public bool SearchTextFound { get; set; } = false;

    }

    public class PrPattern: Pattern
    {
      [XmlAttribute]
      public int IdxPrGroup { get; set; }= -1;

      [XmlAttribute]
      public bool CleanResult { get; set; }= false;

      public string CleanPattern { get; set; }= "";
    }

    public class TrPattern: Pattern
    {
      [XmlAttribute]
      public int IdxTrGroup { get; set; }= -1;

      [XmlAttribute]
      public int IdxTrTypeGroup { get; set; } = -1;

      [XmlAttribute]
      public bool CleanTypeResult { get; set; }= false;

      public string CleanTypePattern { get; set; }= "";

    }

    public abstract class Site
    {
      public string Url { get; set; } = "http://";
      abstract public Pattern[] ParternList { get;}
    }

    public class PrSite: Site
    {
      public override Pattern[] ParternList {
        get
        {
          return (Pattern[]) Patterns;
        }
      }
      public PrPattern[] Patterns { get; set; }= new PrPattern[1];
    }

    public class TrSite: Site
    {
      public override Pattern[] ParternList {
        get
        {
          return (Pattern[]) Patterns;
        }
      }
      public TrPattern[] Patterns { get; set; }= new TrPattern[1];
    }

    public class Pronunciation
    {
      public PrSite[] Sites { get; set; } = new PrSite[1];
    }

    public class Translation
    {
      public TrSite[] Sites { get; set; }= new TrSite[1];
    }

    [XmlRootAttribute("DictionaryConfig", Namespace = "http://DictionaryConfig.com",
    IsNullable = false)]
    public class DictionaryConfig
    {
      public Pronunciation Pronunciation { get; set; }= new Pronunciation();
      public Translation Translation { get; set; }= new Translation();
    }
    #endregion

    class MatchSiteResult
    {
      public string TextFound { get; set; } = "";
      public Site Site { get; set; }
      public Pattern Pattern { get; set; }
      public MatchCollection Matches { get; set; }
      public bool IsMatch { get; set; } = false;
    }

    #region HELPERS
    private void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
    {
      log.Debug("Unknown Node:" + e.Name + "\t" + e.Text);
    }

    private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
    {
      System.Xml.XmlAttribute attr = e.Attr;
      log.Debug("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
    }

    private DictionaryConfig getDictConfig()
    {
      log.Debug(">>Loading web dictionary config...");
      DictionaryConfig result = new DictionaryConfig();
      try
      {
        XmlSerializer serializer = new XmlSerializer(typeof(DictionaryConfig));

        /* If the XML document has been altered with unknown 
        nodes or attributes, handle them with the 
        UnknownNode and UnknownAttribute events.*/
        serializer.UnknownNode += new

        XmlNodeEventHandler(serializer_UnknownNode);
        serializer.UnknownAttribute += new

        XmlAttributeEventHandler(serializer_UnknownAttribute);

        // A FileStream is needed to read the XML document.
        using (StreamReader fs = Eviroment.getR_fromAsset(DICT_CONF_FILE))
        {
          result = (DictionaryConfig)serializer.Deserialize(fs);
        }
        // Declare an object variable of the type to be deserialized.
        /* Use the Deserialize method to restore the object's state with
        data from the XML document. */
        // Read the order date.
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Creating empty dictionary config");
        result = new DictionaryConfig();
      }
      return result;
    }

    
    private MatchSiteResult searchFirstOcurrence(string inText,
                                                 Site[] inSites)
    {
      log.Debug(">>");
      MatchSiteResult result = new MatchSiteResult();
      try
      {
        foreach (var site in inSites)
        {
          using (WebClient web = new WebClient())       
          {
            var fullUrl = string.Format(site.Url + "{0}", inText);
            var htmlSite = "";
            // if there is poor signal, try again
            int webTryCounter = 3;
            while (webTryCounter > 0)
            {
              try
              {
                htmlSite = web.DownloadString(fullUrl);
                webTryCounter = 0;
              }
              catch (Exception ex)
              {
                log.Debug(ex.StackTrace);
                log.Debug("Poor signal, try again");
                Macros.userMsg(Constants.WEAK_NETWORK_SIGNAL);
                webTryCounter--;
                htmlSite = "";
              }
            }

            Macros.userMsg(Constants.SEARCHING_SITE, fullUrl);
            foreach (var pattern in site.ParternList)
            {
              if (pattern.SearchTextFound)
              {
                // search text found in result
                var regexSearchText = new Regex(@pattern.TextFoundPatter, RegexOptions.IgnoreCase);
                var matchesSearchText = regexSearchText.Matches(htmlSite);
               
                if (matchesSearchText.Count > 0)
                {
                  var matches = matchesSearchText[0].Groups;

                  if (matches.Count>=2)
                  {
                    result.TextFound= Regex.Replace(matches[1].Value, pattern.TextFoundNormPatter, pattern.TextFoundNormReplace, RegexOptions.IgnoreCase);
                  }
                }
              }

              var regexPr = new Regex(@pattern.RegularEx, RegexOptions.IgnoreCase);
              var matchesResult = regexPr.Matches(htmlSite);

              if (matchesResult.Count > 0)
              {
                result.Site = site;
                result.Pattern = pattern;
                result.Matches = matchesResult;
                result.IsMatch = true;
                return result;
              }
            }
          }           
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Creating empty dictionary config");
        string info = LocationLangSrv.getTagValue(Constants.WEB_DICT_FAIL) + ":" + ex.Message + Constants.BRLINE;
        GlobalRuntimeMsgsInfoSrv.infoMsg(info);
        Macros.userMsg(Constants.WEB_DICT_FAIL, ex.Message);
        result = new MatchSiteResult();
      }
      log.Debug("<<");
      return result;
    }    
    #endregion CONFIG_HELPERS
  }
}
