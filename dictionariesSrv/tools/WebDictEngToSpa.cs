using eng_spa.env.dictionariesSrv;
using System;
using System.Text.RegularExpressions;
using eng_spa.dictionariesSrv.types;
using eng_spa.dictionariesSrv.env;
using System.Net;

namespace eng_spa.dictionariesSrv.tools
{
  public partial class WebDictEngToSpa
  {
    #region PUBLIC
    public WebDictEngToSpa()
    {
      try
      {
        dicConf = getDictConfig();
        loadConfigFlag= true;
      }
      catch(Exception ex)
      {
        log.Debug(ex.StackTrace);
        Macros.userMsg(Constants.ERROR_LOAD_DICT_WEB_CONFIG, ex.Message);
        log.Debug("Disabling web search module, init flag to false");
        loadConfigFlag = false;
      }
    }

    //---------------------------------------------------------------------------
    // searchText
    //---------------------------------------------------------------------------
    public void searchText(string inText, QueryDictResult inResult)
    {
      if (!this.loadConfigFlag)
      {
        inResult.resetAttrs();
        return;
      }

      // search pronunciation
      try
      {
        var textNormalice= inText.Replace(" ", "+");
        Macros.userMsg(LocationLangSrv.getTagValue(Constants.SEARCHING_PR));
        MatchSiteResult result= searchFirstOcurrence(textNormalice, dicConf.Pronunciation.Sites);
        if (result.IsMatch)
        {
          var pattern= (PrPattern)result.Pattern;
          var idxPr= pattern.IdxPrGroup;
          var prData = idxPr > -1 ? result.Matches[0].Groups[idxPr + 1].Value : "";

          if (pattern.CleanResult)
          {
            inResult.pronunciation = Regex.Replace(prData, pattern.CleanPattern, "", RegexOptions.IgnoreCase);
          }
        }
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Setting pronunciation to empty string value");
        inResult.pronunciation = "";
      }

      // search translations
      try
      {
        Macros.userMsg(LocationLangSrv.getTagValue(Constants.SEARCHING_TR));
        MatchSiteResult result = this.searchFirstOcurrence(inText, this.dicConf.Translation.Sites);       
        if (result.IsMatch)
        {
          if (result.TextFound != inText)
          {
            return;
          }
          inResult.IsEmpty = false;
          var pattern = (TrPattern)result.Pattern;
          foreach (Match match in result.Matches)
          {
            var idxTr = pattern.IdxTrGroup;
            var idxTrType = pattern.IdxTrTypeGroup;

            DictTrEntry trEntry= new DictTrEntry();
            trEntry.translation= idxTr > -1 ? match.Groups[idxTr].Value : "";
            trEntry.type= idxTrType > -1 ? match.Groups[idxTrType].Value : "";

            if (pattern.CleanTypeResult)
            {
              trEntry.type = Regex.Replace(trEntry.type, pattern.CleanTypePattern, "", RegexOptions.IgnoreCase);
            }
            inResult.translations.Add(trEntry);
          }
          //inResult.translations = inResult.translations.GroupBy(x => x.translation).Select(g => g.First()).ToList();
        }
      }
      catch(Exception ex)
      {
        log.Debug(ex.StackTrace);
        log.Debug("Removing last translation entry");
        if (inResult.translations.Count > 0 )        
        {
          inResult.translations.RemoveAt(inResult.translations.Count - 1);
        }
      }
    }
    #endregion
  }
}
