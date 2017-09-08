using System;
using eng_spa.dictionariesSrv.types;
using eng_spa.dictionariesSrv.DAOs;
using eng_spa.dictionariesSrv.tools;
using NLog;

namespace eng_spa.dictionariesSrv
{
  class EngToSpaDictSrvImpl : IEngToSpaDictSrv
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private IDictEngToSpaDAO dictDAO;

    public EngToSpaDictSrvImpl()
    {
      dictDAO= new SQlDictEngToSpaDAOImpl();
    }

    public string getDictionaryName()
    {
      return dictDAO.getDictionaryName();
    }

    public int totalEntries()
    {
      return dictDAO.totalEntries();
    }

    public  QueryDictResult getTranslation(string inText)
    {
      QueryDictResult result = new QueryDictResult(inText);
      // search in local dictionary
      try
      {
        QueryDictResult localDictResult= dictDAO.getEntry(inText.ToUpper());
        if (!localDictResult.IsEmpty)
        {
          localDictResult.sourceText = localDictResult.sourceText.ToLower();
          Macros.userMsg(Constants.LOCAL_DICT_WORD_FOUND);
          return localDictResult;
        }
        else
        {
          Macros.userMsg(Constants.LOCAL_DICT_WORD_NOT_FOUND);
        }
      }
      catch(Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Setting result to empty object");
        Macros.userMsg(Constants.LOCAL_DICT_WORD_FAIL, ex.Message);
        result = new QueryDictResult(inText);
      }

      // search in web dictionary      
      try
      {
        Macros.userMsg(Constants.WEB_DICT_SEARCH);
        var webDict = new WebDictEngToSpa();
        var webResult = new QueryDictResult(inText);
        webDict.searchText(inText, webResult);
        if (!webResult.IsEmpty)
        {
          QueryDictResult resultToSave = webResult.getClone();
          // normalize text
          resultToSave.sourceText = resultToSave.sourceText.ToUpper();
          dictDAO.saveEntry(resultToSave);
          result = webResult;
          return result;
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Setting result to empty object");
        Macros.userMsg(Constants.WEB_DICT_FAIL, ex.Message);
        result = new QueryDictResult(inText);
        result.sourceText = inText;
      }
      result.sourceText = inText;
      return result;
    }
  }
}
