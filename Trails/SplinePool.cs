using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class SplinePool : MonoBehaviour
{
	[SerializeField] GameObject linearTrailPrefab;
	[SerializeField] GameObject bezierTrailPrefab;
	public static SplinePool instance;

	
	[SerializeField] int poolSize;
	PooledSpline[] linearPool;
	PooledSpline[] bezierPool;

	void Awake()
	{
		if(instance && instance != this)
		{
			Destroy(instance);
		}
		instance = this;

		linearPool = new PooledSpline[poolSize];
		bezierPool = new PooledSpline[poolSize];

		for (int i = 0; i < poolSize; i++)
		{
			linearPool[i] = new PooledSpline(linearTrailPrefab);
			bezierPool[i] = new PooledSpline(bezierTrailPrefab);
		}
	}

	public PooledSpline getObject(TrailMaker.TrailType trailType)
	{
		int i = 0;
		PooledSpline[] pool = null;
		GameObject prefab = null;
		if(trailType == TrailMaker.TrailType.Linear)
		{
			pool = linearPool;
			prefab = linearTrailPrefab;
		}
		else if (trailType == TrailMaker.TrailType.Bezier)
		{
			pool = bezierPool;
			prefab = bezierTrailPrefab;
		}
		else
		{
			Debug.LogError($"Trail type not recognized: {trailType}");
			return null;
		}

		while(i<poolSize)
		{
			if(pool[i].isAvailable)
			{
				pool[i].isAvailable = false;
				return pool[i];
			}
			i++;
		}
		Debug.LogWarning("Pool size exceeded!");
		var o = new PooledSpline(prefab);
		o.isAvailable = false;
		return o;
	}	

	public class PooledSpline
	{
		public Spline spline;
		public SplineMeshTiling splineMeshTiling;
		public SplineSmoother splineSmoother;
		private bool _isAvailable;
		public bool isAvailable
		{ 
			get => _isAvailable; 
			set {
				_isAvailable = value;
				if(_isAvailable)
					Disable();
				else
					gameObject.SetActive(true);
			} 
		}

		public GameObject gameObject;


		public PooledSpline(GameObject prefab)
		{
			gameObject = Instantiate(prefab);
			spline = gameObject.GetComponent<Spline>();
			splineMeshTiling = gameObject.GetComponent<SplineMeshTiling>();
			splineSmoother = gameObject.GetComponent<SplineSmoother>();

			gameObject.hideFlags = HideFlags.HideInHierarchy;
			isAvailable = true;
		}
		
		~PooledSpline()
		{
			Destroy(gameObject);
		}

		void Disable()
		{
			spline.Reset(false);
			gameObject.SetActive(false);
		}
	}
}


