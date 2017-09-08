using System;

namespace eng_spa.dictionariesSrv.views
{
  interface IMainView
  {
    void init();
    void run();
    void release();
    void setWindowName(string inName);
    void displayText(string inText);
    void displayScrollUpFirstLine();
    void clearDisplay();
    void displayStatus(string inText);
    void clearStatusArea();
    void setSearchLabel(string inText);
    void setDictHandler(Action<string> inHandler);
    void selectInputUserText();
    void setTipHelp(string inTipText);
    void addEntryToSuggestList(string inText);
    void hideKeyboard();
    void clearUserInput();
    void setFocusOnUserInput();
    void setFocusOnResult();
  }
}
