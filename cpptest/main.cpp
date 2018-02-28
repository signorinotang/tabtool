//#include "tabtool/myconfig.h"
#include <iostream>
#include "tabtool/td_test.hpp"
#include "tabtool/resource_type.h"
#include "tabtool/attack_type.h"


namespace game_data {
	bool load_all_data();
}


int main(int argc, char **argv) {
	int i = 0;

	if (!game_data::load_all_data()) {
		std::cerr << "LOAD TABLE DATA ERROR!! SERVER NOT START!!!" << std::endl;
		return -1;
	}
	auto item = ::GET_TABLE<game_data::td_test>().GetTableItem(2);
	if (item != nullptr) {
		if (item->cost_type == resource_type::rt_gold) {
			int i = 0;
		}
		if (item->attack_type | attack_type::fly) {
			int i = 0;
		}
		if (item->attack_type | attack_type::land) {
			int i = 0;
		}
	}
	auto item2 = ::GET_TABLE_DATA<game_data::td_test>(1);
	if (item2 == nullptr) {

	}

}


