using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName ="SMTrails/TrailTypeData")]
public class TrailTypeData :ScriptableObject
{
	private static TrailTypeData _instance;
	public static TrailTypeData instance
	{
		get{
			if(_instance == null)
				_instance = FindObjectOfType(typeof(TrailTypeData)) as TrailTypeData;

			return _instance;
		}
	}
	private static event Action refreshInstance;

	[SerializeField] GameObject bezierPrefab;
	[SerializeField] GameObject linearPrefab;

	void OnEnable()
	{
		if(_instance != null && _instance != this)
		{
			Debug.LogError("More than one instance of TrailTypeData in the project!");
			return;
		}

#if UNITY_EDITOR
		var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
		if(!preloadedAssets.Contains(this))
        	preloadedAssets.Add(this);
        UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
#endif
		_instance = this;

	}

	public GameObject getPrefab(TrailMaker.TrailType type)
	{
		if(type == TrailMaker.TrailType.Bezier)
			return bezierPrefab;
		else if (type == TrailMaker.TrailType.Linear)
			return linearPrefab;
		else
		{
			Debug.LogError($"Trail type {type} is not registered in the TrailTypeData scriptableObject");
			return null;
		}
	}

	void OnDestroy()
	{
		_instance = null;
#if UNITY_EDITOR
		var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
		if(preloadedAssets.Contains(this))
        	preloadedAssets.Remove(this);
        UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
#endif
	}
}