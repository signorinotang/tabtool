using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace tabtool {
    //校验类型
    enum check_type {
        enum_self, //枚举自检
        mask_self, //掩码自检
        field_type, //字段类型检查

    }


    class Verifier {
        public static void CheckTable(List<TableMeta> metalist, List<TableMeta> tbs_metalist, string excelDir) {//所有的校验都在这里
            ExcelHelper helper = new ExcelHelper();
            Dictionary<string, int> check_dic = new Dictionary<string, int>();
            //枚举表自检
            string[] enums = Directory.GetFiles(excelDir + "/Enum/", "*.xlsm", SearchOption.TopDirectoryOnly);
            foreach (var filepath in enums) {
                DataTable dt = helper.ImportExcelFile(filepath);
                for (int i = 0; i < dt.Columns.Count; i++) {
                    for (int j = 0; j < dt.Rows.Count; j++) {
                        if(i == 0) {
                            int enum_value = 0;
                            bool success = int.TryParse(dt.Rows[j].ItemArray[i].ToString(), out enum_value);
                            if(!success)
                                throw new Exception("[ENUM SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Not Int " + dt.Rows[j].ItemArray[i].ToString() + " Row:" + (j + 1).ToString() + " Col:" + (i + 1).ToString());
                        }
                        if(check_dic.ContainsKey(dt.Rows[j].ItemArray[i].ToString())) {
                            throw new Exception("[ENUM SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Repeat " + dt.Rows[j].ItemArray[i].ToString() + " Row:" + (j + 1).ToString() + " Col:" + (i + 1).ToString());
                        }
                        check_dic.Add(dt.Rows[j].ItemArray[i].ToString(), 0);
                    }
                    check_dic.Clear();
                }
            }
            //掩码表自检
            string[] masks = Directory.GetFiles(excelDir + "/Mask/", "*.xlsm", SearchOption.TopDirectoryOnly);
            foreach (var filepath in masks) {
                DataTable dt = helper.ImportExcelFile(filepath);
                for (int i = 0; i < dt.Columns.Count; i++) {
                    int last_mask = 1, now_mask = 0;
                    for (int j = 0; j < dt.Rows.Count; j++) {
                        if (i == 0) {
                            bool success = int.TryParse(dt.Rows[j].ItemArray[i].ToString(), out now_mask);
                            if (success) {
                                if (j != 0) {
                                    if (now_mask / last_mask != 2) {
                                        throw new Exception("[MASK SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Not " + (last_mask *= 2).ToString() + " Row:" + (j + 1).ToString() + " Col:1");
                                    }
                                } else {
                                    if(now_mask != 1) {
                                        throw new Exception("[MASK SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Not 1 Row:" + (j + 1).ToString() + " Col:1");
                                    }
                                }
                                last_mask = now_mask;
                            }
                            else {
                                throw new Exception("[MASK SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Not Int " + dt.Rows[j].ItemArray[i].ToString() + " Row:" + (j + 1).ToString() + " Col:" + (i + 1).ToString());
                            }
                        }
                        if (check_dic.ContainsKey(dt.Rows[j].ItemArray[i].ToString())) {
                            throw new Exception("[Mask SELF CHECK ERROR] Table:" + Path.GetFileNameWithoutExtension(filepath) + " Value Repeat " + dt.Rows[j].ItemArray[i].ToString() + " Row:" + (j + 1).ToString() + " Col:" + (i + 1).ToString());
                        }
                        check_dic.Add(dt.Rows[j].ItemArray[i].ToString(), 0);
                    }
                    check_dic.Clear();
                }
            }
            //索引校验（主键不可重复）


            //条件检验
            foreach (var meta in metalist) {
                for (int i = 0; i < meta.Fields.Count(); ++i) {
                    var field = meta.Fields[i];
                    string check_info = null;
                    //字段类型检查是否合法:
                    if(field.fieldType == TableFieldType.StructField) {
                       bool find = false;
                       foreach (var s in tbs_metalist) {
                            if(s.TableName == field.realType) {
                                find = true;
                                break;                          
                            }
                       }
                       if(!find)
                            throw new Exception("[FIELD CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Undefined Type " + meta.dt.Rows[1].ItemArray[i] + " Row:1" + " Col:" + (i + 1).ToString());
                    } 
                    //检查字段所填枚举是否存在
                    if (field.fieldType == TableFieldType.EnumField) {
                        string filepath = excelDir + "/Enum/" + field.realType + ".xlsm";
                        DataTable check_dt = helper.ImportExcelFile(filepath);
                        for (int k = 3; k < meta.dt.Rows.Count; ++k) {
                            bool find_value = false;
                            for (int m = 0; m < check_dt.Rows.Count; ++m) {
                                if (meta.dt.Rows[k].ItemArray[i].ToString() == check_dt.Rows[m].ItemArray[2].ToString()) {                                   
                                    meta.dt.Rows[k][i] = check_dt.Rows[m].ItemArray[0].ToString();
                                    find_value = true;
                                    break;
                                }
                            }
                            if (!find_value) {
                                throw new Exception("[ENUM CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Value " + meta.dt.Rows[k].ItemArray[i] + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            }
                        }
                        continue;
                    }
                    //检查字段所填掩码是否存在
                    if (field.fieldType == TableFieldType.MaskField) {
                        string filepath = excelDir + "/Mask/" + field.realType + ".xlsm";
                        DataTable check_dt = helper.ImportExcelFile(filepath);
                        int check_int = 0, mask = 0;
                        for (int m = 0; m < check_dt.Rows.Count; ++m) {
                            if(int.TryParse(check_dt.Rows[m].ItemArray[0].ToString(), out mask)) {
                                check_int += mask;
                            } else {
                                throw new Exception("[MASK CHECK ERROR] Table:" + field.realType + " Value Not Int " + meta.dt.Rows[m].ItemArray[0] + " Row:" + (m + 1).ToString() + " Col:1");
                            }
                        }
                        for (int k = 3; k < meta.dt.Rows.Count; ++k) {                                  
                            if(int.TryParse(meta.dt.Rows[k].ItemArray[i].ToString(), out mask)) {
                                if(mask > check_int || mask < 0)
                                    throw new Exception("[MASK CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Value Out Of Mask Range ! " + meta.dt.Rows[k].ItemArray[i] + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            } else {
                                throw new Exception("[MASK CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Value Not Int " + meta.dt.Rows[k].ItemArray[i] + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            }                           
                        }
                        continue;
                    }
                    //检查该字段所填数据是否在XXX表中
                    //eg: exist head.id
                    if ((check_info = field.Get("exist")) != null) {
                        string[] c = check_info.Split('.');
                        if(c.Count() != 2) {
                            throw new Exception("[EXIST CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName +" ParamNum Not Two!!!");
                        }
                        //找到校验dt
                        DataTable check_dt = null;
                        int check_col = -1;            
                        foreach(var me in metalist) {
                            if(me.TableName.ToLower() == c[0].ToLower()) {
                                check_dt = me.dt;
                                for (int j = 0; j < me.dt.Columns.Count; ++j) {
                                    if(me.dt.Rows[0].ItemArray[j].ToString() == c[1].ToLower()) {
                                        check_col = j;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if(check_dt == null) {
                            throw new Exception("[EXIST CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Table " + c[0]);
                        }
                        if(check_col == -1) {
                            throw new Exception("[EXIST CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Field " + c[1]);
                        }
                        //遍历dt这一列
                        bool find_value = false;
                        for (int k = 3; k < meta.dt.Rows.Count; ++k) {
                            if (field.fieldType == TableFieldType.ListField) {
                                string[] s = meta.dt.Rows[k].ItemArray[i].ToString().Split('|');
                                foreach (var v in s) {
                                    for (int m = 3; m < check_dt.Rows.Count; ++m) {
                                        if (v == check_dt.Rows[m].ItemArray[check_col].ToString()) {
                                            find_value = true;
                                            break;
                                        }
                                    }
                                    if(!find_value) {
                                        break;
                                    }
                                }
                            }
                            else {
                                for (int m = 3; m < check_dt.Rows.Count; ++m) {
                                    if (meta.dt.Rows[k].ItemArray[i].ToString() == check_dt.Rows[m].ItemArray[check_col].ToString()) {
                                        find_value = true;
                                        break;
                                    }
                                }
                            }
                            if (!find_value) {
                                throw new Exception("[EXIST CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Value " + meta.dt.Rows[k].ItemArray[i] + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            }
                        }
                    }
                    //检查该字段所填数据长度是否正确
                    //eg: length 5
                    if ((check_info = field.Get("length")) != null) {
                        for (int k = 3; k < meta.dt.Rows.Count; ++k) {
                            int check_num = 0;
                            bool ret = int.TryParse(check_info, out check_num);
                            if(!ret) {
                                throw new Exception("[LENGTH CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Unrecognized " + check_info + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            }
                            if(!(meta.dt.Rows[k].ItemArray[i].ToString().Split('|').Count() == check_num ||
                               meta.dt.Rows[k].ItemArray[i].ToString().Split(',').Count() == check_num)
                                ) {
                                throw new Exception("[LENGTH CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Count Not Equal " + check_info + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                            }
                        }
                    }
                    //检查该字段所填值是否正确 值校验 大小 是否越界
                    //TODO:
                    //暂时还没想好 想好再说
                    if ((check_info = field.Get("value")) == null) {



                    }
                    //检查该字段所填值是否存在于关联表中
                    //eg: relate head.id
                    if ((check_info = field.Get("relate")) != null) {
                        if (field.fieldType == TableFieldType.ListField) {
                            throw new Exception("[RELATE CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " List Can`t Relate!!!");
                        }
                        string[] check_infos = check_info.Split('|');
                        foreach (var check in check_infos) {
                            string[] c = check.Split('.');
                            if (c.Count() != 2) {
                                throw new Exception("[RELATE CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " ParamNum Not Two!!!");
                            }
                            //找到校验dt
                            DataTable check_dt = null;
                            int check_col = -1;
                            foreach (var me in metalist) {
                                if (me.TableName.ToLower() == c[0].ToLower()) {
                                    check_dt = me.dt;
                                    for (int j = 0; j < me.dt.Columns.Count; ++j) {
                                        if (me.dt.Rows[0].ItemArray[j].ToString() == c[1].ToLower()) {
                                            check_col = j;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (check_dt == null) {
                                throw new Exception("[RELATE CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Table " + c[0]);
                            }
                            if (check_col == -1) {
                                throw new Exception("[RELATE CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Field " + c[1]);
                            }
                            //遍历dt这一列                    
                            for (int k = 3; k < meta.dt.Rows.Count; ++k) {
                                bool find_value = false;
                                for (int m = 3; m < check_dt.Rows.Count; ++m) {
                                    if (meta.dt.Rows[k].ItemArray[i].ToString() == check_dt.Rows[m].ItemArray[check_col].ToString()) {
                                        find_value = true;
                                        break;
                                    }
                                }
                                if (!find_value) {
                                    throw new Exception("[RELATE CHECK ERROR] Table:" + meta.TableName + " Field:" + field.fieldName + " Not Find Value " + meta.dt.Rows[k].ItemArray[i] + " Row:" + (k + 1).ToString() + " Col:" + (i + 1).ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}