using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

//namespace tabtool

    public static class Extension {
        public static T lexical_cast<T>(this object source) {
            if (source == null || source.ToString() == "")
                return default(T);
            try {
                return (T)Convert.ChangeType(source, typeof(T));
            }
            catch {
                return default(T);
            }
        }
    }

    //tbs中的结构体都从这里继承
    interface ITableObject {
        bool FromString(string s);
    }

    //表字段读取
    class DataReader
    {
     
        public List<string> GetStringList(string s, char delim)
        {
            string[] t = s.Split(delim);
            List<string> ret = new List<string>();
            ret.AddRange(t);
            return ret;
        }

        public T GetValue<T>(string s) {
            return Extension.lexical_cast<T>(s);
        }
        public List<T> GetList<T>(string s) {
            string[] vs = s.Split('|');
            List<T> ret = new List<T>();
            foreach (var ss in vs) {
                T x = Extension.lexical_cast<T>(ss);
                ret.Add(x);
            }
            return ret;
        }
        public T GetObject<T>(string s) where T : ITableObject, new()
        {
            T obj = new T();
            obj.FromString(s);
            return obj;
        }

        public List<T> GetObjectList<T>(string s) where T : ITableObject, new()
        {
            string[] vs = s.Split('|');
            List<T> ret = new List<T>();
            foreach (var ss in vs) {
                ret.Add(GetObject<T>(ss));
            }
            return ret;
        }
    };


    class TableReader
    {
        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();
            //首行是字段名 之后是字段值
            string[] lines = File.ReadAllLines(filepath);
            bool firstline = true;
            foreach (var line in lines)
            {
                string[] words = line.Split('\t');
                if (words == null || words.Length == 0)
                {
                    continue;
                }
                if (firstline)
                {
                    firstline = false;
                    foreach (var word in words)
                    {
                        dt.Columns.Add(word);
                    }
                    continue;
                }
                DataRow row = dt.NewRow();
                int i = 0;
                foreach (var word in words)
                {
                    row[i++] = word;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }

