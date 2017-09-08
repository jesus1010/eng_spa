using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace eng_spa.dictionariesSrv.env
{
  static class Eviroment
  {
    public static string RESOURCES_DIR { get; set; } = "/resources/";
    public static Func<string, StreamReader> assetHandlerR { get; set; }
    public static Func<string, Stream> resourceHandlerR_W { get; set; }

    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public static StreamReader getR_fromAsset(string asset)
    {
      return assetHandlerR(asset);
    }

    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public static Stream getR_W_fromResource(string resource)
    {
      return resourceHandlerR_W(resource);
    }
  }
}
