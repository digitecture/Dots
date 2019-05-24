using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// take the csv inputs convert to string - goes to main: dotsdev.cs
// in dotsdev.cs send string to here gen adj input
// in dotsdev.cs send string to here gen geom input for spaces

namespace dots_dev
{
    class CsvParser
    {
        private string FilePath;
        private List<string> FileContents;

        private List<string> adjObjLi;
        private List<string> geomObjLiStr;
        private List<GeomEntry> geomObjLi;

        public CsvParser(){}

        public CsvParser(string path)
        {
            FilePath = path;
            FileContents = new List<string>();
        }

        public string getFilePath()
        {
            return FilePath;
        }

        public List<string> readFile()
        {
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(FilePath))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    int k = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        FileContents.Add(line);
                        k++;
                    }
                }
            }
            return FileContents;
        }

        public List<string> GetAdjObjLi (List<string> adjstrList)
        {
            adjObjLi = new List<string>();
            MakeAdjacencyObjList obj = new MakeAdjacencyObjList(adjstrList);
            adjObjLi=obj.GetAdjacencyObjList();
            return adjObjLi;
        }

        public List<string> GetGeomObjLi (List<string> geomstrList)
        {
            geomObjLiStr = new List<string>();
            geomObjLi = new List<GeomEntry>();
            MakeGeomObjList obj = new MakeGeomObjList(geomstrList);
            geomObjLiStr = obj.GetGeomObjListStr(); // file: MakeGeomObjList-string
            geomObjLi = obj.GetGeomObj(); // file : MakeGeomObjList-object
            return geomObjLiStr;
        }

        public List<GeomEntry> GetGeomObjLi()
        {
            return geomObjLi;
        }
    }
}
