using MyGeometrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCadTools
{
    class Tool1
    {
        public static bool make_data_from_file(string path, out Dictionary<string, object> dic)
        {
            StreamReader srReadFile = new StreamReader(path);
            string wholetxt = srReadFile.ReadLine();
            // 读取流直至文件末尾结束
            while (!srReadFile.EndOfStream)
            {
                wholetxt += "\n" + srReadFile.ReadLine(); //读取每行数据
            }

            // 关闭读取流文件
            srReadFile.Close();
            return make_data_from_paragraph(wholetxt, out dic);
        }

        public static void split_paragraph_to_lines(string paragraph, out List<string> lines)
        {
            lines = new List<string>();
            foreach (var item in paragraph.Split('\n'))
            {
                lines.Add(item);
            }
        }
        public static bool make_data_from_paragraph(string paragraph, out Dictionary<string, object> dic)
        {
            int hanghao_start = 4;//默认从第四行开始读
            int hanghao = 0;//当前行号
            string cur_line;//
            dic = new Dictionary<string, object>();
            split_paragraph_to_lines(paragraph, out List<string> lines);
            while (hanghao < lines.Count)
            {
                cur_line = lines[hanghao];
                hanghao++;
                if (hanghao < hanghao_start) continue;

                string strPatten = @"^(?<name>\S+) (?<type>\S+) (?<rawtxt>.+)";
                Regex rex = new Regex(strPatten);
                //MatchCollection matches = rex.Matches(cur_line);
                Match m = rex.Match(cur_line);
                if (m == null) throw new Exception("错误的行");
                Console.WriteLine(m.Groups["name"].Value);
                //根据type写入值 进入dic
                if ("float" == m.Groups["type"].Value)
                {
                    dic.Add(m.Groups["name"].Value, Convert.ToDouble(m.Groups["rawtxt"].Value));
                }
                else if ("string" == m.Groups["type"].Value)
                {
                    dic.Add(m.Groups["name"].Value, Convert.ToString(m.Groups["rawtxt"].Value));
                }
                else if ("vector" == m.Groups["type"].Value)
                {
                    string[] strs = m.Groups["rawtxt"].Value.Split(',');
                    if (3 != strs.Length) throw new Exception("vector应该有三个数");
                    dic.Add(m.Groups["name"].Value, new Vector3D(Convert.ToDouble(strs[0]),
                                                                Convert.ToDouble(strs[1]),
                                                                Convert.ToDouble(strs[2])));
                }
                else
                {
                    throw new Exception("错误的类型");
                }
            }
            return true;
        }
    }
}
