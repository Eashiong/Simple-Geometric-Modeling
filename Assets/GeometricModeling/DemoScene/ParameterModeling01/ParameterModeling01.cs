using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modeling;
public class ParameterModeling01 : MonoBehaviour {

	
    public Camera rayCamera; 

    public Material modelMat;

    //绘制平面多边形的点
    private List<Transform> pointObjs;


    private GameObject model;

    void Start () {

        pointObjs = new List<Transform>();
        
	}
	
	void Update ()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out hit))
            {
                Transform sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                sphere.localScale = Vector3.one * 0.5f;
                sphere.position = hit.point;
                pointObjs.Add(sphere);
            }
        }


        if(Input.GetKeyDown(KeyCode.Space))
        {
            
            Vector3[] points = new Vector3[pointObjs.Count];
            for(int i = 0;i< pointObjs.Count;i++)
            {
                points[i] = pointObjs[i].position;
            }
            if (Modeling.CoreUtils.MathUtils.PointsInSamePlane(points) == false)
            {
                Debug.LogError("多边形不共面，生成的模型是错误的");
                // Clear();
                //return;
            }
            //Vector3.Cross 取同一个面中的两个相交向量 求法线(模型生长方向) 调换参数位置则方向相反
            ParameterModeling modeling = new ParameterModeling(points, Vector3.Cross(points[0] - points[1], points[0] - points[2]), modelMat, 5, 0, false);
            modeling.Building();
            model = modeling.body;
            model.name = "New Modeling";

        }
		
        
	}


    private void Clear()
    {
        if (model)
            Destroy(model);
        for (int i = 0; i < pointObjs.Count; i++)
        {
            if (pointObjs[i])
                Destroy(pointObjs[i].gameObject);
        }
        pointObjs.Clear();
    }
    private void OnGUI()
    {
        GUILayout.Label("鼠标左键，按多边形的顺序或逆序在红色面板上绘制点");
        GUILayout.Label("空格键，生成模型");
        if (GUILayout.Button("重新绘制",GUILayout.Width(150), GUILayout.Height(70)))
        {
            Clear();
        }
    }
}
