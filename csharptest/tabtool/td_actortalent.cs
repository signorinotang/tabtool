﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2018/3/22 17:29:25]

using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace game_data {
public class td_actortalent_item {
	public int id;
	public int talent_id;
};

public class td_actortalent : TableManager<td_actortalent_item, td_actortalent> {
	public override bool Load() {
		TableReader tr = new TableReader();
		DataReader dr = new DataReader();
		DataTable dt = tr.ReadFile(MyConfig.WorkDir+"ActorTalent.txt");
		foreach(DataRow row in dt.Rows) {
			var item = new td_actortalent_item();
			item.id = dr.GetValue<int>(row["id"].ToString());
			item.talent_id = dr.GetValue<int>(row["talent_id"].ToString());
			m_Items[item.id] = item;
		}
		return true;
	}
}
}

