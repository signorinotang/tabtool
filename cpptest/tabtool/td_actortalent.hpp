﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2018/3/22 17:29:25]
#pragma once
# include "readtablefield.h"
# include "tablestruct.h"

namespace game_data {

struct td_actortalent_item {
  int32_t id;
  int32_t talent_id;
};

class td_actortalent : public IConfigTable<td_actortalent_item>{
public:
virtual bool Load() {
	ReadTableFile reader;
	reader.Initialize();
	if (!reader.Init(GetTableFile().c_str()))
		return false;
	try {
		DataReader dr;
		int iRows = reader.GetRowCount();
		int iCols = reader.GetColCount();
		for (int i = 1; i < iRows; ++i) {
			td_actortalent_item item;
			item.id = dr.GetValue<int32_t>(reader.GetValue(i, "id"));
			item.talent_id = dr.GetValue<int32_t>(reader.GetValue(i, "talent_id"));
			m_Items[item.id] = item;
		}
	} catch(std::exception& e) {
		std::cerr << e.what() << std::endl;
		return false;
	}
	return true;
}
string GetTableFile() {
	string f = WORK_DIR;
	f = f + "ActorTalent.txt";
	return f;
}
};
}


