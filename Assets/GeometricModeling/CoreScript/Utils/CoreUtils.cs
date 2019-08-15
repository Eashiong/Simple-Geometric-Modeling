/*

*
*    功能: 工具类 包含数学计算 、直线和曲线
*
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modeling
{

    namespace CoreUtils
    {

        /// <summary>
        /// 数学计算
        /// </summary>
        public static class MathUtils
        {

            /// <summary>
            /// 返回2点之间的距离（GetDistance）
            /// </summary>
            /// <param name="startSpot">起始</param>
            /// <param name="endSpot">结束</param>
            /// <returns></returns>
            public static float GetStartToEndDistance(Vector3 startSpot, Vector3 endSpot)
            {
                return Vector3.Distance(startSpot, endSpot);

            }

            /// <summary>
            /// 返回2点之间的距离（GetDistance）
            /// </summary>
            /// <param name="startSpot">起始</param>
            /// <param name="endSpot">结束</param>
            /// <returns></returns>
            public static float GetStartToEndDistance(Vector2 startSpot, Vector2 endSpot)
            {
                return Vector2.Distance(startSpot, endSpot);

            }

            /// <summary>
            /// 得到起始点指向终点的向量(GetVector3)
            /// </summary>
            /// <param name="startSpot"></param>
            /// <param name="endSpot"></param>
            /// <returns></returns>
            public static Vector3 GetStartToEndVector3(Vector3 startSpot, Vector3 endSpot)
            {
                return endSpot - startSpot;
            }

            /// <summary>
            /// 得到起始点指向终点的向量(GetVector2)
            /// </summary>
            /// <param name="startSpot"></param>
            /// <param name="endSpot"></param>
            /// <returns></returns>
            public static Vector3 GetStartToEndVector2(Vector2 startSpot, Vector2 endSpot)
            {
                return endSpot - startSpot;
            }

            /// <summary>
            /// 计算两点的中点（GetCenterVector3）
            /// </summary>
            /// <param name="startSpot">起始</param>
            /// <param name="endSpot">结束</param>
            /// <returns></returns>
            public static Vector3 GetCenterSpotVector3(Vector3 startSpot, Vector3 endSpot)
            {
                return (startSpot + endSpot) * 0.5f;
            }


            #region 向量和向量 点和直线之间的投影

            /// <summary>
            /// 求点 在过点p1 p2 的直线上的投影(GetProjection)
            /// </summary>
            public static Vector3 GetPointProjection(Vector3 point, Vector3 point1, Vector3 point2)
            {
                Vector3 point0 = point2 - point1;
                return GetVectorProjection(point - point1, point0) + point1;
            }
            /// <summary>
            /// 求向量p 在向量p0上的投影(GetProjection)
            /// </summary>
            public static Vector3 GetVectorProjection(Vector3 point, Vector3 point0)
            {
                return Vector3.Dot(point, point0) * point0 / point0.magnitude / point0.magnitude;
            }
            /// <summary>
            /// 求点 在过点p1 p2 的直线上的投影(GetProjection)
            /// </summary>
            public static Vector2 GetPointProjection(Vector2 point, Vector2 point1, Vector2 point2)
            {
                Vector2 point0 = point2 - point1;
                return GetVectorProjection(point, point0);
            }
            /// <summary>
            /// 求point在向量point0上的投影(GetProjection)
            /// </summary>
            public static Vector2 GetVectorProjection(Vector2 point, Vector2 point0)
            {
                return Vector2.Dot(point, point0) * point0 / point0.magnitude / point0.magnitude;
            }
            #endregion



            /// <summary>
            /// 得到一个向量在某个平面上的垂直向量，这个垂直向量箭头方向指向平面外 (GetVerticalVector)
            /// </summary>
            /// <param name="vct">向量</param>
            /// <param name="pl">向量所属平面</param>
            /// <returns>返回垂直向量的方向指向平面外</returns>
            public static Vector3 GetOneVerticalVector(Vector3 vct, PlaneV pl = PlaneV.default_ZX)
            {
                Vector3 axi;
                if (pl == PlaneV.x_y)
                {
                    axi = Vector3.forward;
                }
                else if (pl == PlaneV.y_z)
                {
                    axi = Vector3.right;
                }
                else if (pl == PlaneV.z_x)
                {
                    axi = Vector3.up;
                }
                else
                {
                    axi = Vector3.up;
                }
                return Vector3.Normalize(Vector3.Cross(vct, axi));
            }

            /// <summary>
            /// 快速判断两条线段(point1,point2),(point3,point4)是否相交(Intersect)
            /// </summary>
            public static bool LineIntersect(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
            {
                //快速矩形排斥实验
                if (Mathf.Min(point1.x, point2.x) <= Mathf.Max(point3.x, point4.x) &&
                    Mathf.Min(point3.x, point4.x) <= Mathf.Max(point1.x, point2.x) &&
                    Mathf.Min(point1.y, point2.y) <= Mathf.Max(point3.y, point4.y) &&
                    Mathf.Min(point3.y, point4.y) <= Mathf.Max(point1.y, point2.y) &&
                    Mathf.Min(point1.z, point2.z) <= Mathf.Max(point3.z, point4.z) &&
                    Mathf.Min(point3.z, point4.z) <= Mathf.Max(point1.z, point2.z))
                {

                    if (Vector3.Dot(Vector3.Cross(point1 - point3, point4 - point3), Vector3.Cross(point4 - point3, point2 - point3)) >= 0)
                    {
                        if (Vector3.Dot(Vector3.Cross(point3 - point1, point2 - point1), Vector3.Cross(point2 - point1, point4 - point1)) >= 0)
                        {
                            return true;
                        }

                    }
                }
                return false;
            }

            /// <summary>
            /// 2维空间中 A B两向量叉积(Cross2d)
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static float Cross_2d(Vector2 A, Vector2 B)
            {
                float x1 = A.x, y1 = A.y;
                float x2 = B.x, y2 = B.y;
                float f = x1 * y2 - x2 * y1;
                if (f < 0.00001f && f > -0.00001f) f = 0;
                return f;
            }

            /// <summary>
            /// 判断points是否全部共面(PointsInPlane)
            /// </summary>
            /// <returns></returns> 
            public static bool PointsInSamePlane(Vector3[] allPoint)
            {
                if (allPoint.Length <= 3)
                {
                    return true;
                }
                Vector3 v3 = Vector3.zero;
                for (int i = 1; i < allPoint.Length - 1; i++)
                {
                    if (v3 == Vector3.zero)
                    {
                        Vector3 va = allPoint[0] - allPoint[i];
                        Vector3 vb = allPoint[0] - allPoint[i + 1];
                        if (Vector3.Dot(va, vb) == 0)
                        {
                            continue;
                        }
                        else
                        {
                            v3 = Vector3.Cross(va, vb);
                            continue;
                        }
                    }
                    else
                    {
                        if (Vector3.Dot(v3, allPoint[0] - allPoint[i]) != 0)
                        {
                            return false;
                        }
                    }

                }
                if (v3 != Vector3.zero)
                {
                    if (Vector3.Dot(v3, allPoint[0] - allPoint[allPoint.Length - 1]) != 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 判断point是否在plane上(PointsInPlane)
            /// </summary>
            public static bool PointsInSamePlane(Vector3[] thePlane, Vector3[] allPoint)
            {
                Vector3[] temps = new Vector3[thePlane.Length + allPoint.Length];
                thePlane.CopyTo(temps, 0);
                allPoint.CopyTo(temps, thePlane.Length);
                return PointsInSamePlane(temps);
            }

            /// <summary>
            /// 线与面的交点(LineToPlane)
            /// （（P-P0) · normal = 0，P = L0 + dL,P:交点，P0面上的点，L0线上的点，dL距离乘方向）
            /// </summary>
            /// <param name="inPlanePoint">面的一个点</param>
            /// <param name="planeNormalVector">面的法线向量</param>
            /// <param name="linePoint">线的一个点（远面点）</param>
            /// <param name="linePoint1">线的一个点（近面点）</param>
            public static Vector3 LineToPlanePoint(Vector3 inPlanePoint, Vector3 planeNormalVector, Vector3 linePoint, Vector3 linePoint1)
            {
                Vector3 dirction = (linePoint1 - linePoint).normalized;
                float f = Vector3.Dot((inPlanePoint - linePoint), planeNormalVector) / Vector3.Dot(dirction, planeNormalVector);
                return f * dirction + linePoint;
            }
            /// <summary>
            ///  射线与面的交点（LineToPlane）
            /// </summary>
            /// <param name="inPlanePoint">面的一个点</param>
            /// <param name="planeNormalVector">面的法线向量</param>
            /// <param name="r"> 射 线 </param>
            /// <returns></returns>
            public static Vector3 LineToPlanePoint(Vector3 inPlanePoint, Vector3 planeNormalVector, Ray r)
            {
                Vector3 dirction = r.direction;
                float f = Vector3.Dot((inPlanePoint - r.origin), planeNormalVector) / Vector3.Dot(dirction, planeNormalVector);
                return f * dirction + r.origin;
            }

            /// <summary>
            /// 平面
            /// </summary>
            public enum PlaneV
            {
                default_ZX,
                x_y,
                y_z,
                z_x,
            }

            public static Quaternion GetRotation(this Matrix4x4 matrix)
            {
                Quaternion q = new Quaternion();
                q.w = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 + matrix.m11 + matrix.m22)) / 2;
                q.x = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 - matrix.m11 - matrix.m22)) / 2;
                q.y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 + matrix.m11 - matrix.m22)) / 2;
                q.z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 - matrix.m11 + matrix.m22)) / 2;
                q.x = _copysign(q.x, matrix.m21 - matrix.m12);
                q.y = _copysign(q.y, matrix.m02 - matrix.m20);
                q.z = _copysign(q.z, matrix.m10 - matrix.m01);
                return q;
            }
            private static float _copysign(float sizeval, float signval)
            {
                return Mathf.Sign(signval) == 1 ? Mathf.Abs(sizeval) : -Mathf.Abs(sizeval);
            }
        }


        /// <summary>
        /// 2阶 贝塞尔曲线
        /// </summary>
        [System.Serializable]
        public class Bezier_2D
        {
            private Vector3 startPoint, endPoint;

            /// <summary>
            /// 构造2阶的贝塞尔曲线(Bezier2D)
            /// </summary>
            /// <param name="startPoint">固定曲线起始点</param>
            /// <param name="endPoint">固定曲线末端</param>
            public Bezier_2D(Vector3 startPoint, Vector3 endPoint)
            {
                this.startPoint = startPoint;
                this.endPoint = endPoint;
            }
            /// <summary>
            /// 在曲线上进行坐标采样(GetCurvePoint2D)
            /// </summary>
            /// <param name="controlPoint"> 控制点 </param>
            /// <param name="f"> 采样数 </param>
            /// <returns></returns>
            public Vector3 GetCurvePoint_2D(Vector3 controlPoint, float f)
            {
                return (1 - f) * (1 - f) * startPoint + 2 * f * (1 - f) * controlPoint + f * f * endPoint;

            }
            public override string ToString()
            {
                return "->Bezier2D:start(" + startPoint + ")，end(" + endPoint + ")," + base.ToString();

            }
        }

        /// <summary>
        /// 笛卡尔坐标系中表示 --- 直线 线段 和射线 
        /// </summary>
        public class LineSegment_2D
        {

            #region 私有 成员
            //一般式 y = kx + b

            private float yValue;
            private float kValue;
            private float bValue;
            private float xValue;

            private bool _vertical_X = false;//垂直X
            private bool _vertical_Y = false;//垂直Y

            //作用域与值域
            private float _min_X;
            private float _max_X;
            private float _min_Y;
            private float _max_Y;
            #endregion

            /// <summary>
            /// 是否垂直于X轴
            /// </summary>
            public bool vertical_X
            {
                get
                {
                    return _vertical_X;
                }
                private set
                {
                    _vertical_X = value;
                }
            }

            /// <summary>
            /// 是否垂直于Y轴
            /// </summary>
            public bool vertical_Y
            {
                get
                {
                    return _vertical_Y;
                }
                private set
                {
                    _vertical_Y = value;
                }
            }

            /// <summary>
            /// 最小 作用域
            /// </summary>
            public float min_X
            {
                get
                {
                    return _min_X;
                }
                private set
                {
                    _min_X = value;
                }
            }

            /// <summary>
            /// 最大 作用域
            /// </summary>
            public float min_Y
            {
                get
                {
                    return _min_Y;
                }
                private set
                {
                    _min_Y = value;
                }
            }

            /// <summary>
            /// 最小 值域
            /// </summary>
            public float max_X
            {
                get
                {
                    return _max_X;
                }
                private set
                {
                    _max_X = value;
                }
            }

            /// <summary>
            /// 最大 值域
            /// </summary>
            public float max_Y
            {
                get
                {
                    return _max_Y;
                }
                private set
                {
                    _max_Y = value;
                }
            }

            /// <summary>
            /// 用点point1 point2构造一个线 （LineSegment2D）
            /// 作用域 r.x 和r.y 中仅有一个为无穷数时，该线为射线，都是无穷数时为直线,否则为线段
            /// if verticalX is true,ryTest is range of y
            /// </summary>
            public LineSegment_2D(Vector2 point1, Vector2 point2, Vector2 rayx, Vector2 rayTest)
            {

                if (point1 == point2)
                {
                    throw new System.Exception("相同的2个点不能决定一条直线.p1:" + point1.ToString() + ",p2:" + point2.ToString());
                }
                //x = 常数
                if (point1.x == point2.x)
                {
                    xValue = point1.x;
                    vertical_X = true;

                    min_X = max_X = point1.x;




                    min_Y = Mathf.Min(rayTest.x, rayTest.y);
                    max_Y = Mathf.Max(rayTest.x, rayTest.y);

                }

                // y = 常数
                else if (point1.y == point2.y)
                {
                    yValue = point1.y;
                    vertical_Y = true;

                    min_Y = max_Y = point1.y;
                    min_X = Mathf.Min(rayx.x, rayx.y);
                    max_X = Mathf.Max(rayx.x, rayx.y);
                }
                //y = kx + b
                else
                {
                    kValue = (point1.y - point2.y) / (point1.x - point2.x);
                    bValue = point1.y - kValue * point1.x;

                    min_X = Mathf.Min(rayx.x, rayx.y);
                    max_X = Mathf.Max(rayx.x, rayx.y);
                    if (kValue > 0)
                    {
                        if (min_X == float.NegativeInfinity)
                        {
                            max_Y = float.PositiveInfinity;
                        }
                        else
                            max_Y = kValue * min_X + bValue;
                        if (max_X == float.PositiveInfinity)
                        {
                            min_Y = float.NegativeInfinity;
                        }
                        else
                            min_Y = kValue * max_X + bValue;
                    }
                    else
                    {
                        if (min_X == float.NegativeInfinity)
                        {
                            min_Y = float.NegativeInfinity;
                        }
                        else
                            min_Y = kValue * min_X + bValue;
                        if (max_X == float.PositiveInfinity)
                        {
                            max_Y = float.PositiveInfinity;
                        }
                        else
                            max_Y = kValue * max_X + bValue;
                    }


                }
            }

            /// <summary>
            /// 根据X值来求Y值(GetY)
            /// </summary>
            /// <returns>X值是否在作用域上,如果不在无法求出Y</returns>
            public bool ByXGetY(float X, out float Y)
            {
                if (X >= min_X && X <= max_X)
                {
                    if (vertical_Y)
                    {
                        Y = this.yValue;
                        return true;
                    }
                    else if (vertical_X)
                    {
                        Y = 0;
                        return true;
                    }
                    else
                    {
                        Y = kValue * X + bValue;
                        return true;
                    }
                }
                Y = 0;
                return false;
            }

            /// <summary>
            /// 判断点P是否在线上
            /// </summary>
            public bool PointInLine(Vector2 point)
            {
                float testy;
                if (ByXGetY(point.x, out testy))
                {

                    if (Mathf.Abs(testy - point.y) < 0.0001f || testy == 0)
                    {
                        if (testy == 0)
                        {
                            if (point.y < min_Y || point.y > max_Y)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 判断两条线相交测试(GetIntersect)
            /// </summary>
            /// <param name="point">如果相交返回的交点坐标</param>
            /// <returns>是否相交</returns>
            public static bool GetLineIntersect(LineSegment_2D line1, LineSegment_2D line2, out Vector2 point)
            {
                point = Vector2.zero;

                if (line1.vertical_X)
                {
                    if (line2.vertical_X)
                    {
                        return false;
                    }
                    else if (line2.vertical_Y)
                    {
                        Vector2 testP = new Vector2(line1.xValue, line2.yValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Vector2 testP = new Vector2(line1.xValue, line2.kValue * line1.xValue + line2.bValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }

                if (line1.vertical_Y)
                {
                    if (line2.vertical_Y)
                    {
                        return false;
                    }
                    else if (line2.vertical_X)
                    {
                        Vector2 testP = new Vector2(line2.xValue, line1.yValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Vector2 testP = new Vector2((line1.yValue - line2.bValue) / line2.kValue, line1.yValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }

                else
                {
                    if (line2.vertical_X)
                    {
                        Vector2 testP = new Vector2(line2.xValue, line1.kValue * line2.xValue + line1.bValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (line2.vertical_Y)
                    {
                        Vector2 testP = new Vector2((line2.yValue - line1.bValue) / line1.kValue, line2.yValue);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (line1.kValue == line2.kValue)
                        {
                            return false;
                        }
                        float testX = (line2.bValue - line1.bValue) / (line1.kValue - line2.kValue);
                        float testY = line1.kValue * testX + line1.bValue;
                        Vector2 testP = new Vector2(testX, testY);
                        if (line1.PointInLine(testP) && line2.PointInLine(testP))
                        {
                            point = testP;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }

            }
        }




    }
}
