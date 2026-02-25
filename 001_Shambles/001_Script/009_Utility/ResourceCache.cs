using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCache<T> where T : Object {
    private static Dictionary<string, T> resources;
	const string errorImagePath = "ErrorImage/9999";
	
    static ResourceCache() {
        resources = new Dictionary<string, T>();
    }

    public static T Load(string path) {
        if (path == null || path == "") return null;
        if (resources == null) return null;

        T obj;
        if (resources.TryGetValue(path, out obj))
            return obj;
        else {
            obj = Resources.Load<T>(path);
			if (obj == null) {
				obj = Resources.Load<T>(errorImagePath);
			}
            if (obj != null) {
                resources.Add(path, obj);
                return resources[path];
            }
            else
                return null;
        }
    }

    public static void Clear() {
        resources.Clear();
        Resources.UnloadUnusedAssets();
    }
}
