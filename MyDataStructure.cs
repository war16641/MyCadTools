using System;

using System.Collections;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
namespace MyDataStructure
{

    public static class MyStatistic
    {
        /// <summary>
        /// 求和
        /// 如果这个object没有+，会抛出错误
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static object sum(List<object> lst)
        {
            object rt = null;
            foreach (var item in lst)
            {
                if (null == rt)
                {
                    rt = item;
                }
                else
                {
                    rt = (dynamic)rt + (dynamic)item;
                }
            }
            return rt;
        }

        public static int count(List<object> lst)
        {
            return lst.Count;
        }

        /// <summary>
        /// 求平均
        /// 要求实现+  ， 与数相乘 否则抛出错误
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static object avg(List<object> lst)
        {
            object rt = MyStatistic.sum(lst);
            return (dynamic)rt * (1.0 / (double)lst.Count);
        }

        public static object max(List<object> lst)
        {
            object cur = lst[0];
            for (int i = 1; i < lst.Count; i++)
            {
                if (((IComparable)cur).CompareTo(lst[i]) == -1)
                {
                    cur = lst[i];
                }
            }
            return cur;
        }
        public static object min(List<object> lst)
        {
            object cur = lst[0];
            for (int i = 1; i < lst.Count; i++)
            {
                if (((IComparable)cur).CompareTo(lst[i]) == 1)
                {
                    cur = lst[i];
                }
            }
            return cur;
        }

    }
    public class DataUnit : ICloneable
    {
        public Dictionary<string, object> data = new Dictionary<string, object>();
        public FlatDataModel fdm;

        public DataUnit(FlatDataModel f)
        {
            this.fdm = f;
        }
        public string detail()
        {
            string s = string.Format("共{0:D}个值。\n", this.data.Count);
            foreach (string item in this.data.Keys)
            {
                s += item + ":" + this.data[item].ToString() + "\n";
            }
            return s;
        }


        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            DataUnit du = new DataUnit(this.fdm);
            foreach (string key in this.data.Keys)
            {
                du.data.Add(key, this.data[key]);
            }
            return du;
        }
    }


    public class FlatDataModelException : ApplicationException
    {

        public FlatDataModelException(string s) : base(s)
        {

        }
    }
    public class FlatDataModel : IEnumerable, ICloneable
    {
        public List<DataUnit> units = new List<DataUnit>();
        public List<string> vn = new List<string>();//字段名
        public FlatDataModel()
        {

        }


        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < this.units.Count; i++)
            {
                yield return this.units[i];
            }
        }
        /// <summary>
        /// 从excel文件中载入
        /// 第一行作为vn 后续行作为数据
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        public static FlatDataModel load_from_excel(string pathname, string sheetname = null)
        {
            //create the Application object we can use in the member functions.
            Microsoft.Office.Interop.Excel.Application _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _excelApp.Visible = false;

            string fileName = pathname;

            //open the workbook
            Workbook workbook = _excelApp.Workbooks.Open(fileName,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);

            //select the first sheet       
            Worksheet worksheet;
            if (null == sheetname)
            {
                worksheet = (Worksheet)workbook.Worksheets[1];
            }
            else
            {
                worksheet = (Worksheet)workbook.Worksheets[sheetname];
            }


            //find the used range in worksheet
            Microsoft.Office.Interop.Excel.Range excelRange = worksheet.UsedRange;

            //get an object array of all of the cells in the worksheet (their values)
            object[,] valueArray = (object[,])excelRange.get_Value(
                        XlRangeValueDataType.xlRangeValueDefault);

            //access the cells 创建flatdatamodel
            FlatDataModel fdm = new FlatDataModel();

            for (int row = 1; row <= worksheet.UsedRange.Rows.Count; ++row)
            {
                DataUnit du = new DataUnit(fdm);
                for (int col = 1; col <= worksheet.UsedRange.Columns.Count; ++col)
                {
                    //access each cell
                    object thiscell = valueArray[row, col];
                    if (row == 1)//第一行 设定vn
                    {
                        fdm.vn.Add(thiscell.ToString());
                    }
                    else
                    {//其他行 赋值du
                        du.data.Add(fdm.vn[col - 1], thiscell);
                    }
                    //System.Diagnostics.Debug.Print(valueArray[row, col].ToString());



                }
                if (du.data.Count != 0)
                {
                    fdm.units.Add(du);
                }

            }

            //clean up stuffs
            workbook.Close(false, Type.Missing, Type.Missing);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);

            _excelApp.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_excelApp);

            return fdm;
        }

        public int Count
        {
            get
            {
                return this.units.Count;
            }
        }


        /// <summary>
        /// 在excel中显示自己
        /// 不会block
        /// </summary>
        public void show_in_excel()
        {
            //create the Application object we can use in the member functions.
            Microsoft.Office.Interop.Excel.Application _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _excelApp.Visible = true;



            //open the workbook
            Workbook workbook = _excelApp.Workbooks.Add();


            Worksheet worksheet = (Worksheet)workbook.Worksheets[1];
            //显示VN
            for (int i = 0; i < this.vn.Count; i++)
            {
                worksheet.Cells[1, i + 1] = this.vn[i];
            }
            //显示数据
            for (int i = 0; i < this.Count; i++)
            {
                for (int j = 0; j < this.vn.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = this.units[i].data[this.vn[j]];
                }
            }

        }


        /// <summary>
        /// 找到一个复合条件的u
        /// 没找到 返回null
        /// </summary>
        /// <param name="func"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataUnit find_one(System.Func<DataUnit, bool> func, out int index)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (func(this.units[i]))
                {
                    index = i;
                    return this.units[i];
                }
            }
            index = -1;
            return null;
        }

        /// <summary>
        /// 找到一个复合条件的u
        /// 没找到 返回null
        /// </summary>
        /// <param name="func"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataUnit find_one(System.Func<DataUnit, bool> func)
        {
            int _i;
            return this.find_one(func, out _i);
        }


        public void sort(System.Func<DataUnit, DataUnit, int> func)
        {
            this.units.Sort((x, y) => func(x, y));

        }

        public void sort(List<string> keys)
        {
            foreach (string item in keys)
            {
                if (!this.vn.Contains(item))
                {
                    throw new System.ArgumentException("关键字段不在字段名中。");
                }
            }

            this.units.Sort(delegate (DataUnit a, DataUnit b)
            {
                int t;
                foreach (string key in keys)
                {
                    if (!(a.data[key] is IComparable))
                    {
                        throw new FlatDataModelException(string.Format("字段{0}未实现Icomparable"));
                    }
                    IComparable ic = (IComparable)a.data[key];
                    t = ic.CompareTo(b.data[key]);
                    if (t != 0)
                    {
                        return t;
                    }
                    //比平了，比下一个
                }
                //如果所有字段都比平了
                return 1;
            });
        }

        public List<List<DataUnit>> make_bunch(List<string> classifyname)
        {
            foreach (string item in classifyname)
            {
                if (!this.vn.Contains(item))
                {
                    throw new System.ArgumentException("分类字段不在字段名中。");
                }
            }

            List<List<DataUnit>> rt = new List<List<DataUnit>>();
            //先排序
            this.sort(classifyname);

            //判断是否是同一个bunch
            System.Func<DataUnit, DataUnit, bool> is_same_bunch = delegate (DataUnit a, DataUnit b) {
                foreach (string n in classifyname)
                {

                    //if (a.data[n]!=b.data[n])
                    //{
                    //    return false;
                    //}

                    dynamic a1 = a.data[n];
                    dynamic b1 = b.data[n];
                    if (a1 != b1) return false;
                }
                return true;
            };


            List<DataUnit> cur_bunch = new List<DataUnit>();
            DataUnit cur_bunch_name = this.units[0];//当前bunch
            foreach (DataUnit item in this)
            {
                if (is_same_bunch(item, cur_bunch_name))
                {
                    cur_bunch.Add(item);
                }
                else
                {
                    //当前bunch结束
                    rt.Add(cur_bunch);
                    cur_bunch_name = item;
                    cur_bunch = new List<DataUnit>();
                    cur_bunch.Add(item);
                }
            }
            if (cur_bunch.Count > 0)
            {
                rt.Add(cur_bunch);
            }

            return rt;
        }


        /// <summary>
        /// 浅拷贝
        /// 如果是字符串和数 拷贝后与源完全独立
        /// 如果是对象 就是互连的
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            FlatDataModel fdm = new FlatDataModel();
            this.vn.ForEach(i => fdm.vn.Add(i));//复制vn
            DataUnit u;
            foreach (DataUnit item in this)
            {
                u = (DataUnit)item.Clone();
                u.fdm = fdm;
                fdm.units.Add(u);
            }

            return fdm;
        }

        public FlatDataModel flhz(List<string> classifynames, params FLHZ_OPERATION[] par)
        {
            FlatDataModel t_fdm = (FlatDataModel)this.Clone();
            List<List<DataUnit>> bunches = t_fdm.make_bunch(classifynames);
            foreach (FLHZ_OPERATION item in par)
            {
                //先检查fieldname是否合法
                if (!t_fdm.vn.Contains(item.fieldname))
                {
                    throw new ArgumentException(string.Format("不存在字段名{0}", item.fieldname));

                }

                //处理newname的默认值
                if (item.newname.Length == 0) item.newname = item.fieldname;

            }

            //开始统计
            FlatDataModel rt = new FlatDataModel();

            foreach (List<DataUnit> thisbunch in bunches)
            {
                //先写入classfynames
                DataUnit du = new DataUnit(rt);
                foreach (string n in classifynames)
                {
                    du.data.Add(n, thisbunch[0].data[n]);
                }


                //再写入统计数据
                foreach (FLHZ_OPERATION op in par)
                {
                    //把List<DataUnit>中fieldname提取出来
                    List<object> lo = new List<object>();
                    foreach (DataUnit u in thisbunch)
                    {
                        lo.Add(u.data[op.fieldname]);
                    }
                    //统计
                    du.data.Add(op.newname, op.func(lo));
                }
                rt.units.Add(du);
            }

            //整理vn
            foreach (string item in classifynames)
            {
                rt.vn.Add(item);
            }
            foreach (FLHZ_OPERATION item in par)
            {
                rt.vn.Add(item.newname);
            }
            return rt;
        }

        /// <summary>
        /// 提取一列数据出来
        /// </summary>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public List<object> get_col(string fieldname)
        {
            //先检查fieldname是否合法
            if (!this.vn.Contains(fieldname))
            {
                throw new ArgumentException(string.Format("不存在字段名{0}", fieldname));

            }

            List<object> rt = new List<object>();
            foreach (DataUnit item in this)
            {
                rt.Add(item.data[fieldname]);
            }
            return rt;
        }


        /// <summary>
        /// 从其他模型中添加字段
        /// </summary>
        /// <param name="link_field">连接字段</param>
        /// <param name="add_fields">添加字段</param>
        /// <param name="other_model">其他模型</param>
        /// <param name="default_value">默认值，在找不到对应dataunit的时候使用</param>
        public void add_field_from_other_model(string link_field, string[] add_fields, FlatDataModel other_model, object default_value)
        {
            //检查字段名的合法性
            if (!this.vn.Contains(link_field) || !other_model.vn.Contains(link_field))
            {
                throw new ArgumentException(string.Format("字段名{0}不存在。", link_field));
            }
            foreach (var item in add_fields)
            {
                if (!other_model.vn.Contains(item))
                {
                    throw new ArgumentException(string.Format("字段名{0}不存在。", item));
                }
                if (this.vn.Contains(item))
                {
                    throw new ArgumentException(string.Format("字段名{0}已经存在。", item));
                }
            }

            //开始连接
            foreach (DataUnit u in this)
            {
                var umatch = other_model.find_one(delegate (DataUnit tu)
                {
                    dynamic d1 = tu.data[link_field];
                    dynamic d2 = u.data[link_field];
                    return d1 == d2;
                });
                if (null == umatch)
                {
                    //没找到
                    foreach (string item in add_fields)
                    {
                        u.data.Add(item, default_value);
                    }
                }
                else//找到
                {
                    foreach (string item in add_fields)
                    {
                        u.data.Add(item, umatch.data[item]);
                    }
                }
            }

            //更新vn
            foreach (string item in add_fields)
            {
                this.vn.Add(item);
            }
        }
    }

    /// <summary>
    /// 用于flhz的参数
    /// </summary>
    public class FLHZ_OPERATION
    {
        public string fieldname = "";//原字段名 也是被统计的字段
        public string newname = "";//新 字段名 空时=原字段名
        public Func<List<object>, object> func;//统计函数


    }


    public class UnamedClass
    {





        /// <summary>
        /// 对一维数组中的数进行分类
        /// </summary>
        /// <param name="ary"></param>
        /// <param name="groups"></param>
        /// <param name="groups_id"></param>
        /// <param name="span_of_isolate">分类的距离 可取1.0/类别数,此值越小类别越多</param>
        /// <param name="num_of_cells">划分cell的数量，可取类别数*10,此值影响分类的准确性，小到一定值就行</param>
        /// <returns></returns>
        public static bool classify(List<double> ary,
            out List<List<double>> groups,
            out List<List<int>> groups_id,
            double span_of_isolate = 0.3,
            int num_of_cells = 100)
        {
            groups = new List<List<double>>();
            groups_id = new List<List<int>>();
            if (ary.Count == 0)
            {
                return false;
            }
            //归一化ary
            List<object> al = new List<object>();
            foreach (object item in ary)
            {
                al.Add(item);
            }
            double minv = (double)MyStatistic.min(al);
            double maxv = (double)MyStatistic.max(al);
            List<double> ary1 = new List<double>();//归一化后的数组
            foreach (double item in ary)
            {
                ary1.Add((item - minv) / (maxv - minv));
            }

            //计算参数
            int Nc = num_of_cells;
            double Wc = 1.0 / Convert.ToDouble(Nc);
            int[] pinshu = new int[Nc];//频数数组
            for (int i = 0; i < Nc; i++)
            {
                pinshu[i] = 0;
            }

            //计算频数数组
            foreach (double item in ary1)
            {
                for (int i = 0; i < Nc; i++)
                {
                    if (item <= (i + 1) * Wc)
                    {
                        pinshu[i] += 1;
                        break;
                    }
                }
            }

            //计算分隔值
            int Siso = (int)(span_of_isolate / Wc);
            int last_zero = 0;
            List<double> isolate_values = new List<double>();//存放分隔值
            for (int i = 0; i < Nc; i++)
            {
                if (pinshu[i] > 0)
                {
                    if (i - last_zero > Siso)
                    {
                        //是分隔值
                        isolate_values.Add((last_zero + 1) * Wc);
                        last_zero = i + 1;
                    }
                    else
                    {
                        last_zero = i + 1;
                    }
                }
            }

            //利用计算得出的分隔值 分组

            for (int i = 0; i < isolate_values.Count + 1; i++)
            {
                //groups的长度=分隔值+1
                groups.Add(new List<double>());
                groups_id.Add(new List<int>());
            }
            for (int i = 0; i < ary1.Count; i++)
            {
                double value = ary1[i];
                for (int j = 0; j < isolate_values.Count; j++)
                {
                    if (value < isolate_values[j])
                    {
                        groups[j].Add(ary[i]);
                        groups_id[j].Add(i);
                        break;
                    }
                    else
                    {
                        if (j == isolate_values.Count - 1)
                        {
                            //由于分隔值始终会小于最大的value 这时候放到第count+1
                            groups[isolate_values.Count].Add(ary[i]);
                            groups_id[isolate_values.Count].Add(i);
                        }
                    }
                }
            }
            return true;
        }
    }



}
