using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace LeaveSystemMVC.CustomLibraries
{
    public class CsvFile
    {
        public string path { get; }
        public CsvFile(string _path)
        {
            path = _path;
        }
        

        public List<string[]> readFile()
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            List<string[]> valueList = new List<string[]>();
            string[] values;

            if(!reader.EndOfStream)
                reader.ReadLine().Skip(1);

            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                values = line.Split(',');
                valueList.Add(values);
            }
            reader.Close();
            return valueList;
        }
    }
}