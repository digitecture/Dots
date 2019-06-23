using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// take the csv inputs convert to string - goes to main: dotsdev.cs
// in dotsdev.cs send string to here gen adj input
// in dotsdev.cs send string to here gen geom input for spaces

namespace DotsProj
{
    class CsvParser
    {
        private string FilePath;
        private List<string> FileContents;

        private List<string> adjObjLi;
        private List<string> geomObjLiStr;
        private List<GeomObj> geomObjLi;

        private List<GeomObj> norGeomLi;
        public List<string> norGeomObjLiStr { get; set; }

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

        public List<string> GetGeomObjLi(List<string> geomstrList, double site_ar)
        {
            geomObjLiStr = new List<string>();
            MakeGeomObjList obj = new MakeGeomObjList(geomstrList, site_ar);
            geomObjLiStr = obj.GetGeomObjListStr(); // file: MakeGeomObjList-string


            geomObjLi = new List<GeomObj>();
            geomObjLi = obj.GetGeomObj(); // file : MakeGeomObjList-object

            norGeomLi = new List<GeomObj>();
            norGeomLi = obj.NormalizeGeomObj(geomObjLi, site_ar); // file : MakeGeomObjList-object

            norGeomObjLiStr = new List<string>();
            for (int i = 0; i < norGeomLi.Count; i++) {
                string s = norGeomLi[i].ToString();
                norGeomObjLiStr.Add(s);
            }
            return geomObjLiStr;
        }
    }
}
