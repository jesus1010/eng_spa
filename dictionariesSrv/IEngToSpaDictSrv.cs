using eng_spa.dictionariesSrv.types;
using System;

namespace eng_spa.dictionariesSrv
{
  interface IEngToSpaDictSrv
  {
    QueryDictResult getTranslation(string inText);
    string getDictionaryName();
    int totalEntries();
  }
}
