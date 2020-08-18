using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
namespace MOR.Industries
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _instanceFound = false;
        private static T _instance;
        private static object _lock = new object();
        public static List<Type> FindObjectsOfTypeAll<Type>()
        {
            List<Type> results = new List<Type>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                SceneManager.GetSceneAt(i).GetRootGameObjects().ToList().ForEach(g => results.AddRange(g.GetComponentsInChildren<Type>(true)));
            }
            return results;
        }

        public static Type FindObjectOfTypeAll<Type>()
        {
            List<Type> objects = FindObjectsOfTypeAll<Type>();
            if (objects.Count > 0)
            {
                return objects[0];
            }
            else
            {
                return default(Type);
            }
        }
        public static T instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    if (!Application.isPlaying) {
                        applicationIsQuitting = false;
                    } else {
                        Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                         "' already destroyed on application quit." +
                                         " Won't create again - returning null.");
                        return null;
                    }
                }

                lock (_lock)
                {
                    if (!_instanceFound)
                    {

                        //NWUtils.LogStacktrace("Init instance of " + typeof(T));

                        //note that FindObjectsOfTypeAll is quite slow! But FindObjectsOfType doesn't find instances on disabled objects
                        List<T> instances = FindObjectsOfTypeAll<T>();
                        if (instances.Count > 0)
                        {
                            _instance = (T)instances[0];//Resources.FindObjectsOfTypeAll(typeof(T))[0];
                        }

                        if (instances.Count > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopening the scene might fix it.");
                            _instanceFound = true;
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();
                            _instanceFound = true;

                            DontDestroyOnLoad(singleton);

                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singleton +
                                "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            _instanceFound = true;
                            //Debug.Log("[Singleton] " + typeof(T).ToString() + " using instance already created: " + _instance.gameObject.name);
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool InstanceNull => _instance == null;
        
        private static bool applicationIsQuitting = false;
        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        virtual public void OnDestroy()
        {
            _instanceFound = false;
            applicationIsQuitting = true;
        }
    }
}