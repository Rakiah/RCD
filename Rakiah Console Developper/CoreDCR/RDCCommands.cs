using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{

	/// <summary> Attribute to identify command method </summary>
	public class CommandAttribute : System.Attribute 
	{
		public string Description;
		public CommandAttribute (string desc)
		{
			Description = desc;
		}
	}

	internal class Command
	{
		/// <summary> hold the name of the command (which is the call accesor) </summary>
		public string 							Name;

		/// <summary> hold the descriptions of the command (will hold only if the command was added directly through code) </summary>
		public string							Description;

		/// <summary> hold if the command have as last paramater a ConsoleMessage type, if so send it when launching </summary>
		public bool								HaveLogParam;

		/// <summary> hold the types of parameters </summary>
		private List<System.Type> 				Parameters 
		= new List<System.Type>();

		/// <summary> hold the names of the parameters (for descriptions purposes) </summary>
		private List<string>					ParametersName
		= new List<string>();

		/// <summary> add a param to both lists (dictionnary like, but for order purpose we need two lists). </summary>
		public void AddParam (System.Type param, string paramName)
		{
			Parameters.Add(param);
			ParametersName.Add(paramName);
		}

		/// <summary> remove a param to both lists (dictionnary like, but for order purpose we need two lists). </summary>
		public void RemoveParam (int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= Parameters.Count)
				return;

			Parameters.RemoveAt(paramIndex);
			ParametersName.RemoveAt(paramIndex);
		}

		/// <summary> update the name of a parameter. </summary>
		public void UpdateParamName (string paramName, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= Parameters.Count)
				return;

			ParametersName[paramIndex] = paramName;
		}

		/// <summary> get a specific param type by index, can return null if wrong index. </summary>
		public System.Type GetParamType (int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= Parameters.Count)
				return null;
			return Parameters[paramIndex];
		}

		/// <summary> get a specific param name by index, can return null if wrong index. </summary>
		public string GetParamName (int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= ParametersName.Count)
				return null;
			return ParametersName[paramIndex];
		}

		/// <summary> get every params type </summary>
		public List<System.Type> GetParamsType ()
		{
			return Parameters;
		}

		/// <summary> get every params name </summary>
		public List<string> GetParamsName ()
		{
			return ParametersName;
		}

		/// <summary> get the count of params </summary>
		public int ParamCount
		{
			get 
			{
				return Parameters.Count;
			}
		}

		/// <summary> method dictionnary holding the method with its instance destination </summary>
		public Dictionary<object, MethodInfo> 	Method;

		public Command (string _Name)
		{
			Name = _Name;
			Method = new Dictionary<object, MethodInfo>();
		}

		public Command (string _Name, string _Description)
		{
			Name = _Name;
			Description = _Description;
			Method = new Dictionary<object, MethodInfo>();
		}
	}

	internal class RDCCommands 
	{
		///----------Members Part-------------///

		/// <summary> Manager for the other core components </summary>
		private RDCManager							Manager;

		/// <summary> 
		/// Cached Methods (back end dictionary), <Methods> should be called instead,
		/// this one doesnt ensure that methods are already initialized
		/// </summary>
		private Dictionary<string, Command> 		CacheMethods 
		= new Dictionary<string, Command>();

		/// <summary> Internal dictionary for the default commands </summary>
		private Dictionary<string, Command> 		InternalMethods
		= new Dictionary<string, Command>();

		/// <summary> Check if the Dictionnary is initialized or not </summary>
		private bool 								Initialized;

		/// <summary> The count of the internal commands </summary>
		internal int								InternalCommandsCount
		{
			get
			{
				return InternalMethods.Count;
			}
		}

		/// <summary> 
		/// Front end version of CacheMethods, it always ensure that the dictionnary
		/// is correctly initialized and ready to use
		/// </summary>
		public Dictionary<string, Command>			Methods
		{
			get
			{
				if (!Initialized) 
					InitializeMethods(); 
				return CacheMethods;
			}
		}

		/// <summary> The count of commands </summary>
		internal int 								Count 
		{
			get
			{
				return Methods.Count; 
			}
		}

		///------------Methods Part-------------///



		public RDCCommands (RDCManager manager)
		{
			Manager = manager;
		}
	

		/// <summary>
		/// initialize the methods this should be run on startup
		/// because it is a costly operations (especially if you have dozens of MonoBehaviour).	
		/// </summary>
		internal void InitializeMethods ()
		{
			if (Initialized)
				ClearCommands();

			SetInternalMethods();
			SetCustomMethods();

			Initialized = true;
		}

		/// <summary> private method to get the internal commands stored in InternalMethods Dictionary </summary>
		private void SetInternalMethods ()
		{
			foreach(string str in InternalMethods.Keys)
				CacheMethods.Add(str, InternalMethods[str]);
		}

		/// <summary> lookup for every custom methods </summary>
		internal void SetCustomMethods ()
		{
			GameObject[] pAllObjects = (GameObject []) Object.FindObjectsOfType(typeof(GameObject));
			
			foreach(GameObject pObject in pAllObjects)
			{
				if (pObject.hideFlags == HideFlags.NotEditable ||
				    pObject.hideFlags == HideFlags.HideAndDontSave ||
				    pObject.hideFlags == HideFlags.HideInHierarchy ||
				    pObject.hideFlags == HideFlags.HideInInspector ||
				    pObject.hideFlags == HideFlags.DontSave)
					continue;
				
				MonoBehaviour [] temp = pObject.GetComponents<MonoBehaviour>();
				
				foreach(MonoBehaviour behavior in temp)
					FindMethods(behavior);
			}
		}

		/// <summary> called when a new level was loaded </summary>
		internal void ClearCommands ()
		{
			CacheMethods.Clear();
			Initialized = false;
		}

		/// <summary>
		/// Find all methods in MonoBehaviour types check for an [Command]
		/// attribute and adds the method information to the CacheMethods.
		/// </summary>
		internal void FindMethods (MonoBehaviour behavior)
		{
			foreach(MethodInfo methodInfo in behavior.GetType().GetMethods(BindingFlags.Public |
			                                                     		   BindingFlags.NonPublic |
			                                                     		   BindingFlags.DeclaredOnly |
			                                                     		   BindingFlags.Instance |
			                                                     		   BindingFlags.Static))
			{
				CommandAttribute attribute = HasAttribute (methodInfo);
				if (attribute == null) continue;
				if (CacheMethods.ContainsKey(methodInfo.Name)) 
					AddExisting(methodInfo, (object)behavior);
				else 
					AddNew (methodInfo, (object)behavior, true, attribute.Description);
			}
		}

		/// <summary> check if the method as the [Command] attribute </summary>
		private CommandAttribute HasAttribute(MemberInfo member) 
		{
			foreach (object attribute in member.GetCustomAttributes(true))
			{
				if (attribute is CommandAttribute)
				{
					CommandAttribute attr = (CommandAttribute)attribute;
					return attr;
				}
			}
			
			return null;
		}

		/// <summary> add a new command </summary>
		private void AddNew(MethodInfo method, object instance, bool custom, string description = "") 
		{
			ParameterInfo[] parameters = method.GetParameters();

			Command command = new Command(method.Name, description);
			command.Method.Add(instance, method);
			
			foreach (ParameterInfo parameter in parameters)
				command.AddParam(parameter.ParameterType, parameter.Name);

			// check if the command have the consoleMessage parameters, if yes remove it and check the HaveLogParam box
			if (command.GetParamType(command.ParamCount - 1) == typeof(LogMessage))
			{
				command.HaveLogParam = true;
				command.RemoveParam(command.ParamCount - 1);
			}
			
			if (custom) CacheMethods.Add(command.Name, command);
			else InternalMethods.Add(command.Name, command);
		}

		/// <summary> Add to an already existing command a method with its instance </summary>
		private void AddExisting(MethodInfo method, object instance)
		{
			Command command = CacheMethods[method.Name];
			ParameterInfo[] paramArray = method.GetParameters();
			
			if (paramArray.Length != command.ParamCount)
				throw new UnityException("Duplicate RPC method with incompatible param count: " + method.Name);
			if (method.ReturnType != command.Method.Values.First().ReturnType)
				throw new UnityException("Duplicate RPC method with incompatible return type: " + method.Name);
			for (int i = 0; i < paramArray.Length; i++) 
				if (paramArray[i].ParameterType != command.GetParamType(i))
					throw new UnityException("Duplicate RPC method with incompatible param types: " + method.Name);
			
			command.Method.Add(instance, method);
		}

		/// <summary> Add an internal method (string) </summary>
		internal void AddInternal (string methodName, object instance, bool custom, string description = "")
		{
			MethodInfo method = instance.GetType().GetMethod(methodName);
			if (method == null)
			{
				Debug.Log("command not found on the behavior " + methodName + " " + instance.GetType().Name);
				return;
			}
			AddInternal(method, instance, custom, description);
		}

		/// <summary> Add an internal method (MethodInfo) </summary>
		internal void AddInternal (MethodInfo method, object instance, bool custom, string description)
		{
			if (InternalMethods.ContainsKey(method.Name))
				AddExisting(method, instance);
			else
				AddNew(method, instance, custom, description);
		}

		/// <summary> Add a description to an existing command </summary>
		internal void AddDescription (string methodName, string description)
		{
			if (!CacheMethods.ContainsKey(methodName)) return;
			CacheMethods[methodName].Description = description;
		}

		internal void AddParamsName (List<string> paramNames, string commandName)
		{
			if (!Methods.ContainsKey(commandName))
				return;

			Command command = Methods[commandName];

			if (paramNames.Count != command.ParamCount)
				return;

			for (int i = 0; i < paramNames.Count; i++)
				command.UpdateParamName(paramNames[i], i);
		}

		/// <summary> command parser method </summary>
		internal void ParseCommand (string cmd)
		{
			string [] parsedCmd = cmd.Split(' ');
			int i;

			string cmdName = parsedCmd[0];

			string parameters = "";
			for (i = 1; i < parsedCmd.Length; i++)
				parameters += parsedCmd[i] + " ";

			if (!CacheMethods.ContainsKey(cmdName))
			{
				Manager.RegisterCommand(cmdName, parameters, "Unknown command");
				return;
			}

			Command command = CacheMethods[cmdName];

			if (command.ParamCount != parsedCmd.Length - 1)
			{
				Manager.RegisterCommand(cmdName, parameters, "Wrong params count");
				return;
			}

			int count = command.ParamCount;
			if (command.HaveLogParam)
				count++;

			object [] paramsobj = new object[count];
			RDCItem item = null;
			RDCMethodsItem behavior = null;

			for (i = 0; i < command.ParamCount; i++)
			{
				System.Type type = command.GetParamType(i);

				if (type == typeof(RDCMethodsItem))
				{
					paramsobj[i] = Manager.Serializer.DeserializeMethodItem(parsedCmd[i + i], item);
					behavior = (RDCMethodsItem)paramsobj[i];
				}

				else if (type == typeof(MethodInfo))
				{
					paramsobj[i] = Manager.Serializer.DeserializeMethod(parsedCmd[i + 1], behavior);
				}

				else
				{
					paramsobj[i] = Manager.Serializer.Deserialize(command.GetParamType(i), parsedCmd[i + 1]);
					if (type == typeof(RDCItem))
						item = (RDCItem)paramsobj[i];
				}

				if (paramsobj[i] == null)
				{
					Manager.RegisterCommand(cmdName, parameters, "Wrong params");
					return;
				}
			}

			LogMessage log = Manager.RegisterCommand(cmdName, parameters, command.HaveLogParam ? "Waiting" : "Succesfull");

			if (command.HaveLogParam)
				paramsobj[paramsobj.Length - 1] = log;

			Launch (cmdName, paramsobj);
			Manager.CommandLaunched(log);
		}

		/// <summary> Launch a command </summary>
		private void Launch (string methodName, object[] parameters)
		{
			Command command = Get(methodName);

			foreach(object instance in command.Method.Keys)
			{
				MethodInfo method = command.Method[instance];

				if (method.ReturnType == typeof(IEnumerator))
				{
					IEnumerator coroutine = (IEnumerator)method.Invoke(instance, parameters);
					MonoBehaviour behaviour = (MonoBehaviour)instance;
					if (coroutine != null) 
						behaviour.StartCoroutine(coroutine);
				
				}
				else method.Invoke(instance, parameters);
			}
		}
		
		/// <summary> Returns true if the methodName is in the dictionnary. </summary>
		internal bool Exists(string methodName) 
		{
			return Methods.ContainsKey(methodName);
		}

		/// <summary> Get a specific command </summary>
		internal Command Get(string methodName) 
		{
			return Methods[methodName];
		}
		
		/// <summary> Returns a List of parameter types for the corresponding command. </summary>
		internal List<System.Type> ParamatersType(string commandName) 
		{
			return Methods[commandName].GetParamsType();
		}

		/// <summary> Returns a List of parameter names for the corresponding command. </summary>
		internal List<string> ParametersName(string commandName)
		{
			return Methods[commandName].GetParamsName();
		}

		/// <summary> print commands name (debug purpose) </summary>
		internal void PrintCommands ()
		{
			foreach (Command command in Methods.Values)
				Debug.Log("RDC : " + command.Name);
		}

	}
}
