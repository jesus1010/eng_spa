using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace eng_spa.dictionariesSrv.types
{
  [Serializable]
  public class DictTrEntry
  {
    public string translation { get; set; } = "";
    public string type { get; set; } = "";

    public void resetAttrs()
    {
      this.translation = "";
      this.type = "";
    }
  }

  [Serializable]
  public class QueryDictResult
  {
    public bool IsEmpty { get; set; } = true;
    public string sourceText { get; set; } = "";
    public string pronunciation { get; set; } = "";
    public List<DictTrEntry> translations { get; set; } = new List<DictTrEntry>();

    public QueryDictResult(string inSrcText)
    {
      sourceText = inSrcText;
    }

    public QueryDictResult()
    {
    }

    public QueryDictResult getClone()
    {
      IFormatter formatter = new BinaryFormatter();
      Stream stream = new MemoryStream();
      using (stream)
      {
        formatter.Serialize(stream, this);
        stream.Seek(0, SeekOrigin.Begin);
        return (QueryDictResult)formatter.Deserialize(stream);
      }
    }

    public void resetAttrs()
    {
      IsEmpty= true;
      sourceText= "";
      pronunciation= "";
      translations = new List<DictTrEntry>();
  }
}

  public class QueryDictResultFormater
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    private static string SRC_TEXT_PR_FORMAT= ">>{0}|/{1}/" + Constants.BRLINE;
    private static string TR_TYPE_FORMAT = "  {0,-16} {1,-10}" + Constants.BRLINE;

    public static string toSimpleText(QueryDictResult inData)
    {
      string result = "";
      try
      {
        if (!inData.IsEmpty)
        {
          // original text, pronunciation
          result = string.Format(SRC_TEXT_PR_FORMAT, inData.sourceText, inData.pronunciation);
          byte[] bytes = Encoding.Default.GetBytes(result);
          result = Encoding.UTF8.GetString(bytes);

          foreach (var entry in inData.translations)
          {
            var tr = entry.translation;
            bytes = Encoding.Default.GetBytes(tr);
            tr = Encoding.UTF8.GetString(bytes);

            var type = entry.type;
            bytes = Encoding.Default.GetBytes(type);
            type = Encoding.UTF8.GetString(bytes);

            result += string.Format(TR_TYPE_FORMAT, tr.PadRight(16, '.'), type.PadLeft(2, '-'));
          }
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Init result to empty string");
        result = "";
      }
      return result;
    }
  }
}
