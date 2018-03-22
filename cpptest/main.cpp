//#include "tabtool/myconfig.h"
#include <iostream>
#include "tabtool/td_actor.hpp"
#include "tabtool/resource_type.h"
#include "tabtool/attack_type.h"
#include "tabtool/td_actorlvup.hpp"


namespace game_data {
	bool load_all_data();
}


int main(int argc, char **argv) {
	int i = 0;

	if (!game_data::load_all_data()) {
		std::cerr << "LOAD TABLE DATA ERROR!! SERVER NOT START!!!" << std::endl;
		return -1;
	}

	auto actor = ::GET_TABLE_DATA<game_data::td_regroup_actorlvupmap>(1);
	if (actor == nullptr) {
		int i = 0;
		++i;
	}

} 


