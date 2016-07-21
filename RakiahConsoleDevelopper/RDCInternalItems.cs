using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public class RDCInternalItems
	{
		///----------Members Part-------------///

		/// <summary> Back end version of Items call Items instead </summary>
		private List<RDCItem> 			CacheItems
		= new List<RDCItem>();

		/// <summary> Check if the List is initialized or not </summary>
		private bool 				Initialized;

		/// <summary> 
		/// Front end version of CacheItems, it always ensure that the list
		/// is correctly initialized and ready to use
		/// </summary>
		internal List<RDCItem> 			Items
		{
			get
			{
				if (!Initialized) 
					InitializeItems(); 
				return CacheItems;
			}
		}
		
		/// <summary> The count of items </summary>
		internal int 				Count 
		{
			get
			{
				return Items.Count; 
			}
		}

		///------------Methods Part-------------///

		///<summary> Initialize the list of items </summary>
		internal void InitializeItems ()
		{
			if (Initialized)
				ClearItems();

			GameObject[] pAllObjects = (GameObject []) Object.FindObjectsOfType(typeof(GameObject));
			
			foreach (GameObject pObject in pAllObjects)
			{
				if (pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave 
				    || pObject.hideFlags == HideFlags.HideInHierarchy || pObject.hideFlags == HideFlags.HideInInspector || pObject.hideFlags == HideFlags.DontSave)
				{
					continue;
				}
				
				RDCItem item = new RDCItem (pObject);
				CacheItems.Add(item);
			}

			Initialized = true;
		}

		/// <summary> Get a specific RDCItem with an object as key </summary>
		internal RDCItem Get (int id)
		{
			if (id >= 0 && id < Items.Count)
				return Items[id];
			return null;
		}

		/// <summary> Clear the list of items </summary>
		internal void ClearItems ()
		{
			Items.Clear();

			Initialized = false;
		}
	}

	internal class RDCItem
	{
		/// <summary> item that the behaviours are attached </summary>
		public GameObject 								Obj;

		/// <summary> is the item a parent ? </summary>
		public bool 									isParent;

		/// <summary> list containing all the behaviours </summary>
		public Dictionary<string, RDCMethodsItem> 		components
		= new Dictionary<string, RDCMethodsItem> ();

		public RDCItem (GameObject _obj)
		{
			Obj = _obj;
			if(_obj.transform.parent == null) 
				isParent = true;
			
			MonoBehaviour [] temp = _obj.GetComponents<MonoBehaviour>();
			
			foreach(MonoBehaviour behavior in temp)
			{
				// check if the behaviour component actually come from the Unity Engine 
				// (we want to serialize only methods that come from our behaviours)
				string behaviorNamespace = behavior.GetType().Namespace;
				if (behaviorNamespace != null)
					if (behaviorNamespace.Contains("UnityEngine")) 
						continue;
				components.Add(behavior.GetType().Name, new RDCMethodsItem(behavior.GetType().GetMethods(	BindingFlags.Public |
			                                                                     					BindingFlags.Default |
			                                                                     					BindingFlags.DeclaredOnly |
			                                                                     					BindingFlags.Instance), behavior));
			}
		}
	}

	internal class RDCMethodsItem
	{
		/// <summary> Name of the behaviour </summary>
		public string 							nameBehaviour;

		/// <summary> Instance of the behaviour </summary>
		public MonoBehaviour 					behaviour;

		/// <summary> Dictionnary lookup to find Methods on this behaviour </summary>
		public Dictionary<string, MethodInfo> 	MethodLookup
		= new Dictionary<string, MethodInfo>();

		public RDCMethodsItem (MethodInfo [] methods, MonoBehaviour Item)
		{
			behaviour = Item;
			nameBehaviour = Item.GetType().Name;

			foreach(MethodInfo method in methods)
			{
				if (!MethodLookup.ContainsKey(method.Name)) 
					MethodLookup.Add(method.Name, method);
			}
		}
	}
}
