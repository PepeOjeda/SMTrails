using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using Nito.Collections;

public class TrailMaker : MonoBehaviour
{
	public enum TrailType {Linear, Bezier}
	[SerializeField] TrailType trailType = TrailType.Bezier;

	public enum SmoothingType{OnlyLastNode, AllNodes}
	[SerializeField] [Tooltip("Has no effect for linear spline, or if splineSmoother.autoSmoothOnChange is on")]
	SmoothingType smoothingType;

	[SerializeField] float nodeLifeTime = 0.3f;
	[SerializeField] float fadeTime;
	[SerializeField] float emissionFrequency = 0.03f;
	[SerializeField] float trailSpeed;

	[SerializeField] Material material;

	[SerializeField] UVMode uvMode = UVMode.Extend;
	[SerializeField] UnityEngine.Rendering.ShadowCastingMode shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;

	[SerializeField] Vector3 meshScale = new Vector3(1, 1, 1);
	bool _emitting = false;
	public bool emitting
	{
		get { return _emitting; }
		set
		{
			bool shouldEmit = (!emitting && value);
			_emitting = value;
			if(shouldEmit)
				StartCoroutine(emit(emissionFrequency));
		}
	}

	IEnumerator emit(float deltaT)
	{
		TrailPool.PooledTrail trail = TrailPool.instance.getObject();

		BasicSplineTrail splineTrail = trail.trail;
		splineTrail.Initialize(
			transform,
			trailType,
			nodeLifeTime,
			fadeTime,
			trailSpeed,
			material,
			uvMode,
			meshScale,
			shadowMode,
			smoothingType
		);
		Vector3 oldPosition = Vector3.negativeInfinity;
		float timeLastEmission = float.NegativeInfinity;

		while (emitting)
		{ //is stopped by the "emitting" property

			if (Time.time - timeLastEmission > deltaT)
			{
				if (!oldPosition.Equals(Vector3.negativeInfinity))
					splineTrail.addPoint(transform.position,
						transform.position + transform.forward * 0.02f,
						transform.up
					);

				timeLastEmission = Time.time;
			}
			else
			{
				if (!oldPosition.Equals(Vector3.negativeInfinity))
					splineTrail.updateMostRecentNode(transform.position,
						transform.position + transform.forward * 0.02f,
					 	transform.up
					);
			}

			oldPosition = transform.position;
			yield return null;
		}
	}

#if UNITY_EDITOR
	[SerializeField] bool showTrailEmitter = false;
	void OnDrawGizmosSelected()
	{	
		if(!showTrailEmitter)
			return;
		Vector3 half = transform.localToWorldMatrix.MultiplyVector(0.5f* Vector3.up * meshScale.y);
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawAAPolyLine(10f, transform.position - half, transform.position + half);
	}

#endif

}
