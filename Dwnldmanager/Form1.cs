using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Fuck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Клиент для скачивания 
        WebClient client;
        //Экземпляр информационной формы
        public InfoLoader infoLoader;

        //Коллекция для ссылок
        BindingList<HttpObject> httpLinks = new BindingList<HttpObject>();
        public string pathToSave = String.Empty;

        /// <summary>
        /// Показать процесс подгрузки информации о размере файлов
        /// </summary>
        public void ShowPreparingProcess()
        {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    infoLoader = new InfoLoader();
                    infoLoader.Show();
                    this.Enabled = false;
                });
        }

        /// <summary>
        ///Установка источника данных для грида из другого потока. 
        /// </summary>
        /// <param name="listToSet"></param>
        public void SetBindingSource(BindingList<HttpObject> listToSet)
        {
                this.BeginInvoke((MethodInvoker)delegate 
                {
                    //Отвязываю привязку к источнику данных
                    dataGridView1.DataSource = null;
                    //Получаю источник данных из потока, из которого он придет
                    this.httpLinks = listToSet;
                    //Устанавливаю полученный источник данных для таблицы.
                    dataGridView1.DataSource = httpLinks;
                    /*Если к моменту окончания получения данных о файлах форма была не активна, то переводим ее в активный режим*/
                    if (!this.Enabled) this.Enabled = true;
                    this.BringToFront();
                });
        }


        /// <summary>
        /// Загрузка информации о РАЗМЕРЕ ФАЙЛОВ, КОТОРЫЕ УКАЗАНЫ В СПИСКЕ ССЫЛОК В БАЙТАХ!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (HttpObject httpObject in httpLinks) httpObject.pathToSave = ofd.FileName;
                //Удаляем привязку к источнику данных для грида
                dataGridView1.DataSource = null;
                //Создаем экзкемпляр асинхронного загрузчика
                Loader loader = new Loader(ofd.FileName);
                //Запускаем загрузчик для проставления размеров
                loader.LoadInfo_Async();
            }
        }

        /// <summary>
        /// Запуск загрузки файлов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //Если есть файлы на скачивание
            if (httpLinks.Count > 0)
            {
                //Создаем диалог выбора директории
                FolderBrowserDialog sfd = new FolderBrowserDialog();
                //Если пользователь нажал "ОК"
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //Проставляем путь для сохранения файла и активируем таймер, который 10 раз в секунду будет скачивать файлы
                    pathToSave = sfd.SelectedPath;
                    downloadTimer.Start();
                }
                else
                {
                    MessageBox.Show("Выберите путь для скачивания!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Внимание, нет ссылок для скачивания!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// Асинхронная загрузка файла
        /// </summary>
        /// <param name="link"></param>
        private void DownloadHttpFile(HttpObject link)
        {
            //Создаем новый экземпляр клиента, чтобы потоки не ругались друг на друга
            client = new WebClient();
            //Добавляем для данного клиента обработчики собыытий "Изменен статус загрузки файла" и "Загрузка файла окончена"
            client.DownloadProgressChanged += (sender, e) => client_DownloadProgressChanged(sender, e, link);
            client.DownloadFileCompleted += (sender,e) => client_DownloadFileCompleted(sender,e,link);
            //Запускаем загрузку в асинхронном режиме
            client.DownloadFileAsync(link.Url, Path.Combine(link.pathToSave, link.Name));
        }

        /// <summary>
        /// Обработчик события "Изменен статус загрузки файла"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="httpObject"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, HttpObject httpObject)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                //Вычисляем процент загрузки файла
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                //Проставляем процент для прогресс-бара
                httpObject.progressBar.Value = (int)Math.Truncate(percentage);
            });
        }

        /// <summary>
        /// Обработчик события "Загрузка файла окончена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="link"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e,HttpObject link)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                //Удаляем прогресс бар, отвечающий за данный файл
                Watcher(2, link);
            });
        }

        /// <summary>
        /// Тик таймера происходит 10 раз в секунду
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadTimer_Tick(object sender, EventArgs e)
        {
            //Пока есть HTTP - элементы в коллекции, обрабатываем их
            if (httpLinks.Count > 0)
            {
                /*Если в контейнере обрабатывается элементов меньше, чем допустимое количество*/
                if (flowLayoutPanel1.Controls.Count < numericUpDown1.Value)
                {
                    /*Если есть необработанные элементы (inProgress = false)*/
                    if (httpLinks.Where(x => x.inProgress == false).Count()>0)
                    {
                        //Создаем новую ссылку на необработанный HTTP объект
                        HttpObject httpObject = httpLinks.First(x => x.inProgress == false);
                        //Выставляем ему путь, указанный для сохранения файлов
                        httpObject.pathToSave = pathToSave;
                        //Проставляем признак "В обработке"
                        httpObject.inProgress = true;
                        //Приступаем к скачиваню файла 
                        DownloadHttpFile(httpObject);
                        //Добавляем прогресс бар, позволяющий отслеживать как много загрузилось
                        Watcher(1, httpObject);
                    }
                }
            }
            else
            {
                downloadTimer.Stop();
                MessageBox.Show("Загрузка завершена.", "Загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Процедура, для создания прогресс-баров
        /// </summary>
        /// <param name="mode">Режим. 1-создание и добавление, 2-удаление</param>
        /// <param name="httpObject">Объект HTTP</param>
        public void Watcher(int mode, HttpObject httpObject)
        {
           this.BeginInvoke((MethodInvoker)delegate
           {
               /*Добавление прогресс бара для данного HTTP-объекта*/
               if (mode == 1)
               {
                   //Создаю новый груп-бокс
                   GroupBox gb = new GroupBox();
                   //Выставляю ему размер чуть меньше, чем контейнер
                   gb.Size = new Size(flowLayoutPanel1.Width - 5, 25);
                   //Выделяем память и инициализируем место для прогресс-бара
                   httpObject.progressBar = new ProgressBar();
                   //Задаем имя прогресс бара (имя - уникальный идентификатор GUID, который не повторяется)
                   httpObject.progressBar.Name = httpObject.guid.ToString();
                   //Задаем минимум и максимум для прогресс бара
                   /*Так как выполнение отображается в процентах, диапазон от 0 до 100*/
                   httpObject.progressBar.Minimum = 0; httpObject.progressBar.Maximum = 100;
                   //Добавляем данный прогресс бар внутрь групбокса
                   gb.Controls.Add(httpObject.progressBar);
                   //Выставляем режим "Заполнять все пространство" для прогресс-бара
                   httpObject.progressBar.Dock = DockStyle.Fill;
                   //Выставляем текст груп-бокса имя скачиваемого файла
                   gb.Text = httpObject.Name;
                   //Добавляем данную конструкцию в контейнер
                   flowLayoutPanel1.Controls.Add(gb);
               }
               /*Удаление прогресс бара для данного HTTP-объекта*/
               if (mode == 2)
               {
                   //Создаем ссылку на прогресс-бар с указанным именем
                   ProgressBar pb = (ProgressBar)flowLayoutPanel1.Controls.Find(httpObject.guid.ToString(), true)[0];
                   //Удаляем родительский груп-бокс из контейнера (удаляется вместе с прогресс-баром)
                   pb.Parent.Dispose();
                   //Удаляем данный HTTP - элемент из данной коллекции
                   httpLinks.Remove(httpObject);
               }
           });
                
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
