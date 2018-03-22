using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game_data;

namespace csharptest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (TableConfig.Instance.LoadTableConfig()) {
               var item =  td_actor.Instance.GetTableItem(1);
               var atcor = td_regroup_actorlvupmap.Instance.GetTableItem(1);
            }
        }
    }
}
