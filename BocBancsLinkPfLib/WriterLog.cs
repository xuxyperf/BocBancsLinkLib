using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BocBancsLinkPfLib
{
    public class WriterLog
    {
        public void Logger(string msgLog)
        {
            if (!System.IO.Directory.Exists(@"c:\\Logs"))
            {
                System.IO.Directory.CreateDirectory(@"c:\\Logs");
            }
            string path = @"C:\\Logs\\" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-" + System.Guid.NewGuid().ToString() + "-log.txt";
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            if (!fileInfo.Exists)
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(path);
                writer.WriteLine(msgLog);
                writer.Flush();
                writer.Close();
            }
        }        
    }
}
