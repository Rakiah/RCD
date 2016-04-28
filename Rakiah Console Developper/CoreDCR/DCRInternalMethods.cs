using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public class DCRInternalMethods
	{
		DCRManager manager;

		public DCRInternalMethods(DCRManager _manager)
		{
			manager = _manager;
		}

		public void Initialize ()
		{
			SetupInternalMethods();
			SetupCustomTypes();
		}

		void SetupInternalMethods ()
		{
			manager.RegisterInternal("clear", this, "Clear the log window");
		}

		void SetupCustomTypes ()
		{
			manager.RegisterCustomType(typeof(Vector3), DeserializeVector3);
		}

		object DeserializeVector3 (string value)
		{
			string [] pos = value.Split('.');
			Vector3 DeserializedValue = new Vector3(0.0f, 0.0f, 0.0f);

			if (pos.Length != 3)
				return null;

			for (int i = 0; i < 3; i++)
			{
				float tmp = 0.0f;
				if (!float.TryParse(pos[i], out tmp))
					return null;
				DeserializedValue[i] = tmp;
			}

			return (object)DeserializedValue;
		}

		public void clear ()
		{
			for (int i = manager.Interface.Logs.Count - 1; i >= 0; i--) 
			{
				manager.Interface.Logs[i].Release();
				manager.Interface.Logs.RemoveAt(i);
			}

			manager.Interface.LogsWindow.CalculateZone(0);
		}
	}
}
