using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.XSSF.UserModel;
using System.Xml;
using System.Text.RegularExpressions;

namespace tabtool
{
    public class ExcelHelper
    {
        IWorkbook hssfworkbook;
        public DataTable ImportExcelFile(string filePath)
        {
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    hssfworkbook = new XSSFWorkbook(file);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            ISheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            DataTable dt = new DataTable();
            IRow row0 = sheet.GetRow(0);
            for (int j = row0.FirstCellNum; j < (row0.LastCellNum); j++)
            {
                dt.Columns.Add(row0.GetCell(j).ToString());
            }

            while (rows.MoveNext())
            {
                IRow row = (XSSFRow)rows.Current;
                DataRow dr = dt.NewRow();

                for (int i = 0; i < row.LastCellNum; i++)
                {
                    ICell cell = row.GetCell(i);
                    if (cell == null)
                    {
                        dr[i] = null;
                    }
                    else
                    {
                        dr[i] = cell.ToString();
                    }
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public void ExportXmlFile(string filepath, DataTable dt, int firstrow)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings setting = new XmlWriterSettings();
                setting.Indent = true;
                setting.Encoding = new UTF8Encoding(false);
                setting.NewLineChars = Environment.NewLine;

                using (XmlWriter xw = XmlWriter.Create(fs, setting))
                {
                    xw.WriteStartDocument(true);
                    xw.WriteStartElement("datas");
                    for (int i = firstrow; i < dt.Rows.Count; i++)
                    {
                        xw.WriteStartElement("data");
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            xw.WriteAttributeString(dt.Columns[j].ToString(), dt.Rows[i].ItemArray[j].ToString());
                        }
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                }
            }
        }

        public void ExportTxtFile(string filepath, DataTable dt, int firstrow)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                for (int i = firstrow; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j == dt.Columns.Count - 1)
                        {
                            sw.WriteLine(dt.Rows[i].ItemArray[j].ToString());
                        }
                        else
                        {
                            sw.Write(dt.Rows[i].ItemArray[j].ToString() + "\t");
                        }
                    }
                }
                sw.Close();
            }
        }

        public bool IsExportFile(string key, DataTable dt)
        {
            return IsExportField(key, dt, 0);
        }

        public bool IsExportField(string key, DataTable dt, int col) {
            //默认为都导出 NULL 不导出 CLIENT 导出客户端 SERVER 导出服务端
            string s = dt.Rows[1].ItemArray[col].ToString();
            if (s.Contains("null")) {
                return false;
            }
            if (s.Contains(key)) {
                return true;
            }
            if (!s.Contains("client") && !s.Contains("server")) {
                return true;
            }
            return false;
        }

        internal TableMeta ParseTableMeta(string filename, DataTable dt, string cmp)
        {
            TableMeta meta = new TableMeta();
            meta.TableName = filename;
            meta.dt = dt;
            //第一行name 第二行类型和校验条件 第三行注释
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (!IsExportField(cmp, dt, i)) { continue; }
                Match reg = null;
                TableField field = new TableField();
                field.fieldName = dt.Rows[0].ItemArray[i].ToString();
                string[] conds = dt.Rows[1].ItemArray[i].ToString().Split('=');
                int count = 0;
                foreach (string s in conds) {
                    //field.Conditions.AddRange(s.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                    string[] remove_empty_s = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if(remove_empty_s.Length != 1) {
                        throw new Exception("[PARSE_TAbLE_META_ERROR] table:" + filename + " condition:" + s + " remove_empty not 1");
                    }
                    if((reg = Regex.Match(remove_empty_s[0], @"(\w+)\(([A-Za-z0-9_.|]+)\)")).Success) {
                        field.Conditions.Add(reg.Groups[1].Value);
                        field.Conditions.Add(reg.Groups[2].Value);
                    } else {
                        field.Conditions.Add(remove_empty_s[0]);
                    }
                    if(count == 0) {
                        field.realType = remove_empty_s[0];
                    }
                    ++count;
                }


                //enum
                if ((reg = Regex.Match(field.realType, @"^enum\((\w+)\)$")).Success) {
                    field.fieldType = TableFieldType.EnumField;
                    field.realType = reg.Groups[1].Value;
                }
                //mask
                else if ((reg = Regex.Match(field.realType, @"^mask\((\w+)\)$")).Success) {
                    field.fieldType = TableFieldType.MaskField;
                    field.realType = reg.Groups[1].Value;
                }
                //list
                else if ((reg = Regex.Match(field.realType, @"^list\((\w+)\)$")).Success) {
                    field.fieldType = TableFieldType.ListField;
                    field.SetSubType(reg.Groups[1].Value);
                    field.realType = reg.Groups[1].Value;
                }
                //map
                else if ((reg = Regex.Match(field.realType, @"^map\((\w+)\)$")).Success) {
                    field.fieldType = TableFieldType.MapField;
                    field.SetSubType(reg.Groups[1].Value);
                    field.realType = reg.Groups[1].Value;
                }          
                //base_type
                else if (field.realType == "int") { field.fieldType = TableFieldType.IntField; }
                else if (field.realType == "int64") { field.fieldType = TableFieldType.Int64Field; }
                else if (field.realType == "float") { field.fieldType = TableFieldType.FloatField; }
                else if (field.realType == "double") { field.fieldType = TableFieldType.DoubleField; }
                else if (field.realType == "string") { field.fieldType = TableFieldType.StringField; }
                else if (field.realType == "tuple") { field.fieldType = TableFieldType.TupleField; }
                else { field.fieldType = TableFieldType.StructField; }
                meta.Fields.Add(field);
            }
            return meta;
        }

        public void ExportTxtFileEx(string filepath, DataTable dt, string key, int[] ignorerows)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (ignorerows.Contains(i)) continue;
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!IsExportField(key, dt, j))
                        {
                            if (j == dt.Columns.Count - 1)
                            {
                                sw.Write("\n");
                            }
                            continue;
                        }
                        if (j == dt.Columns.Count - 1)
                        {
                            sw.WriteLine("\t" + dt.Rows[i].ItemArray[j].ToString());
                        }
                        else if (j == 0)
                        {
                            sw.Write(dt.Rows[i].ItemArray[j].ToString());
                        }
                        else
                        {
                            sw.Write("\t" + dt.Rows[i].ItemArray[j].ToString());
                        }
                    }
                }
                sw.Close();
            }
        }

    }
}
