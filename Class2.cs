using System;

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

        public string ToString()
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
        public bool contain(Vector3D v)
        {
            double tol = 0.0;
            if (v.x >= this.leftright.x - tol && v.x <= this.rightup.x + tol)
            {
                if (v.y >= this.leftright.y - tol && v.y <= this.rightup.y + tol)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
