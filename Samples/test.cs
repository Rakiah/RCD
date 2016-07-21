using UnityEngine;
using System.Collections;
using System.Reflection;
using RakiahConsoleDevelopper;

public class MyCustomType
{
	public float test1;
	public float test2;
	public float test3;
	public float test4;

	public MyCustomType(float x, float y, float z, float w)
	{
		test1 = x;
		test2 = y;
		test3 = z;
		test4 = w;
	}
}

public class test : MonoBehaviour 
{
	// deserialize method for your custom type
	object DeserializeMyCustomType(string value)
	{
		float x = 0.0f;
		float y = 0.0f;
		float z = 0.0f;
		float w = 0.0f;

		string[] pos = value.Split('/');

		if (pos.Length != 4)
			return null;

		if (!float.TryParse(pos[0], out x))
			return (null);
		if (!float.TryParse(pos[1], out y))
			return (null);
		if (!float.TryParse(pos[2], out z))
			return (null);
		if (!float.TryParse(pos[3], out w))
			return (null);

		return (new MyCustomType(x, y, z, w));
	}

	public void Start()
	{
		// add it to logger
		RCDManager.LoggerManager.RegisterCustomType(typeof(MyCustomType), DeserializeMyCustomType);
	}

	public void Test ()
	{
		Debug.Log("Invoked this method from console !");
	}

	[Command("repeat every parameters that you sent it, just a test")]
	void RepeatMe (bool repeat_times, ulong ts, float ts1, bool ts2, LogMessage message)
	{
		Debug.Log(repeat_times + " : " + ts + " : " + ts1 + " : " + ts2);
	}

	[Command("Print Custom Type")]
	void PrintCustom(MyCustomType custom)
	{
		Debug.Log(custom.test1 + " : " + custom.test2 + " : " + custom.test3 + " : " + custom.test4);
	}

	[Command("Set the position of an object with a vector3 (separate components using /)")]
	void SetPosition(RCDItem obj, Vector3 position)
	{
		obj.Obj.transform.position = position;
	}

	[Command("Set the rotation of an object with a vector3 (separate components using /)")]
	void SetRotation(RCDItem obj, Vector3 rotation)
	{
		obj.Obj.transform.localEulerAngles = rotation;
	}

	[Command("Set the scale of an object with a vector3 (separate components using /)")]
	void SetScale(RCDItem obj, Vector3 scale)
	{
		obj.Obj.transform.localScale = scale;	
	}

	[Command("Set the sendrate of Network")]
	void SetSendRate(int value)
	{
		Network.sendRate = value;
		Debug.Log(Network.sendRate);
	}

	[Command("Invoke a specific method Parameter 1 = object Parameter 2 = Component Parameter 3 = Method name")]
	void InvokeMethod (RCDItem obj, RCDMethodsItem behav, MethodInfo method)
	{
		behav.behaviour.Invoke(method.Name, 0.1f);
	}

	[Command("print internal cached items")]
	void PrintItems ()
	{
		for(int i = 0; i < RCDManager.LoggerManager.InternalItems.Count; i ++)
		{
			RCDItem item = RCDManager.LoggerManager.InternalItems.Get(i);

			Debug.Log(item.Obj.name + " : " + item.isParent + " : " + i.ToString());
		}
	}
}
