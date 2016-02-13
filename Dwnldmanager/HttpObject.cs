using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Fuck
{
    //Объект HTTP - содержит всю нужную информацию для обработки файлов
    public class HttpObject : INotifyPropertyChanged
    /*Тут немного сложноватый момент. Данный класс наследуется от интерфейса INotifyPropertyChanged
     это нужно для того, чтобы если менялись свойства данного файла, менялись значения и в элементе, 
     источником данных которого является данный экземпляр (ничего не понятно, знаю)*/
    {
        /// <summary>
        /// Конструктор для данного класса
        /// </summary>
        /// <param name="url">Ссылка на скачивание файла</param>
        /// <param name="pathToSave">Путь для сохранения файла</param>
        public HttpObject(Uri url,string pathToSave)
            {
                this.url = url;
                this.name = Path.GetFileName(url.LocalPath);
                this.pathToSave = pathToSave;
                this.guid = Guid.NewGuid();
                this.fileSize = GetHttpFileLength(url);
            }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged("Name"); }
        }
        private Uri url;

        public Uri Url
        {
            get { return url; }
            set { url = value; NotifyPropertyChanged("Url"); }
        }
            public string pathToSave;
            public Guid guid;
            public bool inProgress = false;
            public ProgressBar progressBar;
            private long fileSize;

            public long FileSize
            {
                get { return fileSize; }
                set { fileSize = value; NotifyPropertyChanged("FileSize"); }
            }

            private long GetHttpFileLength(Uri uri)
            {
                WebRequest req = System.Net.HttpWebRequest.Create(uri);
                req.Method = "HEAD";
                using (WebResponse resp = req.GetResponse())
                {
                    long ContentLength;
                    if (long.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                    {
                        return ContentLength;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
    }
}
