using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public class RDCManager : MonoBehaviour 
	{
		public static RDCManager	LoggerManager;

		internal RDCInternalItems 	InternalItems;

		internal RDCInternalMethods	InternalMethods;

		internal RDCCommands		Commands;

		internal RDCInterface		Interface;

		internal RDCSerializer		Serializer;

		public delegate void OnCommandLaunchedEvent(LogMessage log);
		/// <summary> Fired after a command was succesfully launched </summary>
		public event OnCommandLaunchedEvent OnCommandLaunched;

		public delegate void OnMessageLoggedEvent(LogMessage log);
		/// <summary> Fired after any message was succesfully logged </summary>
		public event OnMessageLoggedEvent OnMessageLogged;

		void Awake ()
		{
			LoggerManager = this;

			Interface = GetComponent<RDCInterface>();

			Commands = new RDCCommands(this);
			InternalItems = new RDCInternalItems();
			InternalMethods = new RDCInternalMethods(this);
			Serializer = new RDCSerializer(this);

			InternalMethods.Initialize();
			Commands.InitializeMethods();
			InternalItems.InitializeItems();
			Interface.Initialize(this);

			DontDestroyOnLoad(this);
		}

		void Start ()
		{
			Application.logMessageReceivedThreaded += InternalUnityLog;
		}

		void OnLevelWasLoaded (int level)
		{
			Commands.ClearCommands();
			Commands.InitializeMethods();
			InternalItems.ClearItems();
			InternalItems.InitializeItems();
		}

		private void InternalUnityLog (string condition, string stackTrace, UnityEngine.LogType type)
		{
			Interface.RegisterLog(condition, stackTrace, (LogType)type);
		}

		internal void CommandLaunched (LogMessage message)
		{
			if (OnCommandLaunched != null)
				OnCommandLaunched(message);
		}

		internal void MessageLogged (LogMessage message)
		{
			if (OnMessageLogged != null)
				OnMessageLogged(message);
		}

		internal LogMessage RegisterCommand (string commandName, string parameters, string status)
		{
			return Interface.RegisterCommand(parameters, commandName, status);
		}

		internal LogMessage RegisterLog (string log, string stackTrace, LogType type)
		{
			return Interface.RegisterLog(log, stackTrace, type);
		}

		internal void TryCommand (string cmd)
		{
			Commands.ParseCommand(cmd);
		}

		/// <summary> Register a method internally that won't fall off on scene loading (SHOULD TAKE THIS WITH CAUTION) </summary>
		public void RegisterInternal (string commandName, object instance, string description = "")
		{
			Commands.AddInternal(commandName, instance, false, description);
		}

		/// <summary> register a method that will be destroyed on scene loading </summary>
		public void RegisterCommand (string commandName, object instance, string description = "")
		{
			Commands.AddInternal(commandName, instance, true, description);
		}

		/// <summary> register a description for an existing method, or changing it </summary>
		public void RegisterDescription (string commandName, string description)
		{
			if (!Commands.Exists(commandName))
				Debug.Log ("RDC || command : " + commandName + "not found for description registering");
			Commands.AddDescription(commandName, description);
		}

		/// <summary> register the names of the parameters (done by default but you might want to set some custom names) </summary>
		public void RegisterParamsName (string commandName, List<string> paramNames)
		{
			Commands.AddParamsName(paramNames, commandName);
		}

		public void RegisterCustomType (System.Type type, System.Func<string, object> deserializer)
		{
			Serializer.Add(deserializer, type);
		}
	}

	public enum LogType
	{
		Error,
		Assert,
		Warning,
		Info,
		Exception,
		Command
	};
}
