# tabtool
导表工具，excel表格导出csv配置文件并生成C++\C#代码解析配表

## 推荐工作流：
   策划案--解决方案--前后端配置需求汇总--表格版本--打表工具--配置文件--代码生成。
   
## 命名规则。
1. 驼峰命名。
2. 表格命名：系统名+表名.xsl 
3. 字段命名：使用通用的单词 id type count 等

## excel表规则。
1. 普通数据表。
-    第一行name，也是生成代码中的结构体字段名称
-    第二行type，参考下面的字段类型说明。
-    第三行注释，给策划看，也会在生成代码中作为注释
-    每个表第一个字段必须是id字段，id必须从1开始，0是读表错误。
-    id字段的filter如果标识为"client"则表示这个表只导出客户端配置文件，不导出服务器配表。反之亦然。(默认不填都导出 null为不导出)
2. 枚举掩码表。
-    第一列，枚举掩码 数值
-    第二列，也是生成代码中的结构体字段名称 （不可重复）
-    第三列，策划描述 会显示在配置的excel中 （不可重复）
-    enum(table_name) 类型填写后 单击表格 会自动弹出枚举点选框
## 字段类型
-    int 整数和bool
-    int64 整数和bool
-    float 浮点数
-    double 浮点数
-    string 字符串
-    datetime 日期类型(还没支持)
-    enum 枚举类型 eg: enum(table_name)
-    mask 掩码类型 eg: mask(table_name)
-    tbsIdCount 定义在meta.tbs中的结构体
-    list 容器类型 eg:  list(int) list(float)
    
## 复合字段及其迭代
- 一级字段迭代：`11,22,33,44`在type中用`list(int)`表示。
- 二级字段迭代：`1,1|2,2`在type中用`list(tbsIdCount)`表示。
- 为了全表结构统一 不支持自定义分隔符 普通分割 |   二级分割 , |
- 通过一个结构描述文件支持结构体，`meta.tbs`。
- 我认为表字段结构体嵌套是没有意义的，所以仅支持到二级复合字段。
- 注意excel中填写,时要设置单元格为文本模式，否则会变成数字分隔符。
- tbs文件非常简单，如下就定义一个结构体tbsIdCount:
```c
//表示id和数量
tbsIdCount {
    id int
    count int
}
```
## 代码生成
- C++版本 tbs文件tablestruct.h  csv文件生成生成一对tableconfig.cpp
- C#版本  tbs文件生成TableStruct.cs  csv文件生成TableConfig.cs
- Go版本  TODO 暂时没用到，用到了再支持

## 错误检查 所有的检查条件 用=分割
- 枚举表的自检 索引是否重复 字段时候重复
- 掩码表的自检 是否符合掩码规则（1 2 4 8.....） 字段是否重复
- 字段类型检查 是否未定义的字段类型
- 填写枚举检查  检查填写枚举是否存在
- 填写掩码检查  检查填写掩码是否超出范围
- 存在校验 检查所填数据是否数据x表x字段中  eg:  =  exist table_name.field_name
- 长度校验 检查所填数据的长度 eg: = length 5
- 关联设置 关联表格数据 避免多次查找 eg: relate = tablename.field_name
- 聚合设置 表进行聚合 （非id字段作为主键 进行聚合 变为  map<int, list<xxx>>） 自行理解 （支持中）
- 值校验  大小 范围 等等 （暂式不支持 还没想好） eg:  = value < 100 

## 导表工具使用
    参考test目录中`一键导出表.bat`的用法。
```
    "../tabtool/bin/Debug/tabtool.exe" --out_client ../csharptest/config/ --out_server ../cpptest/config/ --out_cpp ../cpptest/ --out_cs ../csharptest/ --in_excel ./ --in_tbs ./meta.tbs
```--out_client 指定导出客户端导出配置文件目录
   --out_server 导出服务器配置文件目录
   --out_cpp 导出C++代码目录，可选
   --out_cs 导出C#代码目录，可选
   --in_excel excel文件所在的目录
   --in_tbs tbs文件路径（表中用到的结构体）
   --space_name 命名空间指定
 ## C++使用  参考test
 ## C# 使用   参考test
 
 
