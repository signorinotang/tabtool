﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2018/1/19 15:42:21]
#pragma once
namespace res_code {
	enum res_code {
		success = 0, //成功-通用
		failed = 1, //失败-通用
	};
	inline char const* res_code_to_string(int val) {
		switch(val) {
			case success:{ return "success";}
			case failed:{ return "failed";}
			default:{ return "";}
		}
	};
}
