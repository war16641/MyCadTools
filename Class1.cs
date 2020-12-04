﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace MyCadTools
{



    public class  Class1
    {

        private class MyBunch
        {
            //public MyGeometrics.Vector3D minpoint;
            //public MyGeometrics.Vector3D maxpoint;
            public Point3d minpoint;
            public Point3d maxpoint;
            public List<DBObject> list = new List<DBObject>();
            public MyBunch()
            {

            }
        }

        /// <summary>
        /// 计算两点斜率
        /// </summary>
        [CommandMethod("CalcSlope")] // 添加命令标识符​
        public void CalcSlope()
        {
            try
            {
                Point3d p1 = my_get_point("请指定第一个点：");
                Point3d p2 = my_get_point("请指定第二个点：", p1);

                Vector3d p = p2 - p1;

                // 声明命令行对象
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                if (Math.Abs(p.X) < 1e-6)
                {
                    ed.WriteMessage("坡率为无穷大");
                }
                else
                {
                    ed.WriteMessage(string.Format("斜率1：{0:f2}", p.X / p.Y));
                }
            }
            catch (MyGeometrics.MyException e)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage(e.Message);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [CommandMethod("DrawSlope")] // 添加命令标识符​

        public void DrawSlope()
        {
            try
            {
                double slope = my_get_double("请输入一个数");
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage(string.Format("数：{0:f4}", slope));
                Point3d sp = my_get_point("拾取起点");
                Point3d ep = my_get_point("拾取大致方向点", sp);
                Vector3d v = ep - sp;
                MyGeometrics.Vector3D v1 = new MyGeometrics.Vector3D(v.X, v.Y);
                double angle = v1.calc_angle_in_xoy();
                Point3d ep1;
                if (angle >= 0 && angle < 0.5 * Math.PI)
                {
                    //第一象限
                    ep1 = new Point3d(sp.X + slope, sp.Y + 1.0, 0.0);
                }
                else if (angle >= 0.5 * Math.PI)
                {
                    //第二
                    ep1 = new Point3d(sp.X - slope, sp.Y + 1.0, 0.0);
                }
                else if (angle < 0 && angle >= -0.5 * Math.PI)
                {
                    //第四
                    ep1 = new Point3d(sp.X + slope, sp.Y - 1.0, 0.0);
                }
                else
                {
                    //第三
                    ep1 = new Point3d(sp.X - slope, sp.Y - 1.0, 0.0);
                }
                //绘制射线
                Ray ray = new Ray();
                ray.BasePoint = sp;
                ray.SecondPoint = ep1;
                Database db = HostApplicationServices.WorkingDatabase;
                db.AddEntityToModelSpace(ray);

            }
            catch (System.Exception)
            {

                throw;
            }

        }




        /// <summary>
        /// 
        /// </summary>
        [CommandMethod("Rearrange")] // 添加命令标识符​

        public void Rearrange()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //// 只选择窗口中的圆形
            //TypedValue[] values = new TypedValue[]
            //{
            //    new TypedValue((int)DxfCode.Start,"")
            //};

            //SelectionFilter filter = new SelectionFilter(values);// 过滤器
            //PromptSelectionResult psr = ed.GetSelection();
            //SelectionSet SS = psr.Value;

            //List<Entity> al = new List<Entity>();

            //using (Transaction trans = db.TransactionManager.StartTransaction())
            //{
            //    foreach (CrossingOrWindowSelectedObject item in SS)
            //    {
            //        Entity ent = (Entity)trans.GetObject(item.ObjectId, OpenMode.ForRead);
            //        al.Add(ent);
            //        //ed.WriteMessage("{0}->{1}", ent.Bounds.Value.MinPoint.ToString(), ent.Bounds.Value.MaxPoint.ToString());

            //    }
            //}


            List<DBObject> al = my_select_objects();
            double group_gap = my_get_double("输入分组距离");
            double new_group_gap = my_get_double("输入新的分组距离");
            //排序
            al.Sort(delegate (DBObject ent1, DBObject ent2)
            {
                if (ent1.Bounds.Value.MinPoint.X > ent2.Bounds.Value.MinPoint.X)
                {
                    return 1;
                }
                return -1;
            });

            //开始分组
            //double group_gap = 10;
            MyBunch mb = new MyBunch();
            List<MyBunch> list_mb = new List<MyBunch>();
            foreach (Entity item in al)
            {
                if (mb.list.Count == 0)
                {
                    //当为空数组时 添加item
                    mb.list.Add(item);
                    mb.maxpoint = item.Bounds.Value.MaxPoint;
                    mb.minpoint = item.Bounds.Value.MinPoint;
                }
                else//根据当前item的范围加入还是新开mybunch
                {
                    if (item.Bounds.Value.MinPoint.X <= mb.maxpoint.X + group_gap)//同组
                    {
                        mb.list.Add(item);
                        if (item.Bounds.Value.MaxPoint.X > mb.maxpoint.X) mb.maxpoint = item.Bounds.Value.MaxPoint;//更新maxpoint 如果有需要

                    }
                    else//超过了间距 新建mybunch
                    {
                        list_mb.Add(mb);
                        mb = new MyBunch();
                        mb.list.Add(item);
                        mb.maxpoint = item.Bounds.Value.MaxPoint;
                        mb.minpoint = item.Bounds.Value.MinPoint;
                    }
                }
            }
            list_mb.Add(mb);
            ed.WriteMessage("找到{0:d}组", list_mb.Count);
            //开始移动
            //double new_group_gap = 2.0;
            for (int i = list_mb.Count - 1; i > 0; i--)
            {
                Point3d bp = new Point3d(list_mb[i].minpoint.X, 0, 0);
                Point3d tp = new Point3d(list_mb[i - 1].maxpoint.X + new_group_gap, 0, 0);//移动的两个参考点

                //移动当前组到最后一组
                for (int j = i; j < list_mb.Count; j++)
                {
                    MyMethods.MoveEnity(bp, tp, list_mb[j].list);
                    //foreach (Entity item in list_mb[j].list)
                    //{
                    //    MyMethods.MoveEntity(item.ObjectId, bp, tp);

                    //}
                }
            }



        }
        /// <summary>
        /// 自动根据文字长度修剪脚线
        /// 暂只支持水平的文字
        /// </summary>
        [CommandMethod("TirmFootline")] // 添加命令标识符
        public void TrimFootline()
        {
            List<DBObject> al = my_select_objects();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            List<Line> list_line = new List<Line>();//直线可以有多个
            DBText text = null;//文字对象 只能有一个

            //分类
            foreach (DBObject item in al)
            {
                if (item is DBText)
                {
                    text = (DBText)item;
                }
                else if (item is Line)
                {
                    list_line.Add((Line)item);
                }
            }

            //开始修剪
            if (text == null)
            {
                ed.WriteMessage("没有文字对象。");
                return;
            }
            MyGeometrics.Vector3D minpoint = new MyGeometrics.Vector3D(text.Bounds.Value.MinPoint.X, text.Bounds.Value.MinPoint.Y);
            MyGeometrics.Vector3D maxpoint = new MyGeometrics.Vector3D(text.Bounds.Value.MaxPoint.X, text.Bounds.Value.MaxPoint.Y);
            foreach (Line item in list_line)
            {
                MyGeometrics.Vector3D v1 = new MyGeometrics.Vector3D(item.StartPoint.X, item.StartPoint.Y);
                MyGeometrics.Vector3D v2 = new MyGeometrics.Vector3D(item.EndPoint.X, item.EndPoint.Y);
                MyGeometrics.Line3D elo = MyGeometrics.Line3D.make_line_by_2_points(v1, v2);
                MyGeometrics.Vector3D np1, np2;
                double t = minpoint.distance_to_line(elo, out np1);
                t = maxpoint.distance_to_line(elo, out np2);
                //edit_line(new Point3d(np1.x, np1.y, np1.z), new Point3d(np2.x, np2.y, np2.z), item);
                edit_line(np1.toPoint3d(), np2.toPoint3d(), item);
            }

        }


        /// <summary>
        /// 自动编号文字
        /// </summary>
        [CommandMethod("AutoNumbering")] // 添加命令标识符
        public void AutoNumbering()
        {

            List<DBObject> al = my_select_objects();
            string format = my_get_string("输入格式文字");//最多可接受两个参数
            string funcname = my_get_string("输入函数名");//最多可接受两个参数
            int offset = (int)my_get_double("输入偏移量");
            for (int i = 0; i < al.Count; i++)
            {
                DBText text = (DBText)al[i];
                edit_text(string.Format(format, ForAutoNumbering.dic[funcname](i + 1 + offset), ForAutoNumbering.dic[funcname](al.Count)), text);
            }
        }

        public static class ForAutoNumbering
        {
            public static Dictionary<string, System.Func<int, string>> dic = new Dictionary<string, Func<int, string>>();
            static ForAutoNumbering()
            {
                dic.Add("arab", arab);
                dic.Add("chinese", chinese);
            }
            static string arab(int x)
            {
                return x.ToString();
            }
            static string chinese(int x)
            {
                return MyMethods.arab_num_2_chinese_num(x);
            }
        }



        [CommandMethod("zk")]
        public void zk()
        {
            List<DBObject> al = my_select_objects();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            List<Line> list_line = new List<Line>();//线的集合
            List<DBText> list_text = new List<DBText>();//线的集合
            //分类
            foreach (DBObject item in al)
            {
                if (item is Line)
                {
                    list_line.Add((Line)item);
                }
                else if (item is DBText)
                {
                    list_text.Add((DBText)item);
                }
                else
                {
                    throw new MyGeometrics.MyException("未知类型");
                }

            }
            ed.WriteMessage(string.Format("一共选择了{0:D}个线，{1:D}个文字。\n", list_line.Count, list_text.Count));

            //开始匹配
            //以线为基准，去寻找最近文字
            List<ZuanKong> list_zk = new List<ZuanKong>();
            List<DBText> current_list_text = new List<DBText>();
            foreach (DBText item in list_text)
            {

                current_list_text.Add(item);
            }

            List<DBText> possible_list_text = new List<DBText>();

            double dist1 = 2.5;
            double dist2 = 0.5;//text参考点必须在直线内部，不能超过这个限值
            double angle1 = 5.0 / 180.0 * Math.PI;//超过这个限值，不认为线和文字是一对
            foreach (Line elo in list_line)
            {
                //生成自己的line3d对象
                MyGeometrics.Line3D myline = MyGeometrics.Line3D.make_line_by_2_points(new MyGeometrics.Vector3D(elo.StartPoint.X, elo.StartPoint.Y),
                                                                                     new MyGeometrics.Vector3D(elo.EndPoint.X, elo.EndPoint.Y));
                MyGeometrics.TransforamtionFunction tf = new MyGeometrics.TransforamtionFunction(myline.basepoint, myline.angle);
                possible_list_text.Clear();
                foreach (DBText item in current_list_text)
                {
                    //计算text到直线的距离，由于text 与 line的关系 可能不是水平 或者 线在文字上方 导致计算出来的值比表观大，因而dist1的值不能取太小，建议至少是文字高度
                    Point3d text_point = item.Position;//文字参考点
                    double d = item.Position.toVector3D().distance_to_line(myline);
                    if (d > dist1)//点到直线距离
                    {
                        continue;
                    }
                    double t = MyGeometrics.Vector3D.equivalent_angle(item.Rotation - myline.angle);
                    if (!(-angle1 < t && t < angle1))//文字的旋转角和直线方向
                    {
                        continue;
                    }

                    MyGeometrics.Vector3D v;
                    v = tf.trans(text_point.toVector3D());
                    if (v.x < -dist2 || v.x > elo.Length + dist2)//文字必须在直线内部
                    {
                        continue;
                    }
                    possible_list_text.Add(item);


                }
                //开始处理possible_list_text
                if (possible_list_text.Count > 2)//多余2个，zuankong只能有两个文字 这里通过距离排序得出最近的两个
                {
                    throw new MyGeometrics.MyException("暂未实现这个功能");
                }

                if (possible_list_text.Count == 1)//1个代表 编号
                {
                    ZuanKong zk = new ZuanKong();
                    zk.geshixian = elo;
                    zk.bianhao = possible_list_text[0];
                    list_zk.Add(zk);
                }
                else if (possible_list_text.Count == 2)//编号+深度
                {
                    ZuanKong zk = new ZuanKong();
                    zk.geshixian = elo;
                    //区分钻孔编号和深度 算法：看能不能转化为double
                    try
                    {
                        double b = Convert.ToDouble(possible_list_text[0].TextString);
                        //可以转换
                        zk.shengdu = possible_list_text[0];
                        zk.bianhao = possible_list_text[1];
                    }
                    catch (System.FormatException)
                    {
                        //不可以转换
                        zk.bianhao = possible_list_text[0];
                        zk.shengdu = possible_list_text[1];

                    }

                    list_zk.Add(zk);
                }
                else if (possible_list_text.Count == 0)//孤独的直线
                {
                    ed.WriteMessage("发现一条未配对的直线\n");
                }
                foreach (DBText item in possible_list_text)
                {
                    current_list_text.Remove(item);
                }

            }
            ed.WriteMessage(string.Format("成功生成{0:D}个钻孔。\n", list_zk.Count));

            //后续操作
            foreach (ZuanKong item in list_zk)
            {
                item.rotate();
                item.trim_line();
                item.adjust_position_of_shengdu();

            }
        }


        /// <summary>
        /// 代表钻孔
        /// </summary>
        public class ZuanKong
        {
            public Line geshixian = null;
            public DBText bianhao = null;
            public DBText shengdu = null;
            public ZuanKong()
            {


            }

            /// <summary>
            /// 修剪geshixain
            /// </summary>
            public void trim_line()
            {
                MyGeometrics.Line3D elo = this.geshixian.toLine3D();
                MyGeometrics.Vector3D b = this.bianhao.Bounds.Value.MaxPoint.toVector3D();
                //MyGeometrics.Vector3D s = this.shengdu.Bounds.Value.MaxPoint.toVector3D();
                //MyGeometrics.TransforamtionFunction tf = new MyGeometrics.TransforamtionFunction(this.geshixian.StartPoint.toVector3D(), elo.angle);
                //double b1 = tf.trans(b).x;
                //double s1 = tf.trans(s).x;
                MyGeometrics.Vector3D t;
                b.distance_to_line(elo, out t);
                edit_line(this.geshixian.StartPoint, t.toPoint3d(), this.geshixian);

            }

            /// <summary>
            /// 绕格式线起点旋转至angle的角度
            /// </summary>
            /// <param name="angle"></param>
            public void rotate(double angle = 0.0)
            {
                if (this.shengdu == null)
                {
                    MyMethods.RotateEntity(this.geshixian.StartPoint, angle - this.geshixian.toLine3D().angle, this.geshixian, this.bianhao);
                }
                else
                {
                    MyMethods.RotateEntity(this.geshixian.StartPoint, angle - this.geshixian.toLine3D().angle, this.geshixian, this.bianhao, this.shengdu);
                }

            }


            public void adjust_position_of_shengdu()
            {
                if (null==this.shengdu)
                {
                    return;
                }
                //计算shengdu位置
                Point3d mi = this.shengdu.Bounds.Value.MinPoint;
                Point3d ma = this.shengdu.Bounds.Value.MaxPoint;
                MyGeometrics.Vector3D c = (mi.toVector3D() + ma.toVector3D()) * 0.5;//中心点
                MyGeometrics.Line3D elo = this.geshixian.toLine3D();
                MyGeometrics.Vector3D rc = new MyGeometrics.Vector3D((this.geshixian.StartPoint.X + this.geshixian.EndPoint.X) * 0.5,
                    (this.geshixian.StartPoint.Y + this.geshixian.EndPoint.Y) * 0.5);
                MyGeometrics.Vector3D diff = c - rc;//差的的这个向量
                MyGeometrics.Vector3D adjust = diff.projection_on_line(elo.direction);//投影到这个格式线上
                DBObject t = this.shengdu;
                MyMethods.MoveEntity(this.shengdu.ObjectId,new Point3d(0, 0, 0), (-adjust).toPoint3d());
                //MyMethods.MoveEnity(new Point3d(0, 0, 0), (-adjust).toPoint3d(),)

            }

        }





        [CommandMethod("mytest")]
        public void mytest()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            List<DBObject> al = my_select_objects();


            Database db = HostApplicationServices.WorkingDatabase;
            //新建图层
            db.AddLayer("DZ");
            //处理这些线
            foreach (DBObject item in al)
            {
                if (MyMethods.IsLocked((Entity)item))
                {
                    continue;
                }
                if (item is Polyline)//处理多段线 炸了
                {
                    Entity e1 = (Entity)item;
                    DBObjectCollection co = new DBObjectCollection();
                    e1.Explode(co);
                    foreach (DBObject item1 in co)
                    {
                        Entity T1 = (Entity)item1;
                        
                        T1.Layer = "DZ";
                        T1.Linetype = "ByLayer";
                        db.AddEntityToModelSpace(T1);
                    }
                    MyMethods.DeleteEntity(e1);//
                    continue;
                }
                else
                {
                    //其余的类型 只改动图层
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        Entity ent = (Entity)trans.GetObject(item.ObjectId, OpenMode.ForWrite);
                        ent.Layer = "DZ";
                        trans.Commit();
                    }

                }

            }

            //筛选 按图层

            //操作


            //移动这些
            //MyMethods.MoveEnity(elo.StartPoint, elo.EndPoint, al);


        }


        /// <summary>
        /// 修改既有线的两个端点
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="ep"></param>
        /// <param name="line"></param>
        private static void edit_line(Point3d sp, Point3d ep, Line line)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Line ent = (Line)trans.GetObject(line.ObjectId, OpenMode.ForWrite);
                ent.StartPoint = sp; ent.EndPoint = ep;
                trans.Commit();
            }
        }


        /// <summary>
        /// 修改文字
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text"></param>
        private static void edit_text(string s, DBText text)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBText ent = (DBText)trans.GetObject(text.ObjectId, OpenMode.ForWrite);
                ent.TextString = s;
                trans.Commit();
            }
        }

        private PromptPointResult GetPoint(PromptPointOptions ppo)
        {

            ppo.AllowNone = true;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return ed.GetPoint(ppo);

        }
        private PromptDoubleResult GetDouble(PromptDoubleOptions ppo)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return ed.GetDouble(ppo);
        }

        private double my_get_double(string prompt)
        {
            PromptDoubleOptions pdo = new PromptDoubleOptions(prompt);
            pdo.AllowNone = false;
            PromptDoubleResult pdr = GetDouble(pdo);
            if (pdr.Status == PromptStatus.Cancel) throw new MyGeometrics.MyException("用户取消");
            if (pdr.Status == PromptStatus.OK) return pdr.Value;
            throw new MyGeometrics.MyException("未知错误");
        }
        private Point3d my_get_point(string prompt)
        {
            PromptPointOptions ppo = new PromptPointOptions(prompt);
            ppo.AllowNone = false;
            PromptPointResult ppr = GetPoint(ppo);
            if (ppr.Status == PromptStatus.Cancel) throw new MyGeometrics.MyException("用户取消");
            if (ppr.Status == PromptStatus.OK) return ppr.Value;
            throw new MyGeometrics.MyException("未知错误");
        }
        private Point3d my_get_point(string prompt, Point3d bp)
        {
            PromptPointOptions ppo = new PromptPointOptions(prompt);
            ppo.AllowNone = false;
            ppo.BasePoint = bp;
            ppo.UseBasePoint = true;
            PromptPointResult ppr = GetPoint(ppo);
            if (ppr.Status == PromptStatus.Cancel) throw new MyGeometrics.MyException("用户取消");
            if (ppr.Status == PromptStatus.OK) return ppr.Value;
            throw new MyGeometrics.MyException("未知错误");
        }

        private string my_get_string(string prompt)
        {
            PromptStringOptions pso = new PromptStringOptions(prompt);
            pso.AllowSpaces = true;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptResult pr = ed.GetString(pso);
            if (pr.Status == PromptStatus.Cancel) throw new MyGeometrics.MyException("用户取消");
            if (pr.Status == PromptStatus.OK) return pr.StringResult;
            throw new MyGeometrics.MyException("用户取消");
        }
        /// <summary>
        /// 获取用户拾取的对象
        /// </summary>
        /// <returns></returns>
        private List<DBObject> my_select_objects()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // 只选择窗口中的圆形
            TypedValue[] values = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,"")
            };

            SelectionFilter filter = new SelectionFilter(values);// 过滤器
            PromptSelectionResult psr = ed.GetSelection();//参数为空 代表无筛选
            SelectionSet SS = psr.Value;

            List<DBObject> al = new List<DBObject>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //foreach (CrossingOrWindowSelectedObject item in SS)
                foreach (SelectedObject item in SS)
                {
                    DBObject ent = trans.GetObject(item.ObjectId, OpenMode.ForRead);
                    al.Add(ent);
                    //ed.WriteMessage("{0}->{1}", ent.Bounds.Value.MinPoint.ToString(), ent.Bounds.Value.MaxPoint.ToString());

                }
            }
            return al;
        }
    }
    public static partial class AddEntityTools
    {
        public static ObjectId[] AddEntityToModelSpace(this Database db, params Entity[] ent)
        {
            ObjectId[] entId = new ObjectId[ent.Length];
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                //打开表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //加入记录
                for (int i = 0; i < ent.Length; i++)
                {
                    entId[i] = btr.AppendEntity(ent[i]);
                    //更新记录
                    trans.AddNewlyCreatedDBObject(ent[i], true);
                    //提交
                }

                trans.Commit();
            }
            return entId;
        }

        
        public static bool AddLayer(this Database db,string layerName)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            
            try
            {
                SymbolUtilityServices.ValidateSymbolName(layerName, false);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                ed.WriteMessage("非法图层名\n");
                return false;
            }


            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开层表
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                //新建层表记录
                if (!lt.Has(layerName))
                {

                    LayerTableRecord ltr = new LayerTableRecord();
                    //判断要创建的图层名是否已经存在,不存在则创建
                    ltr.Name = layerName;
                    //升级层表打开权限
                    lt.UpgradeOpen();
                    lt.Add(ltr);
                    //降低层表打开权限
                    lt.DowngradeOpen();
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    trans.Commit();
                    return true;
                }
                else
                {
                    ed.WriteMessage("图层名已存在。\n");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从vector3d转换到同样坐标值的point3d
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Point3d toPoint3d(this MyGeometrics.Vector3D v)
        {
            return new Point3d(v.x, v.y, v.z);
        }
        public static MyGeometrics.Vector3D toVector3D(this Point3d p)
        {
            return new MyGeometrics.Vector3D(p.X, p.Y, p.Z);
        }
        public static MyGeometrics.Line3D toLine3D(this Line elo)
        {
            return MyGeometrics.Line3D.make_line_by_2_points(elo.StartPoint.toVector3D(), elo.EndPoint.toVector3D());
        }
    }


    public static class MyMethods
    {
        /// <summary>
        /// 移动entity
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="sourcePoint"></param>
        /// <param name="targetPoint"></param>
        public  static void MoveEntity(ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                // 计算变换矩阵
                Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vectoc);
                ent.TransformBy(mt);
                // 提交事务处理
                trans.Commit();
            }
        }



        
        public static void MoveEntity(Point3d sourcePoint, Point3d targetPoint, params DBObject[] obs)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                foreach (DBObject item in obs)
                {
                    // 打开图形
                    Entity ent = (Entity)item.ObjectId.GetObject(OpenMode.ForWrite);
                    // 计算变换矩阵
                    Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                    Matrix3d mt = Matrix3d.Displacement(vectoc);
                    ent.TransformBy(mt);
                }

                // 提交事务处理
                trans.Commit();
            }
        }

        public static void MoveEntity(Point3d sourcePoint, Point3d targetPoint, DBObject obs)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);

                // 打开图形
                Entity ent = (Entity)obs.ObjectId.GetObject(OpenMode.ForWrite);
                // 计算变换矩阵
                Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vectoc);
                ent.TransformBy(mt);


                // 提交事务处理
                trans.Commit();
            }
        }

        /// <summary>
        /// 批量
        /// </summary>
        /// <param name="sourcePoint"></param>
        /// <param name="targetPoint"></param>
        /// <param name="al"></param>
        public static void MoveEnity(Point3d sourcePoint, Point3d targetPoint, List<DBObject> al)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                foreach (Entity item in al)
                {
                    Entity ent = (Entity)item.ObjectId.GetObject(OpenMode.ForWrite);
                    // 计算变换矩阵
                    Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                    Matrix3d mt = Matrix3d.Displacement(vectoc);
                    ent.TransformBy(mt);
                }

                // 提交事务处理
                trans.Commit();
            }
        }

        public static void RotateEntity(Point3d basePoint, double angle, params DBObject[] obs)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                foreach (DBObject item in obs)
                {
                    Entity ent = (Entity)item.ObjectId.GetObject(OpenMode.ForWrite);
                    ent.TransformBy(Matrix3d.Rotation(angle, new Vector3d(0, 0, 1), basePoint));
                }

                // 提交事务处理
                trans.Commit();
            }
        }

        public static void DeleteEntity(params Entity[] ents)
        {
            using (Database db = HostApplicationServices.WorkingDatabase)
            {

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    foreach (Entity item in ents)
                    {
                        Entity entity = (Entity)trans.GetObject(item.ObjectId, OpenMode.ForWrite, true);

                        entity.Erase(true);

                        trans.Commit();
                    }

                }
            }
        }


        /// <summary>
        /// 返回这个对象的图层是否被锁定
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static bool IsLocked(Entity ent)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开层表
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                LayerTableRecord ltr = (LayerTableRecord)lt[ent.Layer].GetObject(OpenMode.ForRead);
                return ltr.IsLocked;
            }

        }

        public static Dictionary<char, string> dic_arab_num_2_chinese_num = new Dictionary<char, string>();

        static MyMethods()
        {
            //dic_arab_num_2_chinese_num.Add(0, "零");
            //dic_arab_num_2_chinese_num.Add(1, "一");
            //dic_arab_num_2_chinese_num.Add(2, "二");
            //dic_arab_num_2_chinese_num.Add(3, "三");
            //dic_arab_num_2_chinese_num.Add(4, "四");
            //dic_arab_num_2_chinese_num.Add(5, "五");
            //dic_arab_num_2_chinese_num.Add(6, "六");
            //dic_arab_num_2_chinese_num.Add(7, "七");
            //dic_arab_num_2_chinese_num.Add(8, "八");
            //dic_arab_num_2_chinese_num.Add(9, "九");
            dic_arab_num_2_chinese_num.Add('0', "零");
            dic_arab_num_2_chinese_num.Add('1', "一");
            dic_arab_num_2_chinese_num.Add('2', "二");
            dic_arab_num_2_chinese_num.Add('3', "三");
            dic_arab_num_2_chinese_num.Add('4', "四");
            dic_arab_num_2_chinese_num.Add('5', "五");
            dic_arab_num_2_chinese_num.Add('6', "六");
            dic_arab_num_2_chinese_num.Add('7', "七");
            dic_arab_num_2_chinese_num.Add('8', "八");
            dic_arab_num_2_chinese_num.Add('9', "九");
        }
        /// <summary>
        /// 将数字转换为中文汉字
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string arab_num_2_chinese_num(int x)
        {
            string xx = x.ToString();
            string r = "";
            foreach (char item in xx)
            {
                r += dic_arab_num_2_chinese_num[item];
            }
            return r;
        }


    }

}