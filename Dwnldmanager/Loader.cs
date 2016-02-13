using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace Fuck
{
    class Loader
    {
            private delegate void _LoadInfo();
            private readonly object _sync_ftp = new object();
            bool loadInProgress = false;

            public Loader(string savePath)
            {
                this.savePath = savePath;
            }

            private string savePath = ""; 
            private BindingList<HttpObject> httpLinks = new BindingList<HttpObject>();
            private List<string> tempUrls = new List<string>();

            private void LoadInfo()
            {
                try
                {
                    Program.form1.ShowPreparingProcess();

                    //Отчистим список ссылок 
                    httpLinks.Clear();
                    //Создаем переменную для считывания ссылок
                    string s = File.ReadAllText(savePath);
                    string tempString = "";
                    for (int i = 0; i <= s.Length - 1; i++)
                    {
                        if (i <= s.Length - 1)
                        {
                            while (s[i] != (char)10)
                            {
                                tempString += s[i];
                                i++;
                            }
                            tempUrls.Add(tempString);
                            tempString = "";
                        }
                    }

                    int value = 0;


                    foreach (string url in tempUrls)
                    {
                        httpLinks.Add(new HttpObject(new Uri(url.Substring(0, url.Length - 1)), ""));
                        value++;
                        Program.form1.infoLoader.SetBarProperties(tempUrls.Count, value, false);
                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }




            public void LoadInfo_Async()
            {
                _LoadInfo loadInfo = new _LoadInfo(LoadInfo);
                AsyncCallback completedCallback = new AsyncCallback(LoadInfo_AsyncCall);
                lock (_sync_ftp)
                {
                    if (loadInProgress)
                    {
                        MessageBox.Show("Загрузка данных уже запущена!");
                        return;
                    }
                    AsyncOperation async = AsyncOperationManager.CreateOperation(null);

                    loadInfo.BeginInvoke(completedCallback, async);
                    loadInProgress = true;
                }
            }
            private void LoadInfo_AsyncCall(IAsyncResult ar)
            {
                _LoadInfo worker =
                  (_LoadInfo)((AsyncResult)ar).AsyncDelegate;
                AsyncOperation async = (AsyncOperation)ar.AsyncState;

                worker.EndInvoke(ar);

                lock (_sync_ftp)
                {
                    loadInProgress = false;
                }

                AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);

                async.PostOperationCompleted(delegate(object e)
                { DownloadComplete((AsyncCompletedEventArgs)e); },
                  completedArgs);
            }
            protected virtual void DownloadComplete(AsyncCompletedEventArgs e)
            {
                Program.form1.infoLoader.SetBarProperties(0, 0, true);
                Program.form1.SetBindingSource(httpLinks);
            }
        
    }
}
