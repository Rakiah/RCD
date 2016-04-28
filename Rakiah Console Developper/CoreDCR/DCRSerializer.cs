using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public class DCRSerializer
	{
		internal Dictionary<Type, Func<string, object>> Deserializers =
		new Dictionary<Type, Func<string, object>>();

		internal DCRManager 							manager;

		public DCRSerializer (DCRManager _manager)
		{
			manager = _manager;
		}

		public void Add (Func<string, object> deserializer, Type type)
		{
			if (Deserializers.ContainsKey(type)) return;

			Deserializers.Add(type, deserializer);
		}

		public void Remove (Type type)
		{
			if(Deserializers.ContainsKey(type)) Deserializers.Remove(type);
		}

		public void RemoveAll ()
		{
			Deserializers.Clear();
		}

		public bool TypeExist (Type type)
		{
			return Deserializers.ContainsKey(type);
		}

		internal object Deserialize (Type type, string value)
		{
			if (type == typeof(int))
			{
				int tmp = 0;
				if (!int.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(float))
			{
				float tmp = 0f;
				if (!float.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(bool))
			{
				bool tmp = false;
				if (!bool.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(uint))
			{
				uint tmp = 0;
				if (!uint.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(short))
			{
				short tmp = 0;
				if (!short.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(ushort))
			{
				ushort tmp = 0;
				if (!ushort.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(long))
			{
				long tmp = 0;
				if (!long.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(ulong))
			{
				ulong tmp = 0;
				if (!ulong.TryParse(value, out tmp))
					return null;
				return (object)tmp;
			}

			if (type == typeof(DCRItem))
			{
				int tmp = -1;
			   	if (!int.TryParse(value, out tmp))
					return null;
				if (tmp >= 0 && tmp < manager.InternalItems.Items.Count)
					return (object)manager.InternalItems.Items[tmp];
				return null;
			}

			if (TypeExist(type))
				return Deserializers[type](value);

			Debug.LogError ("No possible deserializer method for this type, add one before using this");
			return null;
		}

		internal object DeserializeMethodItem (string value, DCRItem item)
		{
			if (item == null)
				return null;

			if (item.components.ContainsKey(value))
				return (object)item.components[value];
			return null;
		}

		internal object DeserializeMethod (string value, DCRMethodsItem behaviour)
		{
			if (behaviour == null)
				return null;

			if (behaviour.MethodLookup.ContainsKey(value))
				return (object)behaviour.MethodLookup[value];
			return null;
		}
	}
}
