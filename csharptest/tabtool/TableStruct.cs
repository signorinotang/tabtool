﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2018/3/22 17:29:25]

namespace game_data {
  public class tbsIdCount : ITableObject
  {
      int id;
      int count;
      public bool FromString(string s)
      {
          DataReader dr = new DataReader();

          var vs = s.Split(',');
          if (vs.Length != 2) {
              return false;
          }

		id = dr.GetValue<int>(vs[0]);
		count = dr.GetValue<int>(vs[1]);
          return true;
      }
  };

  public class tbsKeyValue : ITableObject
  {
      int key;
      int value;
      public bool FromString(string s)
      {
          DataReader dr = new DataReader();

          var vs = s.Split(',');
          if (vs.Length != 2) {
              return false;
          }

		key = dr.GetValue<int>(vs[0]);
		value = dr.GetValue<int>(vs[1]);
          return true;
      }
  };

  public class tbsTest : ITableObject
  {
      int a;
      string b;
      float c;
      public bool FromString(string s)
      {
          DataReader dr = new DataReader();

          var vs = s.Split(',');
          if (vs.Length != 3) {
              return false;
          }

		a = dr.GetValue<int>(vs[0]);
		b = vs[1];
		c = dr.GetValue<float>(vs[2]);
          return true;
      }
  };

}
