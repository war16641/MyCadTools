using MyGeometrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
//using MyGeometrics;

namespace MyDataExchange
{
    public class MyDataExchange
    {
        public static bool make_data_from_file(string path, out Dictionary<string, object> dic, int ignore_hang = 3)
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
            return make_data_from_paragraph(wholetxt, out dic, ignore_hang);
        }

        public static void split_paragraph_to_lines(string paragraph, out List<string> lines)
        {
            lines = new List<string>();
            foreach (var item in paragraph.Split('\n'))
            {
                lines.Add(item);
            }
        }


        public static string make_data_from_line(string line, out string name, out object val, Dictionary<string, object> dic)
        {
            name = "";
            val = null;
            string strPatten = @"^\s*(?<name>\S+)\s+(?<type>\S+)\s+(?<rawtxt>.+)";
            Regex rex = new Regex(strPatten);
            //MatchCollection matches = rex.Matches(cur_line);
            Match m = rex.Match(line);
            if (m == null) throw new Exception("错误的行");

            //根据type写入值 进入dic
            if ("float" == m.Groups["type"].Value)
            {
                //dic.Add(m.Groups["name"].Value, Convert.ToDouble(m.Groups["rawtxt"].Value));
                name = m.Groups["name"].Value;
                val = Convert.ToDouble(m.Groups["rawtxt"].Value);
                return "s";
            }
            else if ("string" == m.Groups["type"].Value)
            {
                //dic.Add(m.Groups["name"].Value, Convert.ToString(m.Groups["rawtxt"].Value));
                name = m.Groups["name"].Value;
                val = Convert.ToString(m.Groups["rawtxt"].Value);
                return "s";
            }
            else if ("vector" == m.Groups["type"].Value)
            {
                string[] strs = m.Groups["rawtxt"].Value.Split(',');
                if (3 != strs.Length) throw new Exception("vector应该有三个数");
                name = m.Groups["name"].Value;
                val = new Vector3D(Convert.ToDouble(strs[0]),
                                                            Convert.ToDouble(strs[1]),
                                                            Convert.ToDouble(strs[2]));
                return "s";
                //dic.Add(m.Groups["name"].Value, new Vector3D(Convert.ToDouble(strs[0]),
                //                                            Convert.ToDouble(strs[1]),
                //                                            Convert.ToDouble(strs[2])));
            }
            else if ("rect" == m.Groups["type"].Value)
            {
                string[] strs = m.Groups["rawtxt"].Value.Split(',');
                if (4 == strs.Length)
                {
                    if (!dic.ContainsKey(strs[0]))
                    {
                        throw new Exception("未找到向量名");
                    }
                    double w = Convert.ToDouble(strs[1]);
                    double h = Convert.ToDouble(strs[2]);
                    double r = Convert.ToDouble(strs[3]);
                    Vector3D v = (Vector3D)dic[strs[0]] + new Vector3D(w, h);
                    name = m.Groups["name"].Value;
                    val = new MyRect((Vector3D)dic[strs[0]], v);
                    return "s";
                }
                else if (6 == strs.Length)//指定两个角点
                {
                    Vector3D v1 = new Vector3D(Convert.ToDouble(strs[0]), Convert.ToDouble(strs[1]), Convert.ToDouble(strs[2]));
                    Vector3D v2 = new Vector3D(Convert.ToDouble(strs[3]), Convert.ToDouble(strs[4]), Convert.ToDouble(strs[5]));
                    name = m.Groups["name"].Value;
                    val = new MyRect(v1, v2);
                    return "s";
                }
                else
                {
                    throw new Exception("错误的rect格式");
                }


                //dic.Add(m.Groups["name"].Value, new MyRect((Vector3D)dic[strs[0]], v));

            }
            else if ("bool" == m.Groups["type"].Value)
            {
                bool f = true;
                if ("0" == m.Groups["rawtxt"].Value)
                {
                    f = false;
                }
                name = m.Groups["name"].Value;
                val = f;
                return "s";
                //dic.Add(m.Groups["name"].Value, f);
            }
            else if ("arc" == m.Groups["type"].Value)
            {
                string[] strs = m.Groups["rawtxt"].Value.Split(',');
                if (strs.Length != 6) throw new Exception("错误的arc格式");
                Vector3D center = new Vector3D(Convert.ToDouble(strs[0]), Convert.ToDouble(strs[1]), Convert.ToDouble(strs[2]));
                double radius = Convert.ToDouble(strs[3]);
                double angle1 = Convert.ToDouble(strs[4]);
                double da = Convert.ToDouble(strs[5]);
                double normalz = 1.0;
                if (da < 0) normalz = -1.0;
                name = m.Groups["name"].Value;
                val = new MyArc(center, radius, angle1, angle1 + da, normalz);
                return "s";
            }
            else if ("polyline" == m.Groups["type"].Value)
            {
                int hang = Convert.ToInt32(m.Groups["rawtxt"].Value);//得到下面的接续行个数 行数放在val中返回
                name = m.Groups["name"].Value;
                val = hang;
                return "m";
            }
            else if ("lineseg" == m.Groups["type"].Value)
            {
                string[] strs = m.Groups["rawtxt"].Value.Split(',');
                if (strs.Length != 6) throw new Exception("错误的lineseg格式");
                Vector3D v1 = new Vector3D(Convert.ToDouble(strs[0]), Convert.ToDouble(strs[1]), Convert.ToDouble(strs[2]));
                Vector3D v2 = new Vector3D(Convert.ToDouble(strs[3]), Convert.ToDouble(strs[4]), Convert.ToDouble(strs[5]));
                LineSegment elo = new LineSegment(v1, v2);
                name = m.Groups["name"].Value;
                val = elo;
                return "s";
            }
            else if ("cadop" == m.Groups["type"].Value)
            {
                name = m.Groups["name"].Value;
                val = Convert.ToString(m.Groups["rawtxt"].Value);
                return "s";
            }
            else
            {
                throw new Exception("错误的类型");
            }
        }
        public static bool make_data_from_paragraph(string paragraph, out Dictionary<string, object> dic,
                                                    int ignore_hang = 3)
        {
            int hanghao_start = ignore_hang + 1;//默认从第四行开始读
            int hanghao = 0;//当前行号
            string cur_line;//
            dic = new Dictionary<string, object>();
            split_paragraph_to_lines(paragraph, out List<string> lines);
            string rt, name;
            while (hanghao < lines.Count)
            {
                cur_line = lines[hanghao];
                hanghao++;
                if (hanghao < hanghao_start) continue;
                if (cur_line.Length == 0) continue;//跳过空行
                rt = MyDataExchange.make_data_from_line(cur_line, out name, out object val, dic);
                if (rt == "s")//独立的行数据
                {
                    dic.Add(name, val);
                }
                else if (rt == "m")//多行组成的数据
                {
                    int jiexuhao = (int)val;
                    int jiexuehao_ct = 0;//接续行的读数
                    Polyline pl = new Polyline();
                    while (true)
                    {
                        if (hanghao >= lines.Count) throw new Exception("行意外结束");
                        cur_line = lines[hanghao];
                        hanghao++;
                        if (cur_line.Length == 0) continue;//跳过空行
                        if ("m" == MyDataExchange.make_data_from_line(cur_line, out _, out object val1, dic))
                        {
                            throw new Exception("在读取多行数据中出现了另一个多行数据");
                        }
                        pl.segs.Add((Imygeometrics)val1);
                        jiexuehao_ct++;
                        if (jiexuehao_ct == jiexuhao)
                        {
                            dic.Add(name, pl);
                            break;
                        }


                    }


                }




            }
            return true;
        }
    }
}

