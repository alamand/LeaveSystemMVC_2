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
        public CsvParser(string _path)
        {
            path = _path;
        }
        

        public string[] readFile()
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            string[] values = new string[];

            if(!reader.EndOfStream)
                reader.ReadLine().Skip(1);

            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                values = line.Split(',');
            }
            return values;
        }
    }
}