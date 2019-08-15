/*
*      
*
*    功能: 多边形中的点
*
*/
using UnityEngine;
using Modeling.Polygon;
using Modeling.CoreUtils;

namespace Modeling
{

    /// <summary>
    /// 多边形中的点 描述点和前继、后继点三者之间的关系
    /// </summary>
    public class PolygonPoint2D
    {
        //坐标
        private Vector2 _point;
        private PolygonSymbol symbol = PolygonSymbol.None;


        public PolygonPoint2D(float x, float y, int index = 0)
        {
            point = new Vector2(x, y);
            this.index = index;
        }
        public PolygonPoint2D(Vector2 p, int index = 0)
        {
            point = p;
            this.index = index;
        }
        public PolygonPoint2D(PolygonPoint2D point2D)
        {
            this.point = point2D.point;
            this.index = point2D.index;
        }
        /// <summary>
        /// 在多边形中的索引
        /// </summary>
        public int index;

        /// <summary>
        /// 坐标值
        /// </summary>
        public Vector2 point
        {
            get
            {
                return _point;
            }
            private set
            {
                _point = value;
            }
        }

        /// <summary>
        /// 计算该点和其他两点组成的三角形的顺序
        /// </summary>
        public PolygonSymbol SetSymbol(PolygonPoint2D last, PolygonPoint2D next)
        {

            Vector2 ab = this.point - last.point;
            Vector2 ac = next.point - last.point;

            // Vector2 ab = last.point - this.point;
            //Vector2 ac = next.point - this.point;
            float v = MathUtils.Cross_2d(ab, ac);
            if (v > 0)
            {
                symbol = PolygonSymbol.Negative;
            }
            else if (v == 0)
            {

                symbol = PolygonSymbol.Line;
            }
            else
            {
                symbol = PolygonSymbol.Positive;
            }

            return symbol;

        }

        /// <summary>
        /// 返回该点在多边形中和相邻2点组成的三角形的顺序
        /// </summary>
        public PolygonSymbol GetSymbol()
        {
            return symbol;
        }

        public override string ToString()
        {
            return ToString("f3");
        }

        public string ToString(string format)
        {
            return "(方向:" + symbol + "," + point.x.ToString(format) + "," + point.y.ToString(format) + ")";
        }
    }
}