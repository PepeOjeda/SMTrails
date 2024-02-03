using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class SplinePool
{
	readonly int POOL_SIZE = 20;
	
	private static SplinePool _instance;
	public static SplinePool instance{ 
		get{
			if(_instance == null)
				_instance = new SplinePool();
			return _instance;
		}
	}

	
	List<PooledSpline> linearPool;
	List<PooledSpline> bezierPool;

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		_instance = new SplinePool();
	}

	private SplinePool()
	{
		_instance = this;

		linearPool = new List<PooledSpline>(POOL_SIZE);
		bezierPool = new List<PooledSpline>(POOL_SIZE);

		for (int i = 0; i < POOL_SIZE; i++)
		{
			linearPool.Add( new PooledSpline(TrailTypeData.instance.getPrefab(TrailMaker.TrailType.Linear)) );
			bezierPool.Add( new PooledSpline(TrailTypeData.instance.getPrefab(TrailMaker.TrailType.Bezier)) );
		}
	}

	public PooledSpline getObject(TrailMaker.TrailType trailType)
	{
		int i = 0;
		List<PooledSpline> pool = null;
		if(trailType == TrailMaker.TrailType.Linear)
			pool = linearPool;
		else if (trailType == TrailMaker.TrailType.Bezier)
			pool = bezierPool;
		else
		{
			Debug.LogError($"Trail type not recognized: {trailType}");
			return null;
		}

		while(i<POOL_SIZE)
		{
			if(pool[i].isAvailable)
			{
				pool[i].isAvailable = false;
				return pool[i];
			}
			i++;
		}
		Debug.LogWarning("Pool size exceeded!");
		var o = new PooledSpline(TrailTypeData.instance.getPrefab(trailType));
		pool.Add(o);
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
			gameObject = UnityEngine.Object.Instantiate(prefab);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
			spline = gameObject.GetComponent<Spline>();
			splineMeshTiling = gameObject.GetComponent<SplineMeshTiling>();
			splineSmoother = gameObject.GetComponent<SplineSmoother>();

			gameObject.hideFlags = HideFlags.HideInHierarchy;
			isAvailable = true;
		}
		
		~PooledSpline()
		{
			UnityEngine.Object.Destroy(gameObject);
		}

		void Disable()
		{
			spline.Reset(false);
			gameObject.SetActive(false);
		}
	}
}


