using Android.App;
using Android.Widget;
using Android.OS;
using eng_spa.dictionariesSrv;
using NLog;
using eng_spa.dictionariesSrv.types;
using eng_spa.dictionariesSrv.env;
using eng_spa.env.dictionariesSrv;
using eng_spa.dictionariesSrv.views;
using System;
using Android.Views;
using Android.Content.Res;
using System.IO;
using Android.Views.InputMethods;
using Android.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace eng_to_spa_android
{
  [Activity(Label = "Eng->Spa v1.0 beta", MainLauncher = true, Icon = "@mipmap/icon")]
  public class MainActivity : Activity, IMainView
  {
    private static AutoCompleteTextView inputUser;
    private static Button searchBtn;
    private static TextView resultView;
    private static TextView statusView;
    private static Action<string> dictHandle;
    private static Logger log = LogManager.GetCurrentClassLogger();
    private static IEngToSpaDictSrv dictSrv;
    private static IMainView view;
    private static AssetManager assetsMng;
    private static List<string> userWordsCache;
    private static MainActivity mainActivity;
    private static bool isSearching = false;
    private static object searchLock = new object();

    protected override void OnCreate(Bundle savedInstanceState)
    {
      try
      {
        userWordsCache = new List<string>();
        mainActivity = this;

        // ANDROID CONFIG
        assetsMng = this.Assets;
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.Main);

        inputUser = FindViewById<AutoCompleteTextView>(Resource.Id.inputUser);                
        inputUser.KeyPress += (object sender, View.KeyEventArgs e) =>
        {
          e.Handled = false;
          if (e.Event.Action == KeyEventActions.Up && e.KeyCode == Keycode.Enter)
          {
            dictHandle(inputUser.Text);
            e.Handled = true;
          }
        };        
        // config suggestion words
        searchBtn = FindViewById<Button>(Resource.Id.searchBtn);
        resultView = FindViewById<TextView>(Resource.Id.resultView);
        statusView = FindViewById<TextView>(Resource.Id.statusView);
        view = this;

        // set runtime info/error output channel        
        // setup eviroment
        Eviroment.assetHandlerR = (string assetResource) => { return new StreamReader(assetsMng.Open(assetResource)); };
        Eviroment.resourceHandlerR_W=(string resource) => 
        {
          //Context.OpenFileOutput(String, FileCreationMode)
          var full_resources_path = CacheDir.AbsolutePath;
          if (System.IO.File.Exists(full_resources_path))
          {
            System.IO.File.Create(full_resources_path);
          }
          var resource_full_path= Path.Combine(full_resources_path, resource);
          return File.Open(resource_full_path, FileMode.OpenOrCreate, FileAccess.ReadWrite); 
        };

        GlobalRuntimeMsgsInfoSrv.infoChannel = statusInfoHdl;
        GlobalRuntimeMsgsInfoSrv.errorChannel = displayErrorHdl;

        LocationLangSrv.changeToEng();
        dictSrv = new EngToSpaDictSrvImpl();

        view.init();
        view.setWindowName(LocationLangSrv.getTagValue(Constants.APP_NAME_TAG));
        view.setTipHelp(LocationLangSrv.getTagValue(Constants.APP_HELP_TIP));
        view.setDictHandler(searchDictHandler);
        view.setSearchLabel(LocationLangSrv.getTagValue(Constants.SEARCH_LABEL));
        view.clearStatusArea();
        view.displayStatus(LocationLangSrv.getTagValue(Constants.STATUS_OK_MSG) + Constants.BRLINE);
        view.displayStatus(LocationLangSrv.getTagValue(Constants.DICTIONARY_INFO) + dictSrv.getDictionaryName() + Constants.BRLINE);
        view.setFocusOnUserInput();
        // events config
        searchBtn.Click += delegate { dictHandle(inputUser.Text); };
      }
      catch (Exception ex)
      {
        AlertDialog.Builder alert = new AlertDialog.Builder(this);
        alert.SetTitle("Info");
        alert.SetMessage(LocationLangSrv.getTagValue(Constants.INIT_APPLICATION_ERROR) + Constants.BRLINE);
        Dialog dialog = alert.Create();
        dialog.Show();
        log.Debug(ex.StackTrace);
      }
    }

    static void searchDictHandler(string inText)
    {
      var viewLocal = view;

      lock(searchLock)
      {
        if (MainActivity.isSearching)
        {
          return;
        }
      }

      try
      {
        lock (searchLock)
        {
          MainActivity.isSearching = true;
        }         

        view.hideKeyboard();
        view.clearStatusArea();
        view.clearDisplay();
        view.displayStatus(LocationLangSrv.getTagValue(Constants.SEARCHING_DICT_TEXT) + "[" + inText + "]" + Constants.BRLINE);
        QueryDictResult result = null;

        var searchThread = new Thread(new ThreadStart(() =>
        {
          // start time out thread
          var timerThread = new Thread(new ThreadStart(() =>
          {
            Thread.Sleep(15000);
            if (result == null)
            {
              viewLocal.displayText(LocationLangSrv.getTagValue(Constants.TIME_OUT_ERROR) + Constants.BRLINE);
              lock (searchLock)
              {
                MainActivity.isSearching = false;              
              }
            }
          }));
          timerThread.Start();

          viewLocal.displayText(LocationLangSrv.getTagValue(Constants.SEARCHING_INFO) + "[" + inText + "]" + Constants.BRLINE);

          result = dictSrv.getTranslation(inText);
          mainActivity.RunOnUiThread(() =>
          {
            viewLocal.clearDisplay();
          });

          mainActivity.RunOnUiThread(() => displayDictResult(result));

          lock (searchLock)
          {
            MainActivity.isSearching = false; ;
          }
        }));
        searchThread.Start();
      }
      catch (Exception ex)
      {
        viewLocal.displayText(LocationLangSrv.getTagValue(Constants.RUNTIME_ERROR_TAG) + Constants.BRLINE);
        log.Debug(ex.StackTrace);
        lock (searchLock)
        {
          MainActivity.isSearching = false;
        }
      }
    }

    static void displayDictResult(QueryDictResult inResult)
    {
      display(dictSrv, view, inResult);
      view.clearUserInput();
      inputUser.ClearFocus();
      view.setFocusOnResult();
    }

    static void statusInfoHandler(string inText)
    {
      view.displayStatus(inText);
    }

    static void display(IEngToSpaDictSrv dictSrv, IMainView view, QueryDictResult result)
    {

      try
      {
        // status banner
        view.displayText(Constants.BANNER_SEPARATOR + Constants.BRLINE);

        if (!result.IsEmpty)
        {
          view.displayText(LocationLangSrv.getTagValue(Constants.TAG_DICT_TEXT_FOUND) + Constants.BRLINE);
          view.addEntryToSuggestList(result.sourceText);
        }
        else
        {
          view.displayText(string.Format(LocationLangSrv.getTagValue(Constants.TAG_DICT_TEXT_NOT_FOUND), result.sourceText) + Constants.BRLINE);
        }

        //result
        view.displayText(Constants.BANNER_SEPARATOR + Constants.BRLINE);
        view.displayText(QueryDictResultFormater.toSimpleText(result));

        // banner dict. resume
        view.displayText(Constants.BANNER_SEPARATOR + Constants.BRLINE);
        var totalEntriesText = string.Format(LocationLangSrv.getTagValue(Constants.TAG_DICT_RESUME_DATA), dictSrv.totalEntries());
        view.displayText(totalEntriesText + Constants.BRLINE);
        view.displayText(Constants.BANNER_SEPARATOR + Constants.BRLINE);
        view.displayScrollUpFirstLine();
      }
      catch (Exception ex)
      {
        log.Debug(ex.StackTrace);
      }
    }

    static void statusInfoHdl(string inText)
    {
      view.displayStatus(inText);
    }

    static void displayErrorHdl(string inText)
    {
      view.displayText(inText);
    }


    public void addEntryToSuggestList(string inText)
    {
      RunOnUiThread(() =>
      {
        if (!userWordsCache.Contains(inText))
        {
          if (userWordsCache.Count > 100)
          {
            userWordsCache.RemoveAt(0);
          }

          userWordsCache.Add(inText);
          ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, userWordsCache);
          inputUser.Adapter = adapter;
        }
      });
    }

    public void clearDisplay()
    {
      RunOnUiThread(() =>
      {
        resultView = FindViewById<TextView>(Resource.Id.resultView);
        resultView.Text = "";
      });
    }

    public void clearStatusArea()
    {
      RunOnUiThread(() =>
      {
        resultView = FindViewById<TextView>(Resource.Id.statusView);
        resultView.Text = "";
      });
    }

    public void displayScrollUpFirstLine()
    {
      //throw new NotImplementedException();
    }

    public void displayStatus(string inText)
    {
      RunOnUiThread(()=>
      {
        statusView.Text += inText;
        statusView.SetHorizontallyScrolling(true);
      });
    }

    public void displayText(string inText)
    {
      RunOnUiThread(() =>
      {
        resultView.Text += inText;
        resultView.SetHorizontallyScrolling(true);
      });
    }

    public void init()
    {
      //throw new NotImplementedException();
    }

    public void release()
    {
      //throw new NotImplementedException();
    }

    public void run()
    {
      //throw new NotImplementedException();
    }

    public void selectInputUserText()
    {
      RunOnUiThread(() =>
      {
        inputUser.SelectAll();
      });
    }

    public void setDictHandler(Action<string> inHandler)
    {
      dictHandle= inHandler;
    }

    public void setSearchLabel(string inText)
    {
      RunOnUiThread(() =>
      {
        searchBtn.Text = inText;
      });
    }

    public void setTipHelp(string inTipText)
    {
      //throw new NotImplementedException();
    }

    public void setWindowName(string inName)
    {
      //throw new NotImplementedException();
    }

    public void hideKeyboard()
    {
      RunOnUiThread(() =>
      {
        InputMethodManager inputManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
        var currentFocus = this.CurrentFocus;
        if (currentFocus != null)
        {
          inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
        }
      });
    }

    public void clearUserInput()
    {
      RunOnUiThread(() =>
      {
        inputUser.Text = "";
      });
    }

    public void setFocusOnResult()
    {
      RunOnUiThread(() =>
      {
        resultView.RequestFocus();
      });

    }

    public void setFocusOnUserInput()
    {
      RunOnUiThread(() =>
      {
        inputUser.RequestFocus();
      });
    }
  }
}

