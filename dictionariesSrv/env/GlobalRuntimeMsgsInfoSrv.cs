using NLog;
using System;

namespace eng_spa.env.dictionariesSrv
{
  public static class GlobalRuntimeMsgsInfoSrv
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    public static Action<string> infoChannel { get; set; }= (msg) => { log.Debug("Info channel not config, using log debug channel{0}", msg); };
    public static Action<string> errorChannel { get; set; } = (msg) => { log.Debug("Error channel not config, using log debug channel{0}", msg); };

    private static object channelLock = new object();

    public static void infoMsg(string inMsg)
    {
      lock(channelLock)
      {
        infoChannel(inMsg);
      }
    }

    public static void errorMsg(string inMsg)
    {
      lock (channelLock)
      {
        errorChannel(inMsg);
      }
    }
  }
}
