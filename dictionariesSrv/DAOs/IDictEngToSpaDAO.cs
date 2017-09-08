using eng_spa.dictionariesSrv.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eng_spa.dictionariesSrv.DAOs
{
  interface IDictEngToSpaDAO
  {
    string getDictionaryName();
    QueryDictResult getEntry(string inText);
    void saveEntry(QueryDictResult inDictData);
    int totalEntries();
  }
}
