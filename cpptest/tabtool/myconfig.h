#pragma once

//说明：这个文件用来定制您的个性化需求

#define ErrorLog printf				//由于去除了日志类，错误日志用printf代替
#define WORK_DIR  "tabtool/config/";	//config所在目录

const int MAX_TABLE_ROWS = 8000;	//表最大行数
const int MAX_LINE_LEN = 8000;		//每行最大长度

//获取表数据模板
template<typename ty> 
inline ty const& GET_TABLE();

template<typename ty, typename key_ty>
inline typename ty::value_type const* GET_TABLE_DATA(key_ty const& key) {
	auto const& m = GET_TABLE<ty>();
	return m.GetTableItem(key);
}