/*
*      
*    功能: Mesh生成底层模块
*    
*    自定义建模
*    1,生成简单的几何体请使用 SimpleMeshCreator提供的静态方法
*    2,Polygon可生成简单多边形单面
*    3,生成非简单的几何体 可用Polygon生成该简单集合体的各个面 然后使用MeshData里面的合并函数将它们合并成体
*    4,MeshData 包含基本的顶点 网格 UV数据
*
*    简化建模
*    使用ParameterModeling类建模
*
*/

using System;
using UnityEngine;
using System.Collections.Generic;
using Modeling.CoreUtils;

namespace Modeling
{
    //通用多边形生成算法
    namespace Polygon
    {

        /// <summary>
        /// 多边形方向（顺时针or逆时针）
        /// </summary>
        public enum PolygonSymbol
        {
            /// <summary>
            /// 顺时针
            /// </summary>
            Positive,
            /// <summary>
            /// 逆时针
            /// </summary>
            Negative,
            /// <summary>
            /// 共线
            /// </summary>
            Line,
            /// <summary>
            /// 未计算
            /// </summary>
            None

        }
        /// <summary>
        /// 多边形类型(凹多边形or凸多边形)
        /// </summary>
        public enum PolygonType
        {
            /// <summary>
            /// 凸多边形
            /// </summary>
            Convex,
            /// <summary>
            /// 凹多边形
            /// </summary>
            Concave,
            /// <summary>     
            /// 未计算
            /// </summary>
            None

        }


        /// <summary>
        /// 凹凸简单多边形算法
        /// 只适用简单多边形
        /// </summary>
        public class SimplePolygon
        {          
            #region 公共接口

           
            /// <summary>
            /// 按2维平面多边形有序传入顶点 构造一个多边形
            /// </summary>
            /// <param name="vertex2dList"></param>
            public SimplePolygon(List<Vector2> vertex2dList)
            {
                this.area = 0;
                this.polygonsList = new List<SimplePolygon>();
                this.polygonsList.Add(this);
                this.vertex2dList = new List<Vector2>(vertex2dList);
               
                this.point2dLink = new DoubleLink<PolygonPoint2D>();

                //构建多边形链式结构                 
                this.point2dLink.Append(0, new PolygonPoint2D(vertex2dList[0].x, vertex2dList[0].y, 0));              
                for (int i = 1; i < vertex2dList.Count; i++)
                {                   
                    this.point2dLink.Append(i - 1, new PolygonPoint2D(vertex2dList[i].x, vertex2dList[i].y, i));                  
                }

                //根据极值点方向判断多边形顺/逆时针

                //单独取出索引值
                int[] maxY_indexs = new int[vertex2dList.Count];
                for(int i = 0; i<maxY_indexs.Length;i++)
                {
                    maxY_indexs[i] = i;
                }
                //根据Y值大小给索引排序
                for(int i = 0;i< maxY_indexs.Length - 1;i++)
                {
                    for(int j = i+1;j< maxY_indexs.Length;j++)
                    {
                        if(vertex2dList[maxY_indexs[j]].y > vertex2dList[maxY_indexs[i]].y)
                        {
                            int index = maxY_indexs[j];
                            maxY_indexs[j] = maxY_indexs[i];
                            maxY_indexs[i] = index;     
                        }
                    }
                }

                //根据极值相邻两点三角形的方向判断多边形方向
                for (int i = 0;i< maxY_indexs.Length;i++)
                {                    
                    var maxPoint = new PolygonPoint2D(this.point2dLink.GetSafe(maxY_indexs[i]));
                    var last_maxPoint = this.point2dLink.GetSafe(maxY_indexs[i] - 1);
                    var next_maxPoint = this.point2dLink.GetSafe(maxY_indexs[i] + 1);
                    maxPoint.SetSymbol(last_maxPoint, next_maxPoint);
                    if(maxPoint.GetSymbol()!= PolygonSymbol.Line)
                    {
                        this.polygonSymbol = maxPoint.GetSymbol();
                    }
                }
                

                
            }

            /// <summary>
            /// 运行多边形生成算法 只有调用该方法后 后面的接口方法才有效
            /// </summary> 
            public void Execute()
            {
                try
                {
                    ComputeProperty();
                    ConcaveSplit();
                    ComputeTriangle();
                }
                catch(Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }

            /// <summary>
            /// 多边形方向 （顺时针/逆时针）
            /// </summary>
            public PolygonSymbol symbol { get { return this.polygonSymbol; } }

            /// <summary>
            /// 多边形类型 （凹多边形/凸多边形）
            /// </summary>
            public PolygonType type { get { return this.polygonType; } }

            /// <summary>
            /// 得到三角数组
            /// </summary>
            /// <param name="reverse">true时正面朝下  false正面朝上</param>
            /// <returns></returns>
            public int[] GetTriangles(bool reverse = false)
            {
                if(this.polygonSymbol == PolygonSymbol.Negative)
                {
                    reverse = !reverse;
                }

                int[] t = new int[triangleList.Count * 3];
                for (int i = 0, j = 0; i < triangleList.Count; i++)
                {
                    if(reverse)
                    {
                        t[j++] = triangleList[i].point2dLink.GetFirst().index;
                        t[j++] = triangleList[i].point2dLink.Get(2).index;
                        t[j++] = triangleList[i].point2dLink.Get(1).index;
                    }
                    else
                    {
                        t[j++] = triangleList[i].point2dLink.GetFirst().index;
                        t[j++] = triangleList[i].point2dLink.Get(1).index;
                        t[j++] = triangleList[i].point2dLink.Get(2).index;
                    }
                    

                }
                return t;
            }

            /// <summary>
            /// 得到顶点数组
            /// </summary>
            public Vector2[] GetVertices()
            {
               // Debug.Log(InitCenter().ToString("f4"));
                return vertex2dList.ToArray();
            }
            
            /// <summary>
            /// 得到UV数组
            /// </summary>
            public Vector2[] GetUV()
            {
                //计算中心
                float minX = vertex2dList[0].x;
                float minY = vertex2dList[0].y;
                float maxX = minX;
                float maxY = minY;
                for (int i = 0; i < vertex2dList.Count; i++)
                {
                    if (minX >= vertex2dList[i].x)
                    {
                        minX = vertex2dList[i].x;

                    }
                    if (minY >= vertex2dList[i].y)
                    {
                        minY = vertex2dList[i].y;
                    }
                    if (maxX <= vertex2dList[i].x)
                    {
                        maxX = vertex2dList[i].x;

                    }
                    if (maxY <= vertex2dList[i].y)
                    {
                        maxY = vertex2dList[i].y;
                    }
                }
                float lenX = maxX - minX;
                float lenY = maxY - minY;

                //计算UV
                Vector2[] uv = new Vector2[vertex2dList.Count];
                for (int i = 0; i < vertex2dList.Count; i++)
                {
                    uv[i] = new Vector2((vertex2dList[i].x - minX) / lenX, (vertex2dList[i].y - minY) / lenY);
                }
                return uv;
            }

            /// <summary>
            /// 得到多边形面积
            /// </summary>
            public float GetArea()
            {
                return area;
            }

            /// <summary>
            /// 计算每个凸多边形中心(重心)，返回多个的原因是凹多边形会拆分成若干个凸多边形
            /// </summary>
            /// <returns></returns>
            public Vector2[] GetCenters()
            {
                Vector2[] centers = new Vector2[polygonsList.Count];
                for(int i =0;i<centers.Length;i++)
                {
                    centers[i] = GetCenter(polygonsList[i].point2dLink);
                }
                return centers;
            }

            /// <summary>
            /// 计算每个凸多边形外接矩形，返回多个的原因是凹多边形会拆分成若干个凸多边形
            /// </summary>
            /// <returns></returns>
            public Rect[] GetBoundRects()
            {
                Rect[] rects = new Rect[polygonsList.Count];
                for (int i = 0; i < rects.Length; i++)
                {
                    rects[i] = GetBoundRects(polygonsList[i].point2dLink);
                }
                return rects;
            }

            #region 按要求计算多边形拆分

            /// <summary>
            /// 求最大面积解的凸多边形分割点集，确保已调用“MaxAreaPolygon(List<Vector2> vertex2dList,out SimplePolygon maxAreaPolygon)”方法;
            /// </summary>
            /// <param name="symbol">设置返回点集的顺序</param>
            /// <returns></returns>
            public List<Vector2> GetMaxAreaPoint(PolygonSymbol symbol)
            {
                List<Vector2> points = new List<Vector2>();
                int len = point2dLink.GetSize();
                if (this.polygonSymbol == symbol)
                {

                    for (int i = 0; i < len; i++)
                    {
                        points.Add(point2dLink.Get(i).point);
                    }
                }
                else
                {
                    for (int i = len - 1; i >=0; i--)
                    {
                        points.Add(point2dLink.Get(i).point);
                    }
                }
                return points;
            }

            /// <summary>
            /// 求最大面积解的凸多边形分割。该方法返回最大面积的一个
            /// </summary>
            /// <param name="vertex2dList">原型坐标</param>
            /// <param name="maxAreaPolygon">计算结果</param>
            public void MaxAreaPolygon(List<Vector2> vertex2dList,out SimplePolygon maxAreaPolygon)
            { 
                float maxArea1;
                SimplePolygon maxAreaPolygon1;
                MaxAreaPolygon(out maxArea1, out maxAreaPolygon1);

                float maxArea2;
                SimplePolygon maxAreaPolygon2;
                List<Vector2> reverse = new List<Vector2>();
                int len = vertex2dList.Count;
                for (int  i = 0;i< len;i++)
                {
                    reverse.Add(vertex2dList[len - i - 1]);
                }

                //逆序的方式在计算一次
                SimplePolygon simplePolygon = new SimplePolygon(reverse);
                simplePolygon.ComputeProperty();
                simplePolygon.ConcaveSplit();
               // simplePolygon.Execute();
                simplePolygon.MaxAreaPolygon(out maxArea2, out maxAreaPolygon2);
                if(maxArea2>=maxArea1)
                {
                    maxAreaPolygon = maxAreaPolygon2;
                    maxAreaPolygon.area = maxArea2;
                }
                else
                {
                    maxAreaPolygon = maxAreaPolygon1;
                    maxAreaPolygon.area = maxArea1;
                }
                maxAreaPolygon.DelLinePoint();
            }

            /// <summary>
            /// 求最大面积解的凸多边形所有分割点集 polygons[0]为最大面积的那个
            /// </summary>
            /// <param name="vertex2dList">原型顶点</param>
            /// <param name="polygons">结果集合</param>
            public void MaxAreaPolygon(List<Vector2> vertex2dList, out List<SimplePolygon> polygons)
            {
                float maxArea1;
                polygons = new List<SimplePolygon>();

                SimplePolygon maxAreaPolygon1;
                MaxAreaPolygon(out maxArea1, out maxAreaPolygon1);
                maxAreaPolygon1.DelLinePoint();
                //逆序的方式在计算一次
                float maxArea2;
                SimplePolygon maxAreaPolygon2;
                List<Vector2> reverse = new List<Vector2>();
                int len = vertex2dList.Count;
                for (int i = 0; i < len; i++)
                {
                    reverse.Add(vertex2dList[len - i - 1]);
                }
                SimplePolygon simplePolygon = new SimplePolygon(reverse);
                simplePolygon.ComputeProperty();
                simplePolygon.ConcaveSplit();
                // simplePolygon.Execute();
                simplePolygon.MaxAreaPolygon(out maxArea2, out maxAreaPolygon2);
                maxAreaPolygon2.DelLinePoint();
                //进行比较最大面积判断
                if (maxArea2 >= maxArea1)
                {
                    maxAreaPolygon2.area = maxArea2;
                    polygons.Add(maxAreaPolygon2);
                    for (int i = 0; i < simplePolygon.polygonsList.Count; i++)
                    {
                        //防止maxAreaPolygon2多次添加
                        if (simplePolygon.polygonsList[i] != maxAreaPolygon2)
                        {
                            simplePolygon.polygonsList[i].DelLinePoint();
                            if (IsRect(simplePolygon.polygonsList[i]))
                                polygons.Add(simplePolygon.polygonsList[i]);
                        }
                    }
                }
                else
                {
                    maxAreaPolygon1.area = maxArea1;

                    polygons.Add(maxAreaPolygon1);
                    for (int i = 0; i < this.polygonsList.Count; i++)
                    {
                        //防止maxAreaPolygon2多次添加
                        if (this.polygonsList[i] != maxAreaPolygon1)
                        {
                            this.polygonsList[i].DelLinePoint();
                            if (IsRect(this.polygonsList[i]))
                                polygons.Add(this.polygonsList[i]);
                        }
                    }
                }
            }

            /// <summary>
            /// 求最大面积解的凸多边形所有分割点集 points[0]为最大面积的那个
            /// </summary>
            /// <param name="vertex2dList">原型顶点</param>
            /// <param name="points">计算结果</param>
            /// <param name="polygonSymbol">设置返回结果的顺序</param>
            public void MaxAreaPolygon(List<Vector2> vertex2dList, out List<List<Vector2>> points, PolygonSymbol polygonSymbol = PolygonSymbol.Positive)
            {
                points = new List<List<Vector2>>();
                List<SimplePolygon> polygons;
                MaxAreaPolygon(vertex2dList, out polygons);
                for (int i = 0; i < polygons.Count; i++)
                {
                    points.Add(polygons[i].GetMaxAreaPoint(polygonSymbol));
                }
            }          

            /// <summary>
            /// 求最大面积解的所有矩形分割
            /// </summary>
            /// <param name="vertex2dList">原型顶点</param>
            /// <param name="maxAreaPolygon"></param>
            public void MaxAreaRect(List<Vector2> vertex2dList, out List<SimplePolygon> polygons)
            {
                float maxArea1;
                polygons = new List<SimplePolygon>();

                SimplePolygon maxAreaPolygon1;
                MaxAreaPolygon(out maxArea1, out maxAreaPolygon1);
                maxAreaPolygon1.DelLinePoint();
                //逆序的方式在计算一次
                float maxArea2;
                SimplePolygon maxAreaPolygon2;
                List<Vector2> reverse = new List<Vector2>();
                int len = vertex2dList.Count;
                for (int i = 0; i < len; i++)
                {
                    reverse.Add(vertex2dList[len - i - 1]);
                }              
                SimplePolygon simplePolygon = new SimplePolygon(reverse);
                simplePolygon.ComputeProperty();
                simplePolygon.ConcaveSplit();
                // simplePolygon.Execute();
                simplePolygon.MaxAreaPolygon(out maxArea2, out maxAreaPolygon2);
                maxAreaPolygon2.DelLinePoint();
                //进行比较最大面积和判断是否为多边形
                if (maxArea2 >= maxArea1 && IsRect(maxAreaPolygon2))
                {
                    maxAreaPolygon2.area = maxArea2;
                    polygons.Add(maxAreaPolygon2);
                    for(int i = 0;i< simplePolygon.polygonsList.Count;i++)
                    {
                        //防止maxAreaPolygon2多次添加
                        if (simplePolygon.polygonsList[i] != maxAreaPolygon2 )
                        {
                            simplePolygon.polygonsList[i].DelLinePoint();
                            if(IsRect(simplePolygon.polygonsList[i]))
                                polygons.Add(simplePolygon.polygonsList[i]);
                        }
                    }
                }
                else if(IsRect(maxAreaPolygon1))
                {
                    maxAreaPolygon1.area = maxArea1;
                    
                    polygons.Add(maxAreaPolygon1);
                    for (int i = 0; i < this.polygonsList.Count; i++)
                    {
                        //防止maxAreaPolygon2多次添加
                        if (this.polygonsList[i]!= maxAreaPolygon1)
                        {
                            this.polygonsList[i].DelLinePoint();
                            if(IsRect(this.polygonsList[i]))
                                polygons.Add(this.polygonsList[i]);
                        }
                    }
                }              
                
            }

            /// <summary>
            /// 求最大面积解的所有矩形分割
            /// </summary>
            /// <param name="vertex2dList">原型顶点</param>
            /// <param name="points">返回所有分割结果点集</param>
            public void MaxAreaRect(List<Vector2> vertex2dList, out List<List<Vector2>> rects,PolygonSymbol polygonSymbol = PolygonSymbol.Positive)
            {
                rects = new List<List<Vector2>>();
                List<SimplePolygon> polygons;
                MaxAreaRect(vertex2dList, out polygons);
                for(int i = 0;i<polygons.Count;i++)
                {
                    rects.Add(polygons[i].GetMaxAreaRectPoint(polygonSymbol));
                }
            }

            /// <summary>
            /// 求最大面积解的所有矩形分割点集 确保调用"MaxAreaRect(List<Vector2> vertex2dList, out List<SimplePolygon> polygons)"方法
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            private List<Vector2> GetMaxAreaRectPoint(PolygonSymbol symbol)
            {
                List<Vector2> points = new List<Vector2>();
                int len = point2dLink.GetSize();
                if (this.polygonSymbol == symbol)
                {

                    for (int i = 0; i < len; i++)
                    {
                        points.Add(point2dLink.Get(i).point);
                    }
                }
                else
                {
                    for (int i = len - 1; i >= 0; i--)
                    {
                        points.Add(point2dLink.Get(i).point);
                    }
                }
                return points;
            }

            #endregion

            /// <summary>
            /// 多边形凹->凸转化 委托
            /// </summary>
            /// <param name="v">在多边形的边上新创建出来的点</param>
            public delegate void CreatePointHander(Vector2 v);

            /// <summary>
            /// 多边形凹->凸转化 事件
            /// </summary>
            public static event CreatePointHander PointEvent;

            #endregion

            #region 私有

            /// <summary>
            /// 多边形的最初顶点，需要运行算法得到最终顶点
            /// </summary>
            private DoubleLink<PolygonPoint2D> point2dLink;
            /// <summary>
            /// 多边形方向
            /// </summary>
            private PolygonSymbol polygonSymbol = PolygonSymbol.Positive;

            /// <summary>
            /// 多边形凹凸性
            /// </summary>
            private PolygonType polygonType = PolygonType.None;

            /// <summary>
            /// 多边形最终顶点，可直接用来三角化
            /// </summary>
            private List<Vector2> vertex2dList;

            /// <summary>
            /// 每次拆分多边形存储区
            /// </summary>
            private List<SimplePolygon> polygonsList;
            /// <summary>
            /// 被计算出来的三角形数组
            /// </summary>
            private List<SimplePolygon> triangleList;

            /// <summary>
            /// 多边形面积
            /// </summary>
            private float area;

            private SimplePolygon(PolygonSymbol polygonSymbol)
            {
                this.polygonSymbol = polygonSymbol;
                this.point2dLink = new DoubleLink<PolygonPoint2D>();
                this.polygonsList = new List<SimplePolygon>();
                this.polygonsList.Add(this);
                this.vertex2dList = new List<Vector2>();
                this.area = 0;
            }

            //计算多边形中心
            private Vector2 GetCenter(DoubleLink<PolygonPoint2D> points)
            {
                Vector2 center = Vector2.zero;
                float len = points.GetSize();               
                if (len == 0)
                {
                    Debug.LogError("坐标链表无数据");
                    return center;
                }
                for (int i = 0;i<len;i++)
                {
                    center += points.Get(i).point;
                }
                center = center / len;             
                return center;
            }

            private Rect GetBoundRects(DoubleLink<PolygonPoint2D> points)
            {
                Vector2 center = Vector2.zero;
                float len = points.GetSize();
                if (len == 0)
                {
                    Debug.LogError("坐标链表无数据");
                    return new Rect();
                }
                Vector2 p0 = points.GetFirst().point;
                float max_x = p0.x, min_x = p0.x, max_y = p0.y,min_y = p0.y;
                for (int i = 0; i < len; i++)
                {
                    Vector2 p = points.Get(i).point;
                    max_x = Mathf.Max(max_x, p.x);
                    min_x = Mathf.Min(min_x, p.x);
                    max_y = Mathf.Max(max_y, p.y);
                    min_y = Mathf.Min(min_y, p.y);
                    center += p;
                }
                center = center / len;
                float w = max_x - min_x;
                float h = max_y - min_y;
                return new Rect(center.x - 0.5f * w, center.y - 0.5f * h, w, h);
            }
            // 计算凹凸性
            private void ComputeProperty()
            {
                PolygonType polygonType = PolygonType.Convex;

                //倒数第一个点 开头第一个点 开头第二个点
                PolygonPoint2D a1 = point2dLink.Get(point2dLink.GetSize() - 1);
                PolygonPoint2D b1 = point2dLink.Get(0);
                PolygonPoint2D c1 = point2dLink.Get(1);
                PolygonSymbol symbol = b1.SetSymbol(a1, c1);
                if (symbol != PolygonSymbol.Line && polygonSymbol != symbol)
                {
                    polygonType = PolygonType.Concave;
                }
                //中间部分
                for (int i = 0; i < point2dLink.GetSize() - 2; i++)
                {
                    PolygonPoint2D a2 = point2dLink.Get(i);
                    PolygonPoint2D b2 = point2dLink.Get(i + 1);
                    PolygonPoint2D c2 = point2dLink.Get(i + 2);
                    symbol = b2.SetSymbol(a2, c2);
                    if (symbol != PolygonSymbol.Line && polygonSymbol != symbol)
                    {
                        polygonType = PolygonType.Concave;
                    }
                }
                //倒数第二个点 倒数第一个点 开头第一个点
                PolygonPoint2D a3 = point2dLink.Get(point2dLink.GetSize() - 2);
                PolygonPoint2D b3 = point2dLink.Get(point2dLink.GetSize() - 1);
                PolygonPoint2D c3 = point2dLink.Get(0);
                symbol = b3.SetSymbol(a3, c3);
                if (symbol != PolygonSymbol.Line && polygonSymbol != symbol)
                {
                    polygonType = PolygonType.Concave;
                }

                this.polygonType = polygonType;

            }

            // 凹点多边形拆分
            // 根据多边形凹点可见性进行拆分
            private void ConcaveSplit()
            {
                int vertexIndex = -1;
                here: while (vertexIndex + 1 < polygonsList.Count)
                {
                    SimplePolygon s = polygonsList[vertexIndex + 1];
                    var point2dLink = s.point2dLink;
                    for (int i = 0; i < point2dLink.GetSize(); i++)
                    {
                        PolygonPoint2D end = point2dLink.Get(i);
                       if (end.GetSymbol() != PolygonSymbol.Line && end.GetSymbol() != PolygonSymbol.None && end.GetSymbol() != polygonSymbol)
                        {
                            List<Vector2> pList = new List<Vector2>();
                            PolygonPoint2D start = point2dLink.GetSafe(i - 1);
                            Debug.Log("起始:" + start.ToString("f3"));
                            Debug.Log("凹点:" + end.ToString("f3"));
                            List<int> indexs = new List<int>();
                            for (int j = i + 1; j < point2dLink.GetSize() + i - 2; j++)
                            {
                                PolygonPoint2D a = point2dLink.GetSafe(j);
                                PolygonPoint2D b = point2dLink.GetSafe(j + 1);
                                Vector2 p;
                                if (GetIntersect(start, end, a, b, out p))
                                {
                                    Debug.Log("A点:" + a.ToString("f3"));
                                    Debug.Log("B点:" + b.ToString("f3"));
                                    Debug.Log("交点:" + p.ToString("f3"));
                                    indexs.Add(point2dLink.NormalIndex(j));
                                    pList.Add(p);
                                }                          
                            }
                            //求最近的那个点
                            if (pList.Count == 0)
                            {
                                //throw new Exception("多边形不合法:凹点引发的射线未能与其他多边形边相交.射线起始点:" + start.ToString() + ",凹点:" + end.ToString());
   
                                return;
                            }

                            Vector2 minP = pList[0];
                            float minDis = Vector2.Distance(pList[0], end.point);
                            int _k = 0;
                            for (int k = 1; k < pList.Count; k++)
                            {
                                float dis = Vector2.Distance(pList[k], end.point);
                                if (dis <= minDis)
                                {
                                    _k = k;
                                    minDis = dis;
                                    minP = pList[k];
                                }
                            }
                            int index = indexs[_k];
                            Debug.Log("采用交点:" + minP.ToString("f3"));
                            point2dLink.Append(index, new PolygonPoint2D(minP, vertex2dList.Count));
                            vertex2dList.Add(minP);
                            if (PointEvent != null)
                            {
                                PointEvent(minP);
                            }
                            //按顺时针拼接新的多边形，他们的符号应该是相同的。按逆时针拼接则符号相反
                            SimplePolygon polygon1 = new SimplePolygon(this.polygonSymbol);
                            SimplePolygon polygon2 = new SimplePolygon(this.polygonSymbol);
                            
                            if (i > index) i++;//凹点位置
                            index++;//拼接点位置
                            if (i > index)
                            {
                                //从拼接点到凹点顺时针组成新的多边形
                                for (int j = index; j <= i; j++)
                                {
                                    polygon1.point2dLink.AppendLast(new PolygonPoint2D(point2dLink.Get(j)));
                                }
                                //从凹点到拼接点顺时针组成新的多边形
                                for (int j = i; j <= point2dLink.GetSize() + index; j++)
                                {
                                    polygon2.point2dLink.AppendLast(new PolygonPoint2D(point2dLink.GetSafe(j)));
                                }
                            }
                            else
                            {
                                //从拼接点到凹点顺时针组成新的多边形
                                for (int j = index; j <= point2dLink.GetSize() + i; j++)
                                {
                                    polygon1.point2dLink.AppendLast(new PolygonPoint2D(point2dLink.GetSafe(j)));
                                }
                                //从凹点到拼接点顺时针组成新的多边形
                                for (int j = i; j <= index; j++)
                                {
                                    polygon2.point2dLink.AppendLast(new PolygonPoint2D(point2dLink.Get(j)));
                                }
                            }
                            polygon1.ComputeProperty();
                            polygon2.ComputeProperty();
                            Debug.Log("分割多边形:" + polygon1.point2dLink.ToString());
                            Debug.Log("分割多边形:" + polygon2.point2dLink.ToString());
                            polygonsList.Add(polygon1);
                            polygonsList.Add(polygon2);

                            polygonsList.RemoveAt(vertexIndex + 1);
                            goto here;
                      }
                    }
                    //走到这里说明不可再分
                    vertexIndex++;
                }
            }

            //由凹点的前继点作为起始点，过凹点的射线和多边形的边进行相交测试
            private bool GetIntersect(PolygonPoint2D start, PolygonPoint2D end, PolygonPoint2D a, PolygonPoint2D b, out Vector2 p)
            {
                if(a.point == b.point || start.point == end.point)
                {
                    p = Vector3.zero;
                    return false;
                }
                float rx,ry;
                if (start.point.x < end.point.x)
                {
                    rx = float.PositiveInfinity;
                }
                else if(start.point.x == end.point.x)
                {
                    rx = start.point.x;
                }
                else
                {
                    rx = float.NegativeInfinity;                  
                }
                if (start.point.y > end.point.y)
                {
                    ry = float.NegativeInfinity;
                }
                else if (start.point.y == end.point.y)
                {
                    ry = start.point.y;
                }
                else
                {
                    ry = float.PositiveInfinity;
                }

                LineSegment_2D rayA = new LineSegment_2D(start.point, end.point, new Vector2(start.point.x, rx),new Vector2(start.point.y,ry));

                LineSegment_2D segmentB = new LineSegment_2D(a.point, b.point, new Vector2(a.point.x, b.point.x),new Vector2(a.point.y,b.point.y));

                if (LineSegment_2D.GetLineIntersect(rayA, segmentB, out p))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            //计算三角形数组
            private void ComputeTriangle()
            {
                List<SimplePolygon> simplePolygons = new List<SimplePolygon>();
                foreach (SimplePolygon s in polygonsList)
                {
                    int index = 1;
                    var point2dLink = s.point2dLink;
                    while (index < point2dLink.GetSize() - 1)
                    {
                        //下面按正序给新多边形插点 所以新的多边形方向和当前的一致
                        SimplePolygon simplePolygon = new SimplePolygon(this.polygonSymbol);

                        var a = point2dLink.GetFirst();
                        var b = point2dLink.Get(index);
                        var c = point2dLink.Get(index + 1);

                        simplePolygon.point2dLink.Append(0, a);
                        simplePolygon.point2dLink.Append(0, b);
                        simplePolygon.point2dLink.Append(1, c);

                        //顺便把面积算一下
                        Vector2 ab = b.point - a.point;
                        Vector2 ac = c.point - a.point;
                        float area = MathUtils.Cross_2d(ab, ac)*0.5f;
                        this.area += Math.Abs(area);

                        simplePolygons.Add(simplePolygon);
                        index++;
                    }
                }
                triangleList = simplePolygons;
            }

            //返回最大面积的凸多边形
            private void MaxAreaPolygon(out float maxArea, out SimplePolygon maxAreaPolygon)
            {
                maxArea = 0;
                maxAreaPolygon = null;
                foreach (SimplePolygon s in polygonsList)
                {
                    float s_area = 0;//这个多边形的面积
                    int index = 1;
                    var point2dLink = s.point2dLink;
                    while (index < point2dLink.GetSize() - 1)
                    {

                        var a = point2dLink.GetFirst();
                        var b = point2dLink.Get(index);
                        var c = point2dLink.Get(index + 1);

                        //把面积算一下
                        Vector2 ab = b.point - a.point;
                        Vector2 ac = c.point - a.point;
                        float area = MathUtils.Cross_2d(ab, ac) * 0.5f;                      
                        s_area += Math.Abs(area);
                        index++;
                    }
                    if (maxAreaPolygon == null || s_area >= maxArea)
                    {
                        maxAreaPolygon = s;
                        maxArea = s_area;
                    }
                }
            }

            /// <summary>
            /// 矩形？
            /// </summary>
            private bool IsRect(SimplePolygon simplePolygon)
            {
                if (simplePolygon.point2dLink.GetSize() != 4)
                {
                    return false;
                }
                Vector2 p1 = simplePolygon.point2dLink.Get(1).point - simplePolygon.point2dLink.GetFirst().point;
                Vector2 p2 = simplePolygon.point2dLink.Get(2).point - simplePolygon.point2dLink.Get(1).point;
                Vector2 p3 = simplePolygon.point2dLink.Get(3).point - simplePolygon.point2dLink.Get(2).point;
                Vector2 p4 = simplePolygon.point2dLink.GetFirst().point - simplePolygon.point2dLink.Get(3).point;
                if (Vector2.Dot(p1, p2) != 0) return false;
                if (Vector2.Dot(p2, p3) != 0) return false;
                if (Vector2.Dot(p3, p4) != 0) return false;
                if (Vector2.Dot(p4, p1) != 0) return false;
                return true;
            }

            /// <summary>
            /// 删除共线点
            /// </summary>
            private void DelLinePoint()
            {
                foreach (SimplePolygon s in polygonsList)
                {
                    int index = 0;
                    var point2dLink = s.point2dLink;
                    while (index < point2dLink.GetSize())
                    {
                        var b = point2dLink.Get(index);
                        
                        //三点共线时 删除中间的点 
                        if (b.GetSymbol() == PolygonSymbol.Line)                                            
                        {
                            point2dLink.Delete(index);
                            index--;
                        }
                        index++;
                    }
                   
                }
            }

            #endregion




        }


    }

    /// <summary>
    /// 通用Mesh数据结构
    /// </summary>
    public class MeshData
    {
        public Vector3[] vertices;
        public Vector2[] uv;
        public int[] triangles;

        /// <summary>
        /// 模型坐标原点
        /// </summary>
        public Vector3 center = Vector3.zero;

        /// <summary>
        /// 模型旋转值
        /// </summary>
        public Quaternion quaternion;

        public MeshData() { }

        public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles)
        {
            quaternion = Quaternion.identity;

            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
            if (vertices.Length > 1000)
            {
                Debug.LogError("顶点数过大，请检查具体原因" + ToString());

            }
        }

        /// <summary>
        /// 网格合并 将多个MeshData合成一个
        /// </summary>
        public static MeshData Combine(List<MeshData> meshDatas)
        {
            Debug.LogWarning("可优化list和array反复转化");           
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();
            for (int i = 0;i<meshDatas.Count;i++)
            {
                for (int j = 0; j < meshDatas[i].triangles.Length; j++)
                {
                    triangles.Add(meshDatas[i].triangles[j] + vertices.Count);
                }
                for (int j = 0;j<meshDatas[i].vertices.Length;j++)
                {
                    vertices.Add(meshDatas[i].vertices[j]);
                    uv.Add(meshDatas[i].uv[j]);         
                }                           
            }
            return new MeshData(vertices.ToArray(),uv.ToArray(),triangles.ToArray());
        }

        public override string ToString()
        {
            return "->Meshdata:顶点数(" + vertices.Length + "),三角数(" + vertices.Length + "),UV数(" + uv.Length + ")," + base.ToString();
        }
    }

    /// <summary>
    /// 基本几何体构造器 写死的 速度快 简单
    /// </summary>
    public class SimpleMeshCreator
    {

        /// <summary>
        /// 绘制一个简单的6边长方体
        /// </summary>
        /// <param name="size"> x y z表示这个长方体的 长 高 宽 </param>
        public static MeshData DrawSimpleCube(Vector3 size)
        {
            Matrix4x4 matrix4X4 = Matrix4x4.Scale(size);
            /// Vector3.
            Vector3[] vertices = new Vector3[]
            {
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),

            };
            int[] triangles = new int[]
            {
                0,2,3,
                0,3,1,
                8,4,5,
                8,5,9,
                10,6,7,
                10,7,11,
                12,13,14,
                12,14,15,
                16,17,18,
                16,18,19,
                20,21,22,
                20,22,23,
            };
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f)
            };


            MeshData meshdata = new MeshData(vertices, uv, triangles);
            return meshdata;
        }

        /// <summary>
        /// 绘制一个简单的6边长方体
        /// </summary>
        /// <param name="size"> x y z表示这个长方体的 长 高 宽 </param>
        /// <param name="pos"> mesh中心坐标 </param>
        /// <param name="rotate"> mesh 旋转 </param>
        /// <returns></returns>
        public static MeshData DrawSimpleCube(Vector3 size, Vector3 pos, Quaternion rotate)
        {
            Matrix4x4 matrix4X4 = Matrix4x4.TRS(pos, rotate, size);
            Vector3[] vertices = new Vector3[]
            {
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f)),
                matrix4X4.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f)),

            };
            int[] triangles = new int[]
            {
                0,2,3,
                0,3,1,
                8,4,5,
                8,5,9,
                10,6,7,
                10,7,11,
                12,13,14,
                12,14,15,
                16,17,18,
                16,18,19,
                20,21,22,
                20,22,23,
            };
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f)
            };


            MeshData meshdata = new MeshData(vertices, uv, triangles);
            return meshdata;
        }

        /// <summary>
        /// 根据顶点绘制4边形Mesh
        /// </summary>
        /// <param name="pos">顶点坐标</param>
        /// <param name="reverse">是一个反向的面(一个立方体的正面和背面的方向是相反的)</param>
        /// <returns></returns>
        public static MeshData Draw4SideShape(Vector3[] vertices, bool reverse = false)
        {
            int[] triangles;
            if(reverse)
            {
                triangles = new int[] { 0,2,1,0,3,2 };
            }
            else
                triangles = new int[] { 0,1,2,0,2,3 };



            Vector2[] uv = new Vector2[4];

            float minX = vertices[0].x, minY = vertices[0].y, maxX = minX, maxY = minY;
            for(int i = 0;i< vertices.Length;i++)
            {
                if(minX> vertices[i].x)
                {
                    minX = vertices[i].x;
                }
                if (minY > vertices[i].y)
                {
                    minY = vertices[i].y;
                }
                if (maxX< vertices[i].x)
                {
                    maxX = vertices[i].x;
                }              
                if(maxY< vertices[i].y)
                {
                    maxY = vertices[i].y;
                }
            }

            uv = new Vector2[]
            {
                new Vector2((vertices[0].x- minX)/maxX,(vertices[0].y - minY)/maxY),
                new Vector2((vertices[1].x- minX)/maxX,(vertices[1].y - minY)/maxY),
                new Vector2((vertices[2].x- minX)/maxX,(vertices[2].y - minY)/maxY),
                new Vector2((vertices[3].x- minX)/maxX,(vertices[3].y - minY)/maxY),
            };
            
            MeshData miaoMeshdata = new MeshData(vertices, uv, triangles);
            return miaoMeshdata;

        }


        /// <summary>
        /// 根据平面顶点绘制N边柱体Mesh
        /// </summary>
        /// <param name="pos">平面点</param>
        /// <param name="worldDic">柱体朝向</param>
        /// <param name="scale">柱体朝向高度</param>
        /// <param name="center">坐标系原点位置：-1，底部，0中间，1顶部</param>
        /// <param name="rotation">使用模型顶点旋转</param>
        /// <param name="section">横截面</param>
        /// <returns></returns>
        public static MeshData DrawNPrism(Vector3[] pos,Vector3  worldDic, float scale,int center,bool rotation, out Polygon.SimplePolygon section)
        {

            //模型坐标系原点设置
            Vector3 modelZero = Vector3.zero;
            if(center == -1)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    modelZero = modelZero + pos[i];
                }
                modelZero = modelZero / pos.Length;

                for (int i = 0; i < pos.Length; i++)
                {
                    pos[i] = pos[i] - modelZero;
                }
            }
            else if(center == 0)
            {
                modelZero = Vector3.zero;
            }
            else if(center == 1)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    modelZero = modelZero + pos[i];
                }
                modelZero = modelZero / pos.Length;

                for (int i = 0; i < pos.Length; i++)
                {
                    pos[i] = modelZero - pos[i];
                }
            }
           

            //建模方式
            //先摆正：采取pos作为模型底部 模型垂直往上增长的
            //后放回：根据逆矩阵重置模型为原来方向
            Quaternion q = Quaternion.FromToRotation(worldDic, Vector3.up);


            Matrix4x4 matrix4 = Matrix4x4.Rotate(q);
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = matrix4.MultiplyPoint(pos[i]);
            }
            worldDic = Vector3.up;
            


            //分治法 1,计算柱体四周的每一个面 2,计算柱体顶底两个面 3，合并计算好的面

            //一共有pos.len 个面,每个面有4个顶点、2个三角形，每个三角形由3个顶点组成
            Vector3[] vectices = new Vector3[pos.Length * 4];//
            Vector2[] uv = new Vector2[pos.Length * 4];
            int[] tr = new int[pos.Length * 2 * 3];


            //四周
            //记录周围网格合并时 最新的填充位置
            int vec_len = 0, uv_len = 0, tr_len = 0;
            for (int i = 0;i<pos.Length;i++)
            {

                int j = (i == pos.Length - 1)? 0:i+1;                          
                Vector3[] _vectices =
                {
                    
                    pos[i],
                    pos[i] + worldDic * scale,
                    pos[j] + worldDic * scale,
                    pos[j]
                };

                //这里保持时针同步
                //int[] _tr = { 0 + vec_len, 1 + vec_len, 2 + vec_len, 0 + vec_len, 2 + vec_len, 3 + vec_len };

                //这里保持时针相反
                int[] _tr = { 0 + vec_len, 2 + vec_len, 1 + vec_len, 0 + vec_len, 3 + vec_len, 2 + vec_len };

                Vector2[] _uv =
                {
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(1,0),
                    Vector2.zero
                };

                _vectices.CopyTo(vectices, vec_len);
                _uv.CopyTo(uv, uv_len);               
                _tr.CopyTo(tr, tr_len);

                vec_len += _vectices.Length;
                uv_len += _uv.Length;
                tr_len += _tr.Length;
            }
           
            //顶部和底部
            List<Vector2> vertex2dList = new List<Vector2>();
            for (int i = 0; i < pos.Length; i++)
            {
                vertex2dList.Add(new Vector2(pos[i].x, pos[i].z));
            }
            Polygon.SimplePolygon downPolygon = new Polygon.SimplePolygon(vertex2dList);
            downPolygon.Execute();
            Vector2[] downVer2D = downPolygon.GetVertices();
            Vector2[] down_up_UV = downPolygon.GetUV();
            int[] downTr = downPolygon.GetTriangles(true);
            int[] upTr = downPolygon.GetTriangles();

            Vector3[] downVer3D = new Vector3[downVer2D.Length];
            Vector3[] upVer3D = new Vector3[downVer2D.Length];
            for (int i = 0;i<downVer2D.Length;i++)
            {
                downVer3D[i] = new Vector3(downVer2D[i].x, pos[0].y, downVer2D[i].y);
                upVer3D[i] = downVer3D[i] + worldDic * scale;
            }

            //合并
            List<MeshData> meshes = new List<MeshData>();
            meshes.Add(new MeshData(vectices, uv, tr));
            meshes.Add(new MeshData(downVer3D, down_up_UV, downTr));
            meshes.Add(new MeshData(upVer3D, down_up_UV, upTr));
            section = downPolygon;
            MeshData meshData = MeshData.Combine(meshes);

            //摆正位置
            //方式一：使模型内部摆正 物体不用做任何旋转
            if (rotation)
            {
                Vector3[] newVertices = meshData.vertices;
                for (int i = 0; i < newVertices.Length; i++)
                {
                    newVertices[i] = matrix4.MultiplyPoint(newVertices[i]);
                }
                meshData.vertices = newVertices;
            }
            //方式二：模型内部不摆正 但是通过旋转物体来摆正
            else
            {
                matrix4 = matrix4.transpose;
                meshData.quaternion = matrix4.GetRotation();

                
            }

            meshData.center = modelZero;
            return meshData;
        }

        /// <summary>
        /// 根据底部顶点绘制6棱柱Mesh
        /// </summary>
        /// <returns></returns>
        public static MeshData Draw6Prism(Vector3[] pos,float weight)
        {
            Vector3[] left = new Vector3[6];
            left[0] = pos[0];
            left[1] = pos[1];
            left[2] = pos[1] + Vector3.up * weight;
            left[3] = pos[0] + Vector3.up * weight;
            left[4] = pos[5] + Vector3.up * weight;
            left[5] = pos[5];

            int[] left_tr = { 0,1,2,0,2,3,0,3,4,0,4,5 };
            Vector2[] left_uv =
            {
                new Vector2(0,0.5f),
                new Vector2(0,1),
                new Vector2(1,0),
                new Vector2(1,0.5f),
                new Vector2(1,0),
                new Vector2(0,0)
            };

            //右边
            Vector3[] right = new Vector3[6];
            right[0] = pos[3];
            right[1] = pos[4];
            right[2] = pos[4] + Vector3.up * weight;
            right[3] = pos[3] + Vector3.up * weight;
            right[4] = pos[2] + Vector3.up * weight;
            right[5] = pos[2];

            int[] right_tr = { 0,1,2,0,2,3,0,3,4,0,4,5};
            Vector2[] right_uv =
            {
                new Vector2(0,0.5f),
                new Vector2(0,1),
                new Vector2(1,0),
                new Vector2(1,0.5f),
                new Vector2(1,0),
                new Vector2(0,0)
            };

            //正面
            Vector3[] forward = new Vector3[4];
            forward[0] = pos[5];
            forward[1] = pos[4];
            forward[2] = pos[4] + Vector3.up * weight;
            forward[3] = pos[5] + Vector3.up * weight;
            int[] forward_tr = {0,2,1,0,3,2 };
            Vector2[] forward_uv =
            {
                //new Vector2(0,1),
                //new Vector2(1,1),
                //new Vector2(1,0),
                //new Vector2(0,0)
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };

            //hou面
            Vector3[] back = new Vector3[4];
            back[0] = pos[1];
            back[1] = pos[2];
            back[2] = pos[2] + Vector3.up * weight;
            back[3] = pos[1] + Vector3.up * weight;
            int[] back_tr = { 0, 1, 2, 0, 2, 3 };
            Vector2[] back_uv =
            {
                //new Vector2(0,1),
                //new Vector2(1,1),
                //new Vector2(1,0),
                //new Vector2(0,0)
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };

            //底面
            Vector3[] down = pos;
            int[] down_tr = { 0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 5, 4 };
            Vector2[] down_uv =
            {
                new Vector2(0,0.5f),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0.5f),
                new Vector2(1,0),
                new Vector2(0,0)
            };
            //顶面
            Vector3[] up = new Vector3[6];
            for(int i = 0;i<6;i++)
                up[i] = down[i] + Vector3.up * weight;
            int[] up_tr = { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 };
            Vector2[] up_uv =
            {
                new Vector2(0,0.5f),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0.5f),
                new Vector2(1,0),
                new Vector2(0,0)
            };

            Vector3[] vectices = new Vector3[up.Length + down.Length + left.Length + right.Length + forward.Length + back.Length];
            int[] tr = new int[up_tr.Length + down_tr.Length + left_tr.Length + right_tr.Length + forward_tr.Length + back_tr.Length];
            Vector2[] uv = new Vector2[vectices.Length];

            //合并下
            down.CopyTo(vectices, 0);
            //for (int i = 0; i < down_tr.Length; i++)
               // down_tr[i] = down_tr[i] + 0;
            down_tr.CopyTo(tr, 0);
            down_uv.CopyTo(uv, 0);
            
            //合并左
            left.CopyTo(vectices, down.Length);
            for (int i = 0; i < left_tr.Length; i++)
                left_tr[i] = left_tr[i] + down.Length;
            left_tr.CopyTo(tr, down_tr.Length);
            left_uv.CopyTo(uv, down_uv.Length);
            
            //合并正
            forward.CopyTo(vectices, down.Length + left.Length);
            for (int i = 0; i < forward_tr.Length; i++)
                forward_tr[i] = forward_tr[i] + down.Length + left.Length;
            forward_tr.CopyTo(tr,  down_tr.Length + left_tr.Length);
            forward_uv.CopyTo(uv,  down_uv.Length + left_uv.Length);

            //合并上
            up.CopyTo(vectices, forward.Length + left.Length + down.Length);
            for (int i = 0; i < up_tr.Length; i++)
                up_tr[i] = up_tr[i] + forward.Length + left.Length + down.Length;
            up_tr.CopyTo(tr, left_tr.Length + down_tr.Length + forward_tr.Length);
            up_uv.CopyTo(uv, left_uv.Length + down_uv.Length + forward_uv.Length);

            //合并后
            back.CopyTo(vectices, forward.Length + left.Length + down.Length + up.Length);
            for (int i = 0; i < back_tr.Length; i++)
                back_tr[i] = back_tr[i] + forward.Length + left.Length + down.Length + up.Length;
            back_tr.CopyTo(tr, left_tr.Length + down_tr.Length + forward_tr.Length + up_tr.Length);
            back_uv.CopyTo(uv, left_uv.Length + down_uv.Length + forward_uv.Length + up_uv.Length);

            //合并右
            right.CopyTo(vectices, forward.Length + left.Length + down.Length + up.Length + back.Length);
            for (int i = 0; i < right_tr.Length; i++)
                right_tr[i] = right_tr[i] + forward.Length + left.Length + down.Length + up.Length + back.Length;
            right_tr.CopyTo(tr, left_tr.Length + down_tr.Length + forward_tr.Length + up_tr.Length + back_tr.Length);
            right_uv.CopyTo(uv, left_uv.Length + down_uv.Length + forward_uv.Length + up_uv.Length + back_uv.Length);

            /**/
            return new MeshData(vectices,uv,tr);
        }


    }

    
}

