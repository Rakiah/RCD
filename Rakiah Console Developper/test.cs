using UnityEngine;
using System.Collections;
using System.Reflection;
using RakiahDevConsole;

public class test : MonoBehaviour 
{
	public void Test ()
	{
		Debug.Log("test");
	}

	[Command("Allow you to write every parameters that you sent it")]
	void Bite (bool repeat_times, ulong ts, float ts1, bool ts2, LogMessage message)
	{
		Debug.Log(repeat_times + " : " + ts + " : " + ts1 + " : " + ts2);
	}

	[Command("set the position of an object")]
	void set1(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set2(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set3(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set134(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set1414(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set2424(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set797(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set86713(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set46248(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set24857(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set6(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void setpos(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set7(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the position of an object")]
	void set5(DCRItem item, Vector3 position)
	{
		Debug.Log(position.ToString());
	}

	[Command("set the rotation of an object")]
	void setrot(float x, float y, float z)
	{
		
	}

	[Command("set the scale of an object")]
	void setscale(float x, float y, float z)
	{
		
	}

	[Command("set the sendrate of an object")]
	void setsendrate(float x, float y, float z)
	{
		
	}

	[Command("testparams")]
	void testparams (float x, float y, float z, params object [] parameters)
	{

	}

	[Command("testautocompletion")]
	void testcompletion (DCRItem obj, DCRMethodsItem behav, MethodInfo method)
	{
		behav.behaviour.Invoke(method.Name, 0.1f);
	}

	[Command("print internal items")]
	void printitems ()
	{
		for(int i = 0; i < DCRManager.LoggerManager.InternalItems.Count; i ++)
		{
			DCRItem item = DCRManager.LoggerManager.InternalItems.Get(i);

			Debug.Log(item.Obj.name + " : " + item.isParent + " : " + i.ToString());
		}
	}
}
