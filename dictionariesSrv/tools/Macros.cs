using eng_spa.dictionariesSrv.env;
using eng_spa.dictionariesSrv.types;
using eng_spa.env.dictionariesSrv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eng_spa.dictionariesSrv.tools
{
  public static class Macros
  {
    public static void userMsg(string inTag)
    {      
      GlobalRuntimeMsgsInfoSrv.infoMsg(LocationLangSrv.getTagValue(inTag) + Constants.BRLINE);
    }

    public static void userMsg(string inTag, string inMsg)
    {
      GlobalRuntimeMsgsInfoSrv.infoMsg(LocationLangSrv.getTagValue(inTag) + ":" + inMsg + Constants.BRLINE);
    }

  }
}
