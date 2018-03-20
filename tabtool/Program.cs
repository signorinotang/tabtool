using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.IO;

namespace tabtool
{
    class Program
    {
        static void Main(string[] args) {

            //args = new string[] { "--out_client", "../../../csharptest/tabtool/config/", "--out_server", "../../../cpptest/tabtool/config/", "--out_cpp", "../../../cpptest/tabtool/", "--out_cs", "../../../csharptest/tabtool/", "--in_excel", "../../../test/doc/", "--in_tbs", "../../../test/doc/meta.tbs" };

            string clientOutDir, serverOutDir, cppOutDir, ServerEnumDir, csOutDir, excelDir, metafile;
            CmdlineHelper cmder = new CmdlineHelper(args);
            if (cmder.Has("--out_client")) { clientOutDir = cmder.Get("--out_client"); } else {
                return;
            }
            if (cmder.Has("--out_server")) { serverOutDir = cmder.Get("--out_server"); } else {
                return;
            }
            if (cmder.Has("--in_excel")) { excelDir = cmder.Get("--in_excel"); } else { return; }
            if (cmder.Has("--in_tbs")) { metafile = cmder.Get("--in_tbs"); } else { return; }
            if (cmder.Has("--space_name")) { CodeGen.space_name = cmder.Get("space_name"); }

            //创建导出目录
            if (!Directory.Exists(clientOutDir)) Directory.CreateDirectory(clientOutDir);
            if (!Directory.Exists(serverOutDir)) Directory.CreateDirectory(serverOutDir);

            //先读取tablemata文件
            TableStruct tbs = new TableStruct();
            if (!tbs.ImportTableStruct(metafile)) {
                Console.WriteLine("解析tbs文件错误！");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey(false);
            }
            Console.WriteLine("<解析tbs文件成功>");

            List<TableMeta> clientTableMetaList = new List<TableMeta>();
            List<TableMeta> serverTableMetaList = new List<TableMeta>();
            List<TableMeta> allTableMeataList = new List<TableMeta>();
            //导出文件
            ExcelHelper helper = new ExcelHelper();
            //读取数据表
            string[] files = Directory.GetFiles(excelDir, "*.xlsm", SearchOption.TopDirectoryOnly);
            foreach (string filepath in files) {
                string clientfile = clientOutDir + Path.GetFileNameWithoutExtension(filepath) + ".txt";
                string serverfile = serverOutDir + Path.GetFileNameWithoutExtension(filepath) + ".txt";
                try {
                    DataTable dt = helper.ImportExcelFile(filepath);
                    TableMeta meta = null;
                    if (helper.IsExportFile("client", dt)) {
                        meta = helper.ParseTableMeta(Path.GetFileNameWithoutExtension(filepath), dt, "client");
                        clientTableMetaList.Add(meta);
                    }
                    if (helper.IsExportFile("server", dt)) {
                        meta = helper.ParseTableMeta(Path.GetFileNameWithoutExtension(filepath), dt, "server");
                        serverTableMetaList.Add(meta);
                    }
                    if(meta != null)  allTableMeataList.Add(meta);
                }
                catch (Exception e)
                {
                    Console.WriteLine(filepath + " 文件出错！");
                    Console.WriteLine(e.ToString());
                }
            }
            Console.WriteLine("<导出配置文件成功>");
                 
            try {
                //1 进行所有表格数据校验
                Verifier.CheckTable(allTableMeataList, tbs.GetMetaList(), excelDir);
                Console.WriteLine("<表格校验成功>");
                //2 导出配置文件
                //导出客户端配置
                foreach(var meta in clientTableMetaList) {
                    helper.ExportTxtFileEx(clientOutDir + meta.TableName + ".txt", meta.dt, "client", new int[] { 1, 2 });
                }
                //导出服务器配置
                foreach (var meta in serverTableMetaList) {
                    helper.ExportTxtFileEx(serverOutDir + meta.TableName + ".txt", meta.dt, "server", new int[] { 1, 2 });
                }
                Console.WriteLine("<导出配置文件成功>");
                //3 生成c++代码
                if (cmder.Has("--out_cpp")) {      
                    ServerEnumDir = cmder.Get("--server_enum");
                    if (ServerEnumDir != null && !Directory.Exists(ServerEnumDir))
                        Directory.CreateDirectory(ServerEnumDir);
                    CodeGen.MakeCppEnumAndMask(excelDir, ServerEnumDir);
                    cppOutDir = cmder.Get("--out_cpp");
                    if (!Directory.Exists(cppOutDir))
                        Directory.CreateDirectory(cppOutDir);
                    CodeGen.MakeCppFileTbs(tbs.GetMetaList(), cppOutDir);
                    CodeGen.MakeCppFile(serverTableMetaList, cppOutDir);
                    Console.WriteLine("<生成.cpp代码文件成功>");
                }
                //4 生成c#代码
                if (cmder.Has("--out_cs")) {
                    csOutDir = cmder.Get("--out_cs");
                    if (!Directory.Exists(csOutDir))
                        Directory.CreateDirectory(csOutDir);
                    CodeGen.MakeCsharpEnumAndMask(excelDir, csOutDir);
                    CodeGen.MakeCsharpFileTbs(tbs.GetMetaList(), csOutDir);
                    CodeGen.MakeCsharpFile(clientTableMetaList, csOutDir);
                    Console.WriteLine("<生成.cs代码文件成功>");
                }
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("按任意键退出...");
            Console.ReadKey(false);
        }
    }
}
