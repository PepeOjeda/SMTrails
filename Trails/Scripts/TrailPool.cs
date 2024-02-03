using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailPool
{
	readonly int POOL_SIZE = 20;
	PooledTrail[] trailPool;

	private static TrailPool _instance;
	public static TrailPool instance{ 
		get{
			if(_instance == null)
				_instance = new TrailPool();
			return _instance;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		_instance = new TrailPool();
	}

	private TrailPool()
	{
		_instance = this;

		trailPool = new PooledTrail[POOL_SIZE];
		for (int i = 0; i < POOL_SIZE; i++)
		{
			trailPool[i] = new PooledTrail();
		}
	}

	public PooledTrail getObject()
	{
		int i = 0;
		while(i<POOL_SIZE)
		{
			if(trailPool[i].isAvailable)
			{
				trailPool[i].isAvailable = false;
				return trailPool[i];
			}
			i++;
		}
		Debug.LogWarning("Pool size exceeded!");
		var o = new PooledTrail();
		o.isAvailable = false;
		return o;
	}

	public class PooledTrail
	{
		public BasicSplineTrail trail{ get; private set; }
		private bool _isAvailable;
		public bool isAvailable
		{ 
			get => _isAvailable; 
			set {
				_isAvailable = value;
				go.SetActive(!_isAvailable);
			} 
		}
		public GameObject go;
		public PooledTrail()
		{
			go = new GameObject();
			go.hideFlags = HideFlags.HideInHierarchy;
            Object.DontDestroyOnLoad(go);
			trail = go.AddComponent<BasicSplineTrail>();
			trail.pooledTrailMaker = this;
			isAvailable = true;
		}

		~PooledTrail()
		{
			UnityEngine.Object.Destroy(go);
		}
	}
}
