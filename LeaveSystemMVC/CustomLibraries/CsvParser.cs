using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace LeaveSystemMVC.CustomLibraries
{
    public class CsvParser
    {
        string path;
        CsvParser()
        {
            path = "";
        }
        

        public void readFile()
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            int iter = 0;
            while(!reader.EndOfStream)
            {
                if(iter == 0)
                {
                    iter++;
                    
                    continue;
                }
                string line = reader.ReadLine();
                var values = line.Split(',');

            }
        }

    }
}