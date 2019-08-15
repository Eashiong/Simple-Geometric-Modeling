
/*
*
*    功能: 参数化建模
*
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modeling.Polygon;


namespace Modeling
{
    /// <summary>
    /// 参数化建模
    /// </summary>
    public class ParameterModeling
    {
        /// <summary>
        /// 生成的游戏对象
        /// </summary>
        public GameObject body;


        private SimplePolygon section;

        /// <summary>
        /// 横截面
        /// </summary>
        public SimplePolygon Section
        {
            get
            {
                return section;
            }

        }



        private Vector3[] pos;

        /// <summary>
        /// 设置平面
        /// </summary>
        public Vector3[] Pos
        {
            get
            {
                return pos;
            }
            set
            {
                if (value == null || value.Length < 3)
                {
                    throw new System.NullReferenceException("平面坐标不能设置为Null或长度小于3");
                }
                else
                    pos = value;
            }
        }

        /// <summary>
        /// 在平面上生成模型时，该值为模型的生长方向
        /// </summary>
        public Vector3 WorldDir { get; set; }

        /// <summary>
        /// 材质
        /// </summary>
        public Material Mat { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public float Height { get; set; }

        private int center;
        /// <summary>
        /// 模型坐标系位置 只能设置如下值:-1(底部),0(中间),1（顶部）
        /// </summary>
        public int Center
        {
            get
            {
                return center;
            }
            set
            {
                if (value != -1 && value != 0 && value != 1)
                {
                    throw new System.InvalidProgramException("只能设置如下值:-1(底部),0(中间),1（顶部）");
                }
                else
                    center = value;
            }
        }

        /// <summary>
        /// 模型顶点旋转
        /// </summary>
        public bool Rotation { get; set; }

        /// <summary>
        /// 根据参数生成模型
        /// </summary>
        /// <param name="pos">平面坐标</param>
        /// <param name="worldDir">法线方向</param>
        /// <param name="material">模型材质</param>
        /// <param name="height">模型高度</param>
        /// <param name="center">模型中心位置 -1，底部，0中间，1顶部 </param>
        /// <param name="rotation">True局部坐标系旋转 False世界坐标系旋转</param>
        public ParameterModeling(Vector3[] pos, Vector3 worldDir, Material material, float height = 1, int center = -1, bool rotation = true)
        {

            this.body = new GameObject("Parameter Model");

            Pos = pos;
            WorldDir = worldDir;
            Mat = material;
            Height = height;
            Center = center;
            Rotation = rotation;



        }


        /// <summary>
        /// 开始根据参数修改模型 只修改内部网格 不会删除原先gameobject
        /// </summary>
        /// <param name="pos">平面坐标</param>
        /// <param name="worldDic">法线方向</param>
        /// <param name="scale">高度</param>
        public void Building()
        {

            MeshData meshData = SimpleMeshCreator.DrawNPrism(Pos, WorldDir, Height, Center, false, out section);

            body.transform.position = meshData.center;
            body.transform.rotation = meshData.quaternion;
            Mesh mesh = new Mesh();
            mesh.vertices = meshData.vertices;
            mesh.triangles = meshData.triangles;
            mesh.uv = meshData.uv;

            mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter mf;
            MeshRenderer mr;
            if (body.GetComponent<MeshFilter>() == null)
            {
                mf = body.AddComponent<MeshFilter>();
            }
            else
            {
                mf = body.GetComponent<MeshFilter>();
            }
            if (body.GetComponent<MeshRenderer>() == null)
            {
                mr = body.AddComponent<MeshRenderer>();
            }
            else
            {
                mr = body.GetComponent<MeshRenderer>();
            }

            mr.material = Mat;
            mf.mesh = mesh;

        }

    }
}