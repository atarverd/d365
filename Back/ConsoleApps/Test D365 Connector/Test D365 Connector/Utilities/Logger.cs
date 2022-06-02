using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public  class Logger
    {
        private StreamWriter file;
        public Logger()
        {
            file = new StreamWriter("Logs.txt");
        }
        public async void Log(string text)
        {
            string dateTiemStr = DateTime.Now.ToString("G");
            string logText = "[" + dateTiemStr + "] " + text;
            Console.WriteLine(logText);
            await file.WriteLineAsync(logText);            
        }

        public void Terminate()
        {
            if (file != null)
            {
                file.Close();
                file.Dispose();
            }
        }     
    }

