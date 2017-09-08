using eng_spa.dictionariesSrv.types;
using LiteDB;
using System.Linq;
using eng_spa.dictionariesSrv.env;
using System;
using NLog;

namespace eng_spa.dictionariesSrv.DAOs
{
  public class DictEntry
  {
    public int Id { get; set; }
    public QueryDictResult EntryData { get; set; }
  }

  class SQlDictEngToSpaDAOImpl : IDictEngToSpaDAO
  {
    private static string localBaseFileName = "localDict.db";
    private static NLog.Logger log = LogManager.GetCurrentClassLogger();

    public string getDictionaryName()
    {
      return localBaseFileName;
    }

    //---------------------------------------
    // getEntry
    //---------------------------------------
    public QueryDictResult getEntry(string inText)
    {
      QueryDictResult result= new QueryDictResult("");
      try
      {
        using (var streamdb=Eviroment.getR_W_fromResource(localBaseFileName))
        using (var db = new LiteDatabase(streamdb))
        {          
          var dictionary = db.GetCollection<DictEntry>("Dictionary");
          var dictSqlReg = new DictEntry();
          dictionary.EnsureIndex(x => x.EntryData.sourceText);
          var entries = dictionary.Find(x => x.EntryData.sourceText == inText);

          if (entries.OfType<DictEntry>().Count() > 0)
          {
            result = entries.First().EntryData;
          }
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Setting result to empty object");
        result = new QueryDictResult(inText);
      }
      return result;
    }

    public void saveEntry(QueryDictResult inDictData)
    {
      try
      {
        using (var streamdb = Eviroment.getR_W_fromResource(localBaseFileName))
        using (var db = new LiteDatabase(streamdb))
        {
          var dictionary = db.GetCollection<DictEntry>("Dictionary");
          if (!inDictData.IsEmpty && !dictionary.Exists(x => x.EntryData.sourceText == inDictData.sourceText))
          {
            var dictSqlReg = new DictEntry();
            dictSqlReg.EntryData = inDictData; 
            dictionary.EnsureIndex(x => x.EntryData.sourceText);
            dictionary.Insert(dictSqlReg);
          }
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
      }
    }

    public int totalEntries()
    {
      int result= -1;
      try
      {
        using (var streamdb = Eviroment.getR_W_fromResource(localBaseFileName))
        using (var db = new LiteDatabase(streamdb))
        {
          var dictionary = db.GetCollection<DictEntry>("Dictionary");
          dictionary.EnsureIndex(x => x.EntryData.sourceText);
          int count = dictionary.FindAll().ToList<DictEntry>().Count;
          result= count;
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Setting result to -1");
        result = -1;
      }
      return result;
    }
  }
}
