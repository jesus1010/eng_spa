using eng_spa.dictionariesSrv.tools;
using eng_spa.env.dictionariesSrv;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace eng_spa.dictionariesSrv.env
{

  [XmlRootAttribute("LocationLangStore", Namespace = "http://location_eng.com", IsNullable = false)]
  public class LocationLangStore
  {
    public string LangId { get; set; } = "";
    public List<LocationLangTag> LangTags { get; set; } = new List<LocationLangTag>();
  }

  public class LocationLangTag
  {
    public string IdTag { get; set; } = "";
    public string Text { get; set; } = "";
  }

  public static class LocationLangSrv
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    private const string DEFAULT_ENG_LANG_FILE= "location_eng.xml";
    private const string DEFAULT_SPA_LANG_FILE = "location_spa.xml";

    private static LocationLangStore locationStore = new LocationLangStore();

    public static string getTagValue(string inTag)
    {
      // if not found return in tag
      string result = inTag;

      var tagFound= locationStore.LangTags.Find(x => x.IdTag == inTag);
      if (tagFound != null)
      {
        result = tagFound.Text;
      }
      return result;
    }

    public static void changeToEng()
    {
      locationStore= loadTags(DEFAULT_ENG_LANG_FILE);
    }

    public static void changeToSpa()
    {
      locationStore= loadTags(DEFAULT_SPA_LANG_FILE);
    }

    private static LocationLangStore loadTags(string inTagFile)
    {
      LocationLangStore result= new LocationLangStore();
      try
      {
        XmlSerializer serializer = new XmlSerializer(typeof(LocationLangStore));

        /* If the XML document has been altered with unknown 
        nodes or attributes, handle them with the 
        UnknownNode and UnknownAttribute events.*/
        serializer.UnknownNode += new

        XmlNodeEventHandler(serializer_UnknownNode);
        serializer.UnknownAttribute += new

        XmlAttributeEventHandler(serializer_UnknownAttribute);

        using (StreamReader fs = Eviroment.getR_fromAsset(inTagFile))
        {
          // Declare an object variable of the type to be deserialized.
          /* Use the Deserialize method to restore the object's state with
          data from the XML document. */
          // Read the order date.
          result = (LocationLangStore)serializer.Deserialize(fs);
        }
      }
      catch(Exception ex)
      {
        log.Debug(ex.StackTrace);
        GlobalRuntimeMsgsInfoSrv.infoMsg("Error loading language TAGS, loaded defaulf tags");
        log.Debug("Init result to empty location store");
        result = new LocationLangStore();
      }
      return result;
    }

    private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
    {
      log.Debug("Unknown Node:" + e.Name + "\t" + e.Text);
    }

    private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
    {
      System.Xml.XmlAttribute attr = e.Attr;
      log.Debug("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
    }
  }
}
