﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace tabtool {
    class CodeGen {
        public static string space_name = "game_data";
        public static void MakeCppFile(List<TableMeta> metalist, string codepath) {
            foreach (var meta in metalist) {
                string hfile = codepath + meta.GetClassName() + ".hpp";
                using (FileStream fs = new FileStream(hfile, FileMode.Create, FileAccess.Write)) {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                    sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                    sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                    sw.WriteLine("#pragma once");
                    sw.WriteLine("# include \"readtablefield.h\"");
                    sw.WriteLine("# include \"tablestruct.h\"");
                    sw.WriteLine();
                    sw.WriteLine("namespace {0} {{", space_name);
                    sw.WriteLine();
                    foreach (var field in meta.Fields) {
                        string relate_info = field.Get("relate");
                        if(relate_info != null && field.Get("regroup") == null) {
                            string[] s = relate_info.Split('.');
                            if (s.Count() != 2) {
                                throw new Exception(meta.TableName + " relate error!!! field name " + field.fieldName);
                            }
                            meta.relate = true;
                            sw.WriteLine("struct td_{0}_item;", s[0].ToLower());
                        }
                    }
                    sw.WriteLine("struct {0} {{", meta.GetItemName());
                    foreach (var field in meta.Fields) {
                        sw.WriteLine("  {0} {1};", field.GetCppTypeName(), field.fieldName);
                    }
                    foreach(var field in meta.Fields) {
                        string relate_info = field.Get("relate");
                        if (relate_info != null && field.Get("regroup") == null) {
                            string[] s = relate_info.Split('.');
                            sw.WriteLine("  std::vector<const td_{0}_item*> __relate__td_{0}_items;", s[0].ToLower());
                        }        
                    }
                    

                    sw.WriteLine("};");
                    sw.WriteLine();
                    sw.WriteLine("class {0} : public IConfigTable<{1}>{{", meta.GetClassName(), meta.GetItemName());
                    sw.WriteLine("public:");
                    sw.WriteLine("virtual bool Load() {");
                    sw.WriteLine("\tReadTableFile reader;");
                    sw.WriteLine("\treader.Initialize();");
                    sw.WriteLine();
                    sw.WriteLine("\tif (!reader.Init(GetTableFile().c_str()))");
                    sw.WriteLine("\t\treturn false;");
                    sw.WriteLine();
                    sw.WriteLine("\ttry {");
                    sw.WriteLine("\t\tDataReader dr;");
                    sw.WriteLine("\t\tint iRows = reader.GetRowCount();");
                    sw.WriteLine("\t\tint iCols = reader.GetColCount();");
                    sw.WriteLine("\t\tfor (int i = 1; i < iRows; ++i) {");
                    sw.WriteLine("\t\t\t{0} item;", meta.GetItemName());
                    foreach (var field in meta.Fields) {
                        switch (field.fieldType) {
                            case TableFieldType.IntField:
                            case TableFieldType.EnumField:
                            case TableFieldType.MaskField:
                                sw.WriteLine("\t\t\titem.{0} = dr.GetValue<int32_t>(reader.GetValue(i, \"{0}\"));", field.fieldName);
                                break;
                            case TableFieldType.Int64Field:
                                sw.WriteLine("\t\t\titem.{0} = dr.GetValue<int64_t>(reader.GetValue(i, \"{0}\"));", field.fieldName);
                                break;
                            case TableFieldType.FloatField:
                                sw.WriteLine("\t\t\titem.{0} = dr.GetValue<float>(reader.GetValue(i, \"{0}\"));", field.fieldName);
                                break;
                            case TableFieldType.DoubleField:
                                sw.WriteLine("\t\t\titem.{0} = dr.GetValue<double>(reader.GetValue(i, \"{0}\"));", field.fieldName);
                                break;
                            case TableFieldType.StringField:
                                sw.WriteLine("\t\t\titem.{0} = (reader.GetValue(i, \"{0}\"));", field.fieldName);
                                break;
                            case TableFieldType.TupleField:
                                //TODO:
                                break;
                            case TableFieldType.StructField:
                                sw.WriteLine("\t\t\titem.{0} = dr.GetObject<{1}>(reader.GetValue(i, \"{0}\"));", field.fieldName, field.GetCppTypeName());
                                break;
                           case TableFieldType.ListField:
                                    if(field.subType == TableFieldType.StructField) {
                                        sw.WriteLine("\t\t\titem.{0} = dr.GetObjectList<{1}>(reader.GetValue(i, \"{0}\"));", field.fieldName, field.realType);
                                    }
                                    else {
                                        sw.WriteLine("\t\t\titem.{0} = dr.GetList<{1}>(reader.GetValue(i, \"{0}\"));", field.fieldName, field.FieldTypeToString(field.subType));
                                    }
                                break;
                           case TableFieldType.MapField:
                                break;
                        }
                    }
                    sw.WriteLine("\t\t\tm_Items[item.{0}] = item;", meta.Fields[0].fieldName);//必须有一个id
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t} catch(std::exception& e) {");
                    sw.WriteLine("\t\tstd::cerr << e.what() << std::endl;");
                    sw.WriteLine("\t\treturn false;");
                    sw.WriteLine("\t}");
                    sw.WriteLine("\treturn true;");
                    sw.WriteLine("}");
                    sw.WriteLine();
                    sw.WriteLine("string GetTableFile() {");
                    sw.WriteLine("\tstring f = WORK_DIR;");
                    sw.WriteLine("\tf = f + \"{0}.txt\";", meta.TableName);
                    sw.WriteLine("\treturn f;");
                    sw.WriteLine("}");
                    sw.WriteLine();
                    sw.WriteLine("};");
                    sw.WriteLine();
                    sw.WriteLine("}");
                    }
                }
            }

            string cppfile = codepath +  "table_load.cpp";
            using (FileStream fs = new FileStream(cppfile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                    sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                    sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                    sw.WriteLine("#include <iostream>");
                    sw.WriteLine();
                    foreach (var meta in metalist) {
                        sw.WriteLine("#include \"{0}.hpp\"", meta.GetClassName());
                        sw.WriteLine("{0}::{1} the_{1};", space_name, meta.GetClassName());
                        sw.WriteLine("template<> ::{0}::{1} const& GET_TABLE<::{0}::{1}>() {{", space_name, meta.GetClassName());
                        sw.WriteLine("\treturn the_{0};", meta.GetClassName());
                        sw.WriteLine("}");
                    }
                    sw.WriteLine();
                    sw.WriteLine("namespace {0} {{", space_name);
                    sw.WriteLine("bool load_all_data() {");
                    foreach (var meta in metalist) {
                        //序列化版本 使用exception机制
                        //sw.WriteLine("try {{ if(the_{0}.Load() == false) throw std::logic_error(\"{0} load failed !!!\"); }} catch (std::exception& e) {{ std::cerr << e.what(); throw e;}}", meta.GetClassName());
                        //非序列化版本
                        sw.WriteLine("\tif(!the_{0}.Load()) {{ std::cerr << \"load {0} failed !!!\" << std::endl; return false; }}", meta.GetClassName());
                    }
                    //relate code
                    sw.WriteLine("\t//relate code");
                    foreach (var meta in metalist) {
                        if (meta.relate) {
                            sw.WriteLine("\t{");
                            foreach (var field in meta.Fields) {
                                string relate_info = field.Get("relate");
                                if (relate_info != null) {
                                    string[] s = relate_info.Split('.');
                                    sw.WriteLine("\t\tfor(auto p1 = the_{0}.RawMap().begin(); p1 != the_{0}.RawMap().end(); ++p1) {{", meta.GetClassName());
                                    sw.WriteLine("\t\t\tauto& v1 = ({0}&)p1->second;", meta.GetItemName());
                                    sw.WriteLine("\t\t\tauto const& k1 = v1.{0};", field.fieldName);                                  
                                    sw.WriteLine("\t\t\tfor(auto p2 = the_td_{0}.RawMap().begin(); p2 != the_td_{0}.RawMap().end(); ++p2) {{", s[0].ToLower());
                                    sw.WriteLine("\t\t\t\tauto const& v2 = p2->second;");
                                    sw.WriteLine("\t\t\t\tauto const& k2 = v2.{0};", s[1].ToLower());
                                    sw.WriteLine("\t\t\t\tif(k1 == k2) {");
                                    sw.WriteLine("\t\t\t\t\tv1.__relate__td_{0}_items.push_back(&v2);", s[0].ToLower());
                                    sw.WriteLine("\t\t\t\t}");
                                    sw.WriteLine("\t\t\t}");
                                    sw.WriteLine("\t\t}");                                   
                                }
                            }
                            sw.WriteLine("\t}");
                        }
                    }
                    sw.WriteLine("\treturn true;");
                    sw.WriteLine("}");
                    sw.WriteLine("}");
                }
            }
        }
        public static void MakeCppFileTbs(List<TableMeta> metalist, string codepath)
        {
            string hfile = codepath + "tablestruct.h";
            using (FileStream fs = new FileStream(hfile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                    sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                    sw.WriteLine("#pragma once");
                    sw.WriteLine("#include \"readtablefield.h\"");
                    sw.WriteLine("#include \"myconfig.h\"");
                    sw.WriteLine();
                    sw.WriteLine("namespace {0} {{", space_name);

                    foreach (var meta in metalist)
                    {
                        sw.WriteLine();
                        sw.WriteLine("struct {0} : public ITableObject<{0}>", meta.TableName);
                        sw.WriteLine("{");
                        foreach (var field in meta.Fields) {
                            sw.WriteLine("	{0} {1};", field.GetCppTypeName(), field.fieldName);
                        }
                        sw.WriteLine();
                        sw.WriteLine("	virtual bool FromString(string s)");
                        sw.WriteLine("	{");
                        sw.WriteLine("		DataReader dr;");
                        sw.WriteLine("		vector<string> vs = dr.GetStringList(s,',');");
                        sw.WriteLine("		if (vs.size() != {0})", meta.Fields.Count());
                        sw.WriteLine("		{");
                        sw.WriteLine("			ErrorLog(\"{0}字段配置错误\");", meta.TableName);
                        sw.WriteLine("			return false;");
                        sw.WriteLine("		}");
                        for (int i = 0; i < meta.Fields.Count(); i++)
                        {
                            var field = meta.Fields[i];
                            switch (field.fieldType)
                            {
                                case TableFieldType.IntField:
                                    sw.WriteLine("		{0} = dr.GetValue<int32_t>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.Int64Field:
                                    sw.WriteLine("		{0} = dr.GetValue<int64_t>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.FloatField:
                                    sw.WriteLine("		{0} = dr.GetValue<float>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.DoubleField:
                                    sw.WriteLine("		{0} = dr.GetValue<double>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.StringField:
                                    sw.WriteLine("		{0} = (vs[{1}]);", field.fieldName, i);
                                    break;
                                default:
                                    Console.WriteLine("{0}.{1}字段类型错误，只能是int float string！", meta.TableName, field.fieldName);
                                    break;
                            }
                        }
                        sw.WriteLine("		return true;");
                        sw.WriteLine("	}");
                        sw.WriteLine("};");
                        sw.WriteLine();
                    }
                    sw.WriteLine("}");
                    ////////////////////////
                 
                }
            }
        }
        public static void MakeCsharpFileTbs(List<TableMeta> metalist, string codepath)
        {
            string hfile = codepath + "TableStruct.cs";
            using (FileStream fs = new FileStream(hfile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                    sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                    sw.WriteLine();
                    sw.WriteLine("namespace {0} {{", space_name);
                    foreach (var meta in metalist)
                    {
                        sw.WriteLine("  public class {0} : ITableObject", meta.TableName);
                        sw.WriteLine("  {");
                        foreach (var field in meta.Fields)
                        {
                            sw.WriteLine("      {0} {1};", field.GetCsharpTypeName(), field.fieldName);
                        }
                        sw.WriteLine("      public bool FromString(string s)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          DataReader dr = new DataReader();");
                        sw.WriteLine();
                        sw.WriteLine("          var vs = s.Split(',');");
                        sw.WriteLine("          if (vs.Length != {0}) {{", meta.Fields.Count());
                        //sw.WriteLine("          Console.WriteLine(\"{0}字段配置错误\")", meta.TableName);
                        sw.WriteLine("              return false;");
                        sw.WriteLine("          }");
                        sw.WriteLine();
                        for (int i = 0; i < meta.Fields.Count(); i++) {
                            var field = meta.Fields[i];
                            switch (field.fieldType) {
                                case TableFieldType.IntField:
                                    sw.WriteLine("		{0} = dr.GetValue<int>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.FloatField:
                                    sw.WriteLine("		{0} = dr.GetValue<float>(vs[{1}]);", field.fieldName, i);
                                    break;
                                case TableFieldType.StringField:
                                    sw.WriteLine("		{0} = vs[{1}];", field.fieldName, i);
                                    break;
                                default:
                                    Console.WriteLine("{0}.{1}字段类型错误，只能是int float string！", meta.TableName, field.fieldName);
                                    break;
                            }
                        }
                        sw.WriteLine("          return true;");
                        sw.WriteLine("      }");
                        sw.WriteLine("  };");
                        sw.WriteLine();
                    }
                    sw.WriteLine("}");

                    ////////////////////////
                }
            }

        }
        public static void MakeCsharpRegroupFile(TableMeta meta, string codepath) {
            foreach (var field in meta.Fields) {
                string relate_info = field.Get("regroup");
                if(relate_info != null) {
                   

                }
            }
        }
        public static void MakeCsharpFile(List<TableMeta> metalist, string codepath)
        {
            foreach (var meta in metalist) {
                MakeCsharpRegroupFile(meta, codepath);
                string csfile = codepath + meta.GetClassName() + ".cs";
                using (FileStream fs = new FileStream(csfile, FileMode.Create, FileAccess.Write)) {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                        sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                        sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                        sw.WriteLine();
                        sw.WriteLine("using System.Collections;");
                        sw.WriteLine("using System.Collections.Generic;");
                        sw.WriteLine("using System.Data;");
                        sw.WriteLine();
                        sw.WriteLine("namespace {0} {{", space_name);
                        sw.WriteLine("public class {0} {{", meta.GetItemName());
                        foreach (var field in meta.Fields) {
                            sw.WriteLine("\tpublic {0} {1};", field.GetCsharpTypeName(), field.fieldName);
                        }
                        foreach (var field in meta.Fields) {
                            string relate_info = field.Get("relate");
                            if (relate_info != null && field.Get("regroup") == null) {
                                string[] s = relate_info.Split('.');
                                if (s.Count() != 2) {
                                    throw new Exception(meta.TableName + " relate error!!! field name " + field.fieldName);
                                }
                                meta.relate = true;
                                sw.WriteLine("\tpublic List<td_{0}_item> __relate__td_{0}_items = new List<td_{0}_item>();", s[0].ToLower());
                            }
                        }
                        sw.WriteLine("};");
                        sw.WriteLine();
                        sw.WriteLine("public class {0} : TableManager<{1}, {0}> {{", meta.GetClassName(), meta.GetItemName());
                        sw.WriteLine("\tpublic override bool Load() {");
                        sw.WriteLine("\t\tTableReader tr = new TableReader();");
                        sw.WriteLine("\t\tDataReader dr = new DataReader();");
                        sw.WriteLine("\t\tDataTable dt = tr.ReadFile(MyConfig.WorkDir+\"{0}.txt\");", meta.TableName);
                        sw.WriteLine("\t\tforeach(DataRow row in dt.Rows) {");
                        sw.WriteLine("\t\t\tvar item = new {0}();", meta.GetItemName());

                        foreach (var field in meta.Fields) {
                            switch (field.fieldType) {
                                case TableFieldType.EnumField:
                                case TableFieldType.MaskField:
                                case TableFieldType.IntField:
                                    sw.WriteLine("\t\t\titem.{0} = dr.GetValue<int>(row[\"{0}\"].ToString());", field.fieldName);
                                    break;
                                case TableFieldType.Int64Field:
                                    sw.WriteLine("\t\t\titem.{0} = dr.GetValue<long>(row[\"{0}\"].ToString());", field.fieldName);
                                    break;
                                case TableFieldType.FloatField:
                                    sw.WriteLine("\t\t\titem.{0} = dr.GetValue<float>(row[\"{0}\"].ToString());", field.fieldName);
                                    break;
                                case TableFieldType.DoubleField:
                                    sw.WriteLine("\t\t\titem.{0} = dr.GetValue<double>(row[\"{0}\"].ToString());", field.fieldName);
                                    break;
                                case TableFieldType.StringField:
                                    sw.WriteLine("\t\t\titem.{0} = (row[\"{0}\"].ToString());", field.fieldName);
                                    break;
                                case TableFieldType.TupleField:
                                    //TODO:
                                    break;
                                case TableFieldType.StructField:
                                   sw.WriteLine("\t\t\titem.{0} = dr.GetObject<{1}>(row[\"{0}\"].ToString());", field.fieldName, field.GetCsharpTypeName());
                                   break;
                                case TableFieldType.ListField:
                                    if (field.subType == TableFieldType.StructField) {
                                        sw.WriteLine("\t\t\titem.{0} = dr.GetObjectList<{1}>(row[\"{0}\"].ToString());", field.fieldName, field.realType);
                                    }
                                    else {
                                        sw.WriteLine("\t\t\titem.{0} = dr.GetList<{1}>(reader.GetValue(i, \"{0}\"));", field.fieldName, field.FieldTypeToString(field.subType));
                                    }
                                    break;
                                case TableFieldType.MapField:
                                    break;

                            }
                        }
                        sw.WriteLine("\t\t\tm_Items[item.{0}] = item;", meta.Fields[0].fieldName);//必须有一个id
                        sw.WriteLine("\t\t}");
                        sw.WriteLine("\t\treturn true;");
                        sw.WriteLine("\t}");
                        sw.WriteLine("}");
                        sw.WriteLine("}");
                        sw.WriteLine();
                    }
                }
            }

            string cppfile = codepath + "table_load.cs";
            using (FileStream fs = new FileStream(cppfile, FileMode.Create, FileAccess.Write)) {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                    sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                    sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                    sw.WriteLine();
                    sw.WriteLine("namespace {0} {{", space_name);
                    sw.WriteLine("public class TableConfig : SingletonTable<TableConfig> {");
                    sw.WriteLine("\tpublic bool LoadTableConfig() {");
                    foreach (var meta in metalist) {
                        sw.WriteLine("\t\tif (!{0}.Instance.Load()) return false;", meta.GetClassName());
                    }
                    sw.WriteLine("\t//relate code");
                    foreach (var meta in metalist) {
                        if (meta.relate) {
                            foreach (var field in meta.Fields) {
                                string relate_info = field.Get("relate");
                                if (relate_info != null && field.Get("regroup") == null) {
                                    string[] s = relate_info.Split('.');
                                    sw.WriteLine("\t\tforeach(var p1 in {0}.Instance.GetTable()) {{", meta.GetClassName());
                                    sw.WriteLine("\t\t\tvar v1 = p1.Value;");
                                    sw.WriteLine("\t\t\tvar k1 = v1.{0};", field.fieldName);
                                    sw.WriteLine("\t\t\tforeach(var p2 in td_{0}.Instance.GetTable()) {{", s[0].ToLower());
                                    sw.WriteLine("\t\t\t\tvar v2 = p2.Value;");
                                    sw.WriteLine("\t\t\t\tvar k2 = v2.{0};", s[1].ToLower());
                                    sw.WriteLine("\t\t\t\tif(k1 == k2) {");
                                    sw.WriteLine("\t\t\t\t\tv1.__relate__td_{0}_items.Add(v2);", s[0].ToLower());
                                    sw.WriteLine("\t\t\t\t}");
                                    sw.WriteLine("\t\t\t}");
                                    sw.WriteLine("\t\t}");
                                }
                            }
                        }
                    }
                    sw.WriteLine("\t\t\treturn true;");
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                    sw.WriteLine("}");
                }
            }

        }
        public static void MakeCppEnumAndMask(string excel_path, string codepath) {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(excel_path + "/Enum/", "*.xlsm", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(excel_path + "/Mask/", "*.xlsm", SearchOption.TopDirectoryOnly));
            ExcelHelper helper = new ExcelHelper();
            foreach (string filepath in files) {
                    try {
                        DataTable dt = helper.ImportExcelFile(filepath);
                        string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
                        string path = codepath + filename + ".h";
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                            sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                            sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());
                            sw.WriteLine("#pragma once");
                            sw.WriteLine("namespace {0} {{", filename);
                            sw.WriteLine("\tenum {0} {{", filename);
                            for (int i = 0; i < dt.Rows.Count; i++) {
                                sw.WriteLine("\t\t{0} = {1}, //{2}", dt.Rows[i].ItemArray[1].ToString().ToLower(), dt.Rows[i].ItemArray[0].ToString().ToLower(), dt.Rows[i].ItemArray[2].ToString().ToLower());
                            }
                            sw.WriteLine("\t};");
                            sw.WriteLine("\tinline char const* {0}_to_string(int val) {{", filename);
                            sw.WriteLine("\t\tswitch(val) {");
                            for (int i = 0; i < dt.Rows.Count; i++) {
                                sw.WriteLine("\t\t\tcase {0}:{{ return \"{1}\";}}", dt.Rows[i].ItemArray[1].ToString().ToLower(), dt.Rows[i].ItemArray[1].ToString().ToLower());
                            }
                            sw.WriteLine("\t\t\tdefault:{ return \"\";}");
                            sw.WriteLine("\t\t}");
                            sw.WriteLine("\t};");
                            sw.WriteLine("}");
                            sw.Close();
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(filepath + " 文件出错！");
                        Console.WriteLine(e.ToString());
                    }
            }
            Console.WriteLine("<生成C++枚举掩码文件成功>");
        }
        public static void MakeCsharpEnumAndMask(string excel_path, string codepath) {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(excel_path + "/Enum/", "*.xlsm", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(excel_path + "/Mask/", "*.xlsm", SearchOption.TopDirectoryOnly));
            ExcelHelper helper = new ExcelHelper();       
            string path = codepath + "enum_types.cs";
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!");
                sw.WriteLine("//GENERATE TIME [{0}]", System.DateTime.Now.ToString());                
                foreach (string filepath in files) {
                    try {
                    DataTable dt = helper.ImportExcelFile(filepath);
                    string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
                    sw.WriteLine("namespace {0} {{", filename);
                        sw.WriteLine("\tenum {0} {{", filename);
                        for (int i = 0; i < dt.Rows.Count; i++) {
                            sw.WriteLine("\t\t{0} = {1}, //{2}", dt.Rows[i].ItemArray[1].ToString().ToLower(), dt.Rows[i].ItemArray[0].ToString().ToLower(), dt.Rows[i].ItemArray[2].ToString().ToLower());
                        }
                        sw.WriteLine("\t};");
                        //sw.WriteLine("\tstring {0}_to_string({0} val) {{", filename);
                        //sw.WriteLine("\t\tswitch(val) {");
                        //for (int i = 0; i < dt.Rows.Count; i++) {
                        //    sw.WriteLine("\t\t\tcase {2}.{0}:{{ return \"{1}\";}}", dt.Rows[i].ItemArray[1].ToString().ToLower(), dt.Rows[i].ItemArray[1].ToString().ToLower(), filename);
                        //}
                        //sw.WriteLine("\t\t\tdefault:{ return \"\";}");
                        //sw.WriteLine("\t\t}");
                        //sw.WriteLine("\t}");
                        sw.WriteLine("}");
                    }
                    catch (Exception e) { 
                        Console.WriteLine(filepath + " 文件出错！");
                        Console.WriteLine(e.ToString());
                    }
                }
                sw.Close();
            }
            Console.WriteLine("<生成C#枚举掩码文件成功>");
        }
    }
}