using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace tabtool {
    enum TableFieldType {
        IntField,
        Int64Field,
        FloatField,
        DoubleField,
        StringField,
        TupleField,
        StructField,
        EnumField,//枚举
        MaskField,//掩码
        ListField,//c++(vector) c#(list)
        MapField, //c++(map)    c#(dictionary)
        DateField,//日期类型 格林位置时间
    }

    class TableField {
        public TableFieldType fieldType; //主类型
        public TableFieldType subType;   //子类型(当主类型为List 或者 Map)
        public string fieldName; //字段名
        public string realType;  //真实类型 cpp&c# (eg: c++ int32_t   c#  int)
        public List<string> Conditions = new List<string>();//条件数据
        public string Get(string s) {
            for (int i = 0; i < Conditions.Count(); i++) {
                if (Conditions[i] == s && i + 1 < Conditions.Count()) {
                    return Conditions[i + 1];
                }
            }
            return null;
        }

        public string FieldTypeToString(TableFieldType type) {
            switch (type) {
                case TableFieldType.IntField: { return "int32_t"; }
                case TableFieldType.Int64Field: { return "int64_t"; }
                case TableFieldType.FloatField: { return "float"; }
                case TableFieldType.DoubleField: { return "double"; }
                case TableFieldType.StringField: { return "string"; }
                default: { return realType; }
            }
        }


        public void SetSubType(string type) {
            if(type == "int") {
                subType = TableFieldType.IntField;
            } 
            else if(type == "int64") {
                subType = TableFieldType.Int64Field;
            }
            else if (type == "float") {
                subType = TableFieldType.FloatField;
            }
            else if (type == "double") {
                subType = TableFieldType.DoubleField;
            }
            else if (type == "string") {
                subType = TableFieldType.StringField;
            } 
            else {
                subType = TableFieldType.StructField;
            }
        }

        public string GetCppTypeName()
        {
            string[] ts = { "int32_t", "int64_t", "float", "double", "string", "xxx", "xxx", "int32_t", "int32_t", "xxx", "xxx"};
            if(fieldType == TableFieldType.ListField) {
                return string.Format("vector<{0}>", FieldTypeToString(subType));
            }
            if(fieldType == TableFieldType.MapField) {
                return string.Format("map<{0}>", FieldTypeToString(subType));
            }
            if(fieldType == TableFieldType.StructField) {
                return realType;
            }
            if(fieldType == TableFieldType.TupleField) {
                //TODO:
                return "";
            }
            return ts[(int)fieldType];
        }

        //public string GetTypeNameOfStructList()
        //{
        //    return typeName.Substring(0, typeName.Length - 1);
        //}


        public string GetCsharpTypeName()
        {
            string[] ts = { "int", "long", "float", "double", "string", "xxx", "xxx", "int", "int", "xxx", "xxx" };
            if (fieldType == TableFieldType.ListField) {
                return string.Format("List<{0}>", FieldTypeToString(subType));
            }
            if (fieldType == TableFieldType.MapField) {
                return string.Format("map<{0}>", FieldTypeToString(subType));
            }
            if (fieldType == TableFieldType.StructField) {
                return realType;
            }
            if (fieldType == TableFieldType.TupleField) {
                //TODO:
                return "";
            }
            return ts[(int)fieldType];
        }

    }

    class TableMeta
    {
        public string TableName;
        public DataTable dt;//表数据
        public List<TableField> Fields = new List<TableField>();
        public bool relate = false;//表中是否含有关联
        public bool regroup = false;//表中是否含有聚合
        public string GetClassName()
        {
            return "td_" + TableName.ToLower();
        }

        public string GetItemName()
        {
            return "td_" + TableName.ToLower() + "_item";
        }
    }
}
