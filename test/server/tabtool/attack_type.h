﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2018/1/19 15:42:21]
#pragma once
namespace attack_type {
	enum attack_type {
		fly = 1, //飞行
		water = 2, //水中
		land = 4, //陆地
		stealth = 8, //隐身
	};
	inline char const* attack_type_to_string(int val) {
		switch(val) {
			case fly:{ return "fly";}
			case water:{ return "water";}
			case land:{ return "land";}
			case stealth:{ return "stealth";}
			default:{ return "";}
		}
	};
}
