using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using Nito.Collections;

public class BasicSplineTrail : MonoBehaviour
{
	Spline spline;
	SplinePool.PooledSpline splineObject;
	public TrailPool.PooledTrail pooledTrailMaker;

	Deque<float> nodeCreationTimestamps;

	float nodeLifeTime; //for actually deleting the spline nodes

	float fadeTime; // how long in seconds before the shader parameter "t" reaches 1. What t is used for is up to the shader
	float trailSpeed;
	float startT;

	Material mat;
	TrailMaker.SmoothingType smoothingType;
	public void Initialize(
		Transform emitterTransform, 
		TrailMaker.TrailType trailType, 
		float _nodeLifeTime, 
		float _fadetime, float _trailSpeed, 
		Material material, 
		UVMode uvMode, 
		Vector3 meshScale, 
		UnityEngine.Rendering.ShadowCastingMode shadowMode, 
		TrailMaker.SmoothingType _smoothingType)
    {
		if(nodeCreationTimestamps == null)
			nodeCreationTimestamps = new Deque<float>(20);

		splineObject = SplinePool.instance.getObject(trailType);
		spline = splineObject.spline;

		var splineTiling = splineObject.splineMeshTiling;
		splineTiling.uvMode = uvMode;
		splineTiling.originalMaterial = material;
		splineTiling.scale = meshScale;
		splineTiling.shadowMode = shadowMode;

		nodeLifeTime = _nodeLifeTime;
		fadeTime = _fadetime;
		trailSpeed = _trailSpeed;
		smoothingType = _smoothingType;

		//the two mandatory nodes
		spline.nodes[0].Position = emitterTransform.position; 
		spline.nodes[1].Position = emitterTransform.position+0.01f* emitterTransform.forward; 
		
		spline.nodes[0].Up = emitterTransform.up;
		spline.nodes[1].Up = emitterTransform.up;

		
		spline.nodes[0].Direction = spline.nodes[0].Position + (emitterTransform.forward*0.01f);
		spline.nodes[1].Direction = spline.nodes[1].Position + (emitterTransform.forward*0.01f);

		nodeCreationTimestamps.AddToBack(Time.time);
		nodeCreationTimestamps.AddToBack(Time.time);
		startT = Time.time;

		splineTiling.CreateMeshes();
		mat = splineTiling.instancedMaterial.instance;
	}

	public void addPoint(Vector3 position, Vector3 direction, Vector3 up){
		SplineNode node = new SplineNode(position, direction);
		spline.AddNode(node);
		node.Up = up;
		nodeCreationTimestamps.AddToBack(Time.time);
	}

	public void updateMostRecentNode(Vector3 position, Vector3 direction, Vector3 up){
		SplineNode node = spline.nodes[spline.nodes.Count-1];
		node.Position = position;
		node.Direction = direction;
		node.Up = up;

		if(spline.nodes.Count == 2)
		{
			spline.nodes[0].Direction = spline.nodes[0].Position + (node.Position - spline.nodes[0].Position) * 0.02f;
		}
	}

	void Update(){

		if(splineObject!=null){
			removeOldPoints();
			mat.SetFloat("t", (Time.time-startT)/fadeTime);
			mat.SetFloat("distanceT", (Time.time-startT) * trailSpeed);
			
			if(splineObject.splineSmoother && !splineObject.splineSmoother.autoSmoothOnChange)
			{
				if(smoothingType==TrailMaker.SmoothingType.AllNodes)
					splineObject.splineSmoother.SmoothAll();
				else if(smoothingType == TrailMaker.SmoothingType.OnlyLastNode)
					splineObject.splineSmoother.SmoothNodeAndNeighbours(spline.nodes[spline.nodes.Count-1]);
			}
		}
	}

	void removeOldPoints(){
		bool goodPoint = false;
		while(!goodPoint && spline.nodes.Count>2){
			if(Time.time-nodeCreationTimestamps[0]>nodeLifeTime){
				spline.RemoveNode(spline.nodes[0]);
				nodeCreationTimestamps.RemoveFromFront();
			}
			else
				goodPoint = true;
		}

		if(spline.nodes.Count == 2 && Time.time-nodeCreationTimestamps[0]>nodeLifeTime){
			nodeCreationTimestamps.Clear();
			pooledTrailMaker.isAvailable = true;
			splineObject.isAvailable = true;
		}
	}
}
