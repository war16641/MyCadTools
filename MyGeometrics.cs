using System;
using System.Collections.Generic;

namespace MyGeometrics
{

    /// <summary>
    /// 自定义一个异常
    /// </summary>
    public class MyException : System.SystemException
    {
        public MyException()
        {

        }
        public MyException(string e) : base(e)
        {

        }
    }

    public interface Imygeometrics
    {
        bool contain(Vector3D a, double b);
        bool contain(Vector3D a, double b, out double c);
        Vector3D calc_nearest_point(Vector3D a, out bool b, out double lc, double d);
        double length { get; }
        Vector3D center { get; }//中心点

    }

    public class Vector3D
    {
        public double x;
        public double y;
        public double z;


        public static double equal_tolerance = 1e-5;//用于判断相等的误差
        public Vector3D()
        {
            x = 0.0; y = 0.0; z = 0.0;
        }
        //public Vector3D(double x1,double y1,double z1)
        //{
        //    x = x1;
        //    y = y1;
        //    z = z1;
        //}
        public Vector3D(params double[] xyz)
        {
            if (2 == xyz.Length)
            {
                x = xyz[0];
                y = xyz[1];
                z = 0.0;
            }
            else if (3 == xyz.Length)
            {
                x = xyz[0];
                y = xyz[1];
                z = xyz[2];
            }
            else if (1 == xyz.Length)
            {
                x = xyz[0];
                y = 0.0;
                z = 0.0;
            }
            else
            {
                throw new SystemException("数组过长");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3D)
            {
                return this == (Vector3D)obj;
            }
            return base.Equals(obj);
        }
        public override string ToString()
        {
            return string.Format("{0:f4},{1:f4},{2:f4}", x, y, z);
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static Vector3D operator -(Vector3D v1)
        {
            return new Vector3D(-v1.x, -v1.y, -v1.z);
        }

        public static Vector3D operator *(Vector3D v1, double scale)
        {
            return new Vector3D(v1.x * scale, v1.y * scale, v1.z * scale);
        }
        public static double operator *(Vector3D v1, Vector3D v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public Vector3D get_copy()//获得一个复制
        {
            return new Vector3D(x, y, z);
        }

        public double norm
        {
            get
            {
                return Math.Sqrt(x * x + y * y + z * z);
            }

            set
            {//设置新的模
                if (value < 0.0)
                {
                    throw new SystemException("模不能为负");
                }
                else
                {
                    double t = value / norm;
                    x = x * t;
                    y = t * y;
                    z = t * z;
                }
            }
        }

        public static bool operator ==(Vector3D v1, Vector3D v2)
        {
            Vector3D t = v1 - v2;
            if (t.norm <= equal_tolerance)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Vector3D v1, Vector3D v2)
        {
            return !(v1 == v2);
        }


        /// <summary>
        /// //判断两个向量是否平行
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool is_parallel(Vector3D other)
        {

            Vector3D t1 = get_copy();
            Vector3D t2 = other.get_copy();
            t1.norm = 1; t2.norm = 1;
            return t1 == t2;
        }

        /// <summary>
        /// 判断两个向量是否垂直
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool is_perpendicular(Vector3D other)
        {
            return (this * other) < equal_tolerance;
        }


        /// <summary>
        ///向量叉积
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Vector3D cross_product(Vector3D other)
        {
            return new Vector3D(this.y * other.z - this.z * other.y,
                this.z * other.x - this.x * other.z,
                this.x * other.y - this.y * other.x);
        }

        /// <summary>
        /// 点到直线的距离
        /// </summary>
        /// <param name="elo"></param>
        /// <returns></returns>
        public double distance_to_line(Line3D elo)
        {
            Vector3D y = this - elo.basepoint;
            Vector3D d = elo.direction.get_copy();
            d.norm = 1;//获取单位长度的直线
            double s = y * d;
            d = d * s;
            Vector3D x = y - d;
            return x.norm;
        }

        public double distance_to_line(Line3D elo, out Vector3D nearest_point)
        {
            Vector3D y = this - elo.basepoint;
            Vector3D d = elo.direction.get_copy();
            d.norm = 1;//获取单位长度的直线
            double s = y * d;
            d = d * s;
            nearest_point = elo.basepoint + d;
            Vector3D x = y - d;
            return x.norm;
        }

        public void test(Vector3D v)
        {
            v.x = -1;
        }


        /// <summary>
        /// 计算x,y组成的向量在xoy平面内的夹角，x轴转向y轴为正 弧度制
        /// </summary>
        /// <returns>-pi,pi</returns>
        public double calc_angle_in_xoy()
        {
            if (x == 0)
            {
                if (y == 0)
                {
                    return 0.0; //向量为零向量 返回任意值都没问题，这里返回0
                }
                else if (y > 0)
                {
                    return Math.PI / 2;
                }
                else
                {
                    return -Math.PI / 2;
                }
            }

            double t = Math.Atan(y / x);
            if (x < 0)
            {
                if (y > 0)
                {
                    return t + Math.PI;
                }
                else
                {
                    return t - Math.PI;
                }
            }
            return t;
        }
        /// <summary>
        /// 判断两个角度是否相等
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="tol"></param>
        /// <returns></returns>

        public static bool is_equal_angle(double a1, double a2, double tol = 1e-5)
        {
            if (a1 < 0)
            {
                double t = 1 + Math.Floor(-a1 / (2 * Math.PI));
                a1 += t * 2 * Math.PI;
            }
            if (a2 < 0)
            {
                double t = 1 + Math.Floor(-a2 / (2 * Math.PI));
                a2 += t * 2 * Math.PI;
            }
            double y1 = a1 % (2 * Math.PI);
            double y2 = a2 % (2 * Math.PI);
            if (Math.Abs(y1 - y2) < tol)
            {
                return true;
            }
            return false;

        }


        /// <summary>
        /// 将一个角度转移到（-PI,PI]的区间上
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double equivalent_angle(double angle)
        {
            if (angle < 0)
            {
                double t = 1 + Math.Floor(-angle / (2 * Math.PI));
                angle += t * 2 * Math.PI;
            }
            double f = angle % (2 * Math.PI);
            if (f <= Math.PI)
            {
                return f;
            }
            else
            {
                return f - 2 * Math.PI;
            }

        }

        /// <summary>
        /// 将一个角度转移到[0,2PI)的区间上
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double equivalent_angle1(double angle)
        {
            double t = Vector3D.equivalent_angle(angle);
            if (t < 0)
            {
                return t + 2 * Math.PI;
            }
            return t;

        }


        /// <summary>
        /// 获取向量在另外一个向量上的投影
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3D projection_on_line(Vector3D v)
        {
            Vector3D fx = v.get_copy();
            fx.norm = 1;//获取单位向量
            return fx * (fx * this);
        }
    }

    public class Line3D
    {
        public Vector3D basepoint;
        public Vector3D direction;
        public Line3D()
        {
            this.basepoint = new Vector3D();
            this.direction = new Vector3D();
        }
        public Line3D(Vector3D bp, Vector3D dir)
        {
            this.basepoint = bp.get_copy();
            this.direction = dir.get_copy();
        }
        /// <summary>
        /// 从两个点生成线
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Line3D make_line_by_2_points(Vector3D v1, Vector3D v2)
        {
            return new Line3D(v1, v2 - v1);
        }


        /// <summary>
        /// 返回线在平面内的方向角
        /// 
        /// </summary>
        public double angle
        {
            get
            {
                return this.direction.calc_angle_in_xoy();
            }
        }
    }


    /// <summary>
    /// 线段
    /// </summary>
    public class LineSegment : Line3D, Imygeometrics
    {
        public Vector3D p1;
        public Vector3D p2;
        public TransforamtionFunction tf = null;


        public Vector3D center
        {
            get
            {
                return (p1 + p2) * 0.5;
            }
        }
        public double length//线段长度
        {
            get
            {
                return (p2 - p1).norm;
            }
        }
        public LineSegment() { }


        public LineSegment(Vector3D v1, Vector3D v2)
        {
            this.basepoint = v1;
            this.direction = v2 - v1;
            this.p1 = v1;
            this.p2 = v2;
            //设置tf
            this.tf = new TransforamtionFunction(v1, this.angle);
        }
        /// <summary>
        /// 使用两个点创建线段
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static LineSegment make_lineseg_by_2_points(Vector3D v1, Vector3D v2)
        {
            Line3D elo = new Line3D(v1, v2 - v1);
            LineSegment rt = new LineSegment();
            rt.basepoint = elo.basepoint;
            rt.direction = elo.direction;
            rt.p1 = v1.get_copy();
            rt.p2 = v2.get_copy();
            //设置tf
            rt.tf = new TransforamtionFunction(v1, elo.angle);
            return rt;
        }


        /// <summary>
        /// 判断点是否在线段上
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol = 1e-6)
        {
            //看是不是在直线上
            double dist1 = v.distance_to_line(this);
            if (dist1 > tol) return false;
            //看点是否在线段内
            Vector3D v1 = this.tf.trans(v);
            if (v1.x > -tol && v1.x < this.length + tol)//允许有个小误差
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 计算v在线段上的长度坐标
        /// 长度坐标：标识v在线段上的位置
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <param name="lc"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol, out double lc)
        {
            bool b = this.contain(v, tol);
            lc = -1;
            if (!b)
            {
                return b;
            }
            //在直线上 计算长度坐标
            lc = (v - this.p1).norm;
            return b;
        }

        public Vector3D calc_nearest_point(Vector3D v, out bool flag_in, out double lc, double tol = 1e-6)
        {
            Vector3D rt;
            v.distance_to_line(this, out rt);
            lc = (this.p1 - rt).norm;
            flag_in = this.contain(rt, tol);
            return rt;
        }

    }


    public class MyArc : Imygeometrics
    {
        public Vector3D _center;
        public double radius;
        public double theta1;
        public double theta2;
        public double normalz;//轴的z向量 1正向 -1逆向
        public Vector3D center
        {
            get
            {
                return this._center;
            }
        }
        public MyArc(Vector3D center, double radius, double theta1, double theta2, double reversed = 1.0)
        {
            //if (theta2 <= theta1)
            //{
            //    throw new MyException("要求theta2大于theta1");
            //}
            this._center = center.get_copy();
            this.radius = radius;
            this.theta1 = theta1;
            this.theta2 = theta2;
            this.normalz = reversed;
        }


        /// <summary>
        ///圆心-端点-端点
        ///第二个端点可以不再圆上
        /// </summary>
        /// <param name="center"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public MyArc(Vector3D center, Vector3D p1, Vector3D p2, double reversed = 1.0) :
            this(center, (p1 - center).norm, (p1 - center).calc_angle_in_xoy(), (p2 - center).calc_angle_in_xoy(), reversed)
        {

        }
        /// <summary>
        /// 判断点是否在弧上
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol = 1e-6)
        {
            TransformationFunctionPolar tf = new TransformationFunctionPolar(
                this._center,
                0);
            Vector3D v1 = tf.trans(v);
            if (Math.Abs(v1.y - this.radius) > tol) return false;

            //
            this.theta1 = Vector3D.equivalent_angle1(this.theta1);
            this.theta2 = Vector3D.equivalent_angle1(this.theta2);
            v1.x = Vector3D.equivalent_angle1(v1.x);
            if (this.theta2 > this.theta1)
            {
                return (this.theta1 < v1.x + tol && v1.x - tol < this.theta2);
            }
            else
            {
                //double t1 = 0.0;
                double t2 = this.theta2 - this.theta1;
                t2 = Vector3D.equivalent_angle1(t2);
                double t = Vector3D.equivalent_angle1(v1.x - this.theta1);
                return t < t2 + tol;
            }

        }


        /// <summary>
        /// 计算v在线段上的长度坐标
        /// 长度坐标：标识v在线段上的位置
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <param name="lc"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol, out double lc)
        {
            bool b = this.contain(v, tol);
            lc = -1;
            if (!b)
            {
                return b;
            }
            //在弧上 计算长度坐标
            double theta = Vector3D.equivalent_angle1((v - this._center).calc_angle_in_xoy() - this.theta1);
            lc = theta * this.radius;
            return b;


        }

        public double length
        {
            get
            {
                return this.radius * Vector3D.equivalent_angle1(this.theta2 - this.theta1);
            }
        }

        public Vector3D calc_nearest_point(Vector3D v, out bool flag_in, out double lc, double tol = 1e-6)
        {
            TransformationFunctionPolar tf = new TransformationFunctionPolar(
                this._center,
                0);
            Vector3D zb = tf.trans(v);
            Vector3D n = new Vector3D(zb.x, this.radius);
            lc = Vector3D.equivalent_angle1(n.x - this.theta1) * this.radius;
            Vector3D rt = tf.itrans(n);
            flag_in = this.contain(rt, tol);
            return rt;

        }
    }


    public class Polyline
    {
        public List<Imygeometrics> segs = new List<Imygeometrics>();
        public int num_of_segs
        {
            get
            {
                return this.segs.Count;
            }
        }

        /// <summary>
        /// 点是否在多段线上
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol = 1e-6)
        {
            for (int i = 0; i < this.segs.Count; i++)
            {
                if (this.segs[i].contain(v, tol))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 判断点是否在多段线上
        /// </summary>
        /// <param name="v"></param>
        /// <param name="tol"></param>
        /// <param name="lc">长度坐标</param>
        /// <param name="lc1">对应seg的 长度坐标</param>
        /// <param name="id">对应seg的id</param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol, out double lc, out double lc1, out int id)
        {
            lc = -1;
            lc1 = -1;
            id = -1;
            double lcc = 0.0;
            for (int i = 0; i < this.segs.Count; i++)
            {
                if (this.segs[i].contain(v, tol, out lc1))
                {
                    id = i;
                    lc = lcc + lc1;
                    return true;
                }
                lcc += this.segs[i].length;
            }
            return false;
        }

        /// <summary>
        /// 给定点 计算该点在多段线上最近点
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="flag_in">最近点是否在多段线上</param>
        /// <param name="lc">长度坐标</param>
        /// <param name="id">最近点对应seg的id</param>
        /// <param name="tol">计算误差</param>
        /// <returns></returns>
        public Vector3D calc_nearest_point(Vector3D v, out bool flag_in, out double lc, out int id, double tol = 1e-6)
        {
            lc = 0.0;

            Imygeometrics cur;
            double llc;
            Vector3D nearest_point;
            for (int i = 0; i < this.num_of_segs; i++)
            {
                cur = this.segs[i];
                nearest_point = cur.calc_nearest_point(v, out flag_in, out llc, tol);
                if (flag_in)
                {
                    //找到了
                    lc += llc;
                    id = i;
                    return nearest_point;
                }
                else
                {
                    lc += cur.length;
                }

            }
            flag_in = false;
            lc = -1;
            id = -1;
            return new Vector3D(0, 0, 0);

        }



    }

    /// <summary>
    /// 坐标系变换
    /// 先平移 后旋转（逆时针）
    /// 只是xoy平面内的变化
    /// </summary>
    public class TransforamtionFunction
    {
        public double theta;
        public Vector3D p;
        public TransforamtionFunction(double theta)
        {
            this.p = new Vector3D(0, 0, 0);
            this.theta = theta;
        }
        public TransforamtionFunction(Vector3D v, double theta)
        {
            this.p = v.get_copy();
            this.theta = theta;
        }


        public Vector3D trans(Vector3D v)
        {
            double s = Math.Sin(this.theta);
            double c = Math.Cos(this.theta);
            return new Vector3D(c * v.x + s * v.y - this.p.x * c - this.p.y * s,
                               -s * v.x + c * v.y + this.p.x * s - this.p.y * c);
        }
        public Vector3D itrans(Vector3D v)
        {
            double s = Math.Sin(this.theta);
            double c = Math.Cos(this.theta);
            return new Vector3D(c * v.x - s * v.y + this.p.x,
                               s * v.x + c * v.y + this.p.y);
        }
    }



    /// <summary>
    /// 极坐标
    /// 也是先平移 后旋转
    /// </summary>
    public class TransformationFunctionPolar
    {
        public double theta;
        public Vector3D p;
        public TransformationFunctionPolar(Vector3D v, double theta)
        {
            this.p = v.get_copy();
            this.theta = theta;
        }

        /// <summary>
        /// 计算新坐标系下坐标
        /// 使用vector返回 x为角度 y为半径
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3D trans(Vector3D v)
        {
            //先计算半径
            double x = v.x - this.p.x;
            double y = v.y - this.p.y;
            double rou = Math.Sqrt(x * x + y * y);

            //在计算角度
            double theta = (new Vector3D(x, y)).calc_angle_in_xoy();

            //返回 角度和半径
            return new Vector3D(theta - this.theta, rou);
        }

        public Vector3D itrans(Vector3D v)
        {
            double theta1 = v.x + this.theta;
            double x = v.y * Math.Cos(theta1);
            double y = v.y * Math.Sin(theta1);
            return new Vector3D(x + this.p.x, y + this.p.y);
        }
    }

    public class MyRect
    {
        public Vector3D leftright;
        public Vector3D rightup;
        public MyRect(Vector3D lr, Vector3D ru)
        {
            this.leftright = lr;
            this.rightup = ru;
        }
        public Vector3D center
        {
            get
            {
                return (this.rightup + this.leftright) * 0.5;
            }
        }

        /// <summary>
        /// 判断点是否在内部
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool contain(Vector3D v, double tol = 0.0)
        {
            if (v.x >= this.leftright.x - tol && v.x <= this.rightup.x + tol)
            {
                if (v.y >= this.leftright.y - tol && v.y <= this.rightup.y + tol)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算两个区间的距离  （x1,X1）和（x2,X2）
        /// 相交 返回-1
        /// 相切 返回0
        /// 分离 返回最短距离
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="X1"></param>
        /// <param name="x2"></param>
        /// <param name="X2"></param>
        /// <returns></returns>
        public static double distance_between_sections(double x1, double X1, double x2, double X2)
        {
            if (x2 < x1)//小的是1号
            {
                double t = x1; double T = X1;
                x1 = x2; X1 = X2;
                x2 = t; X2 = T;
            }
            if (x2 < X1)//相交
            {
                return -1;
            }
            else if (x2 == X1)//相切
            {
                return 0;
            }
            else//分离
            {
                return x2 - X1;
            }
        }



        /// <summary>
        /// 计算两个区间的交集
        /// 交集是：区间或者点
        /// 有交集时返回true
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="X1"></param>
        /// <param name="x2"></param>
        /// <param name="X2"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool intersection_between_sections(
            double x1, double X1,
            double x2, double X2,
            out double a, out double b)
        {
            if (x2 < x1)//小的是1号
            {
                double t = x1; double T = X1;
                x1 = x2; X1 = X2;
                x2 = t; X2 = T;
            }
            if (x2 < X1)//相交
            {
                if (X2 <= X1)
                {
                    a = x2; b = X2;
                    return true;
                }
                else
                {
                    a = x2; b = X1;
                    return true;
                }
            }
            else if (x2 == X1)//相切
            {
                a = b = x2;//此时交集是一个点
                return true;
            }
            else//分离
            {
                a = b = 0;//a和b置为0 但不代表0是交集
                return false;
            }
        }


        /// <summary>
        /// 计算两个rect的距离
        /// 相交 返回-1
        /// 相切 返回0
        /// 分离 返回最短距离
        /// </summary>
        /// <param name="other"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double distance_to_rect(MyRect other, out double x, out double y)
        {
            x = MyRect.distance_between_sections(this.leftright.x, this.rightup.x,
                other.leftright.x, other.rightup.x);
            y = MyRect.distance_between_sections(this.leftright.y, this.rightup.y,
                other.leftright.y, other.rightup.y);
            if (x < 0 && y < 0)//x和y方向是相交 两个rect才是相交
            {
                return -1;
            }
            else if (x + y == -1)//有一个方向相切 一个相交，两个rect是相切
            {
                return 0;
            }
            else if (x == 0 && y == 0)//x和y方向是相切 两个rect是相切
            {
                return 0;
            }
            else//两个rect分离 返回最短距离
            {
                if (x < 0)
                {
                    return y;
                }
                if (y < 0)
                {
                    return x;
                }
                return Math.Sqrt(x * x + y * y);
            }
        }


        public MyRect intersection(MyRect other)
        {
            double a, b;
            bool flag;

            //分成两个方向计算
            flag = MyRect.intersection_between_sections(
                this.leftright.x, this.rightup.x,
                other.leftright.x, other.rightup.x,
                out a, out b);
            if (!flag) return null;
            double a1, b1;
            flag = MyRect.intersection_between_sections(
                this.leftright.y, this.rightup.y,
                other.leftright.y, other.rightup.y,
                out a1, out b1);
            if (!flag) return null;
            return new MyRect(new Vector3D(a, a1), new Vector3D(b, b1));
        }

        public static bool operator ==(MyRect m1, MyRect m2)
        {
            if (m1.leftright == m2.leftright && m1.rightup == m2.rightup) return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is MyRect)
            {
                return this == (MyRect)obj;
            }
            return base.Equals(obj);
        }
        public static bool operator !=(MyRect m1, MyRect m2)
        {
            return !(m1 == m2);
        }

        public override string ToString()
        {
            return string.Format("{0:f2},{1:f2}->{2:f2},{3:f2}",
                this.leftright.x, this.leftright.y, this.rightup.x, this.rightup.y);
        }
    }
}
