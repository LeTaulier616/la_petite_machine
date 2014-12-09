using UnityEngine;

// Inherit from MonoBehavior and take another as T parameter
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	// We'll need to store the current instance of the T class
	private static T _instance;
	// We'll use this to lock threads and avoid multiple thread
	//private static object _lock = new object();
	// This bool will help us know if the app is quitting
	private static bool appIsClosing = false;

	//[ExecuteInEditMode]
	// The public Instance of T, accessible from anywhere
	public static T Instance
	{
		// With an overridden getter method
		get
		{
			// If the app is closing...
			if (_instance == null)
			{
				//... Find out if one already exists in the scene
				_instance = (T) FindObjectOfType(typeof(T));
				// If it's still null, then it doesn't exist. Create it!
				if (_instance == null)
				{
					GameObject newSingleton;
					Object singletonPrefab;

					singletonPrefab = Resources.Load(typeof(T).ToString()) as Object;

					if(!appIsClosing)
					{
						if(singletonPrefab != null)
						{

							newSingleton = Instantiate(Resources.Load(singletonPrefab.name, typeof(GameObject))) as GameObject;
							if(newSingleton.GetComponent<T>() != null)
								_instance = newSingleton.GetComponent<T>();
							newSingleton.name = singletonPrefab.name;

						}
						else
						{
							Debug.Log("Not found!");
							// Create a new GameObject...
							newSingleton = new GameObject();
							//... Add the T component to it
							_instance = newSingleton.AddComponent<T>();
							// Rename it with the T class's name
							newSingleton.name = typeof(T).ToString();
						}
					}
				}
				// Mark it as DontDestroyOnLoad
				DontDestroyOnLoad(_instance);
			}
			// Return the final _instance
			return _instance;
		}
	}
	
	//[ExecuteInEditMode]
	// At start, for all singletons
	public void Start()
	{
		// Mark the existent instant as DontDestroyOnLoad
		DontDestroyOnLoad((T) FindObjectOfType(typeof(T)));
	}

	//[ExecuteInEditMode]
	// When the singleton object is destroyed...
	public void OnDestroy ()
	{
		//... Set the appIsClosing boolean to true
		appIsClosing = true;
	}
}