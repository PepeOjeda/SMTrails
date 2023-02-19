using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailPool : MonoBehaviour
{
	[SerializeField] int poolSize;
	PooledTrail[] trailPool;

	public static TrailPool instance;

	void Awake()
	{
		if(instance && instance != this)
		{
			Destroy(instance);
		}
		instance = this;

		trailPool = new PooledTrail[poolSize];
		for (int i = 0; i < poolSize; i++)
		{
			trailPool[i] = new PooledTrail();
		}
	}

	public PooledTrail getObject()
	{
		int i = 0;
		while(i<poolSize)
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
			trail = go.AddComponent<BasicSplineTrail>();
			trail.pooledTrailMaker = this;
			isAvailable = true;
		}

		~PooledTrail()
		{
			Destroy(go);
		}
	}
}
