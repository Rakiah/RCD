using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public class RDCInterface : MonoBehaviour 
	{
		///----------Members Part-------------///

		/// <summary> Keycode for disabling/enabling console </summary>
		public KeyCode							InputDisableEnable
		= KeyCode.LeftAlt;

		/// <summary> state of the console </summary>
		public bool							Active
		= false;

		/// <summary> holder for members of the Logs window </summary>
		public ScrollWindow						LogsWindow;

		/// <summary> holder for members of the CommandHelper window </summary>
		public ScrollWindow						CommandHelperWindow;

		/// <summary> Input field for commands </summary>
		public InputField						CommandInputField;

		/// <summary> max ammounts of message logged in the console at same time </summary>
		public int							MaxLoggedMessage
		= 3000;

		/// <summary> Manager for the other core components</summary>
		private RDCManager						Manager;

		/// <summary> Accesor to the logs </summary>
		public List<LogMessage> 					Logs 
		= new List<LogMessage>();

		/// <summary> Accesor to the commands list </summary>
		public List<CommandHelperMessage>				CommandsMessage
		= new List<CommandHelperMessage>();

		public CommandHelperMessage					SelectedCommandMessage		
		{ 
			get 
			{ 
				if (SelectedCommmandHelperMessageID >= 0 && SelectedCommmandHelperMessageID < CommandsMessage.Count)
					return CommandsMessage[SelectedCommmandHelperMessageID];
				return null;
			}
		}

		/// <summary> Sprite corresponding to the type of the log </summary>
		public static List<Sprite>					LogTypeTextures
		= new List<Sprite>();

		public Transform 						GeneralInformations;

		public Transform						SelectedElementInfo;

		public Canvas							MainCanvas;
		
		/// <summary> Command helper object filled when user write </summary>
		private commandHelperObject					CmdHelperStruct;

		private List<string> 						sysInfo
		= new List<string>();

		private List<Text> 						sysInfoText
		= new List<Text>();

		private List<Text>						selectedElementText
		= new List<Text>();

		private int							fps;

		private float							fpsTime;

		private float							navTime;

		private int							SelectedCommmandHelperMessageID
		= -1;

		public float							NavigationTimer 
		= 0.2f;

		///------------Methods Part-------------///

		public void Initialize (RDCManager _manager)
		{
			Manager = _manager;
			CmdHelperStruct = new commandHelperObject();

			int i = 0;

			for (i = 0; i < 6; i++)
				LogTypeTextures.Add(Resources.Load("Textures/LogTypeTexture" + i, typeof(Sprite)) as Sprite);

			for (i = 1; i < GeneralInformations.childCount; i++)
				sysInfoText.Add(GeneralInformations.GetChild(i).GetComponent<Text>());

			for (i = 1; i < SelectedElementInfo.childCount; i++)
				selectedElementText.Add(SelectedElementInfo.GetChild(i).GetComponent<Text>());


			sysInfo.Add ("FPS : " + fps);
			sysInfo.Add ("since start : " + Time.realtimeSinceStartup.ToString("0:00"));
			sysInfo.Add ("Resolution : " + Screen.width + " x " + Screen.height);
			sysInfo.Add (System.DateTime.Now.ToString());
			sysInfo.Add (SystemInfo.operatingSystem);
			sysInfo.Add ("RAM : " + SystemInfo.systemMemorySize.ToString() + " GC Memory : " + SystemInfo.graphicsMemorySize.ToString());
			sysInfo.Add ("Device infos : " + SystemInfo.deviceName + " " + SystemInfo.deviceType.ToString());
			sysInfo.Add (SystemInfo.graphicsDeviceName);
			sysInfo.Add (SystemInfo.deviceModel);

			SetGeneralInformations();

			SetHierarchy(CommandHelperHierarchy.Commands, "");

			MainCanvas.enabled = Active;
			CommandInputField.enabled = Active;
		}

		private IEnumerator Start ()
		{
			yield return null;

			CommandHelperWindow.Scrollbar.value = 1.0f;
		}

		private void SetGeneralInformations ()
		{
			for (int i = 0; i < sysInfo.Count; i++)
				sysInfoText[i].text = sysInfo[i];
		}

		void Update ()
		{
			if (Input.GetKeyDown(InputDisableEnable))
			{
				Active = !Active;
				MainCanvas.enabled = Active;
				CommandInputField.enabled = Active;
			}

			if (Active)
			{
				RealTimeGeneralInformations();

				if (Input.GetKeyDown(KeyCode.Return))
				{
					EventSystem system = EventSystem.current;

					if (system.currentSelectedGameObject == CommandInputField.gameObject)
					{
						if (SelectedCommandMessage != null)
						{
							string [] splitted = CommandInputField.text.Split(' ');

							string newCmd = "";

							for (int i = 0; i < splitted.Length - 1; i++) 
								newCmd += splitted[i] + " ";

							newCmd += SelectedCommandMessage.primaryText + " ";

							CommandInputField.text = newCmd;
							CommandHelperWindow.Scrollbar.value = 1.0f;

							CommandInputField.ActivateInputField();
						}

						else
						{
							Manager.TryCommand(CommandInputField.text.Trim());
							CommandInputField.text = "";
							CommandInputField.ActivateInputField();
						}
					}

					else CommandInputField.ActivateInputField();
				}

				if (SelectedCommmandHelperMessageID >= 0)
				{
					if(navTime <= 0.0f)
					{
						if (Input.GetKey(KeyCode.DownArrow))
						{
							HighlightCommandHelper(SelectedCommmandHelperMessageID + 1);
							navTime = NavigationTimer;
						}

						else if (Input.GetKey(KeyCode.UpArrow))
						{
							HighlightCommandHelper(SelectedCommmandHelperMessageID - 1);
							navTime = NavigationTimer;
						}
					}
				}

				navTime -= Time.deltaTime;
			}
		}

		public void RealTimeGeneralInformations ()
		{
			fps++;
			fpsTime += Time.deltaTime;
			
			if (fpsTime >= 1.0f)
			{
				sysInfo[0] = "FPS : " + fps;
				fps = 0;
				fpsTime = 0.0f;
			}
			sysInfo[1] = "since start : " + Time.realtimeSinceStartup.ToString("0:00");
			sysInfo[2] = "Resolution : " + Screen.width + " x " + Screen.height;

			SetGeneralInformations();
		}

		public LogMessage RegisterCommand (string command, string commandName, string commandStatus)
		{
			return RegisterLog(commandName + " : " + commandStatus, command, LogType.Command); 
		}

		public LogMessage RegisterLog (string log, string stackTrace, LogType type)
		{
			LogMessage message = LogMessage.New(Time.realtimeSinceStartup, log, stackTrace, type, LogsWindow.ScrollZone, LogsWindow.UIElementPrefab);
			Logs.Add(message);
			
			LogsWindow.CalculateZone(Logs.Count);
			
			Manager.MessageLogged(message);

			if (Logs.Count > MaxLoggedMessage)
			{
				Logs[0].Release();
				Logs.RemoveAt(0);
			}

			return message;
		}

		public void OnValueChanged (string input)
		{
			SetHierarchy(CalculateCommandHelper(input), input);
		}

		private void ClearCommandsZone ()
		{
			for (int i = CommandsMessage.Count - 1; i >= 0; i--) 
			{
				CommandsMessage[i].Release();
				CommandsMessage.RemoveAt(i);
			}

			CommandHelperWindow.CalculateZone(0);
		}

		private void SetHierarchy (CommandHelperHierarchy hierarchy, string input)
		{
			ClearCommandsZone ();

			if (hierarchy == CommandHelperHierarchy.Commands)
			{
				foreach(Command cmd in Manager.Commands.Methods.Values)
				{
					if (cmd.Name.Contains(input) || input.Length < 2)
					{
						CommandHelperMessage message = CommandHelperMessage.New(cmd.Name, cmd.Description, CommandHelperWindow.ScrollZone, CommandHelperWindow.UIElementPrefab);
						CommandsMessage.Add(message);
					}
				}

				HighlightCommandHelper(0);
			}
			else if (hierarchy == CommandHelperHierarchy.Items)
			{
				foreach(RDCItem item in Manager.InternalItems.Items)
				{
					CommandHelperMessage message = CommandHelperMessage.New(Manager.InternalItems.Items.IndexOf(item).ToString(), item.Obj.name, CommandHelperWindow.ScrollZone, CommandHelperWindow.UIElementPrefab);
					CommandsMessage.Add(message);
				}

				HighlightCommandHelper(0);
			}
			else if (hierarchy == CommandHelperHierarchy.Behaviours)
			{
				foreach(RDCMethodsItem behaviour in CmdHelperStruct.obj.components.Values)
				{
					CommandHelperMessage message = CommandHelperMessage.New(behaviour.nameBehaviour, behaviour.MethodLookup.Count.ToString(), CommandHelperWindow.ScrollZone, CommandHelperWindow.UIElementPrefab);
					CommandsMessage.Add(message);
				}

				HighlightCommandHelper(0);
			}
			else if (hierarchy == CommandHelperHierarchy.Methods)
			{
				foreach(string method in CmdHelperStruct.behavior.MethodLookup.Keys)
				{
					MethodInfo info = CmdHelperStruct.behavior.MethodLookup[method];

					CommandHelperMessage message = CommandHelperMessage.New(method, info.GetParameters().Length.ToString(), CommandHelperWindow.ScrollZone, CommandHelperWindow.UIElementPrefab);
					CommandsMessage.Add(message);
				}

				HighlightCommandHelper(0);
			}
			else if (hierarchy == CommandHelperHierarchy.Parameters)
			{
				Command cmd = CmdHelperStruct.cmd;
				int currentParam = CmdHelperStruct.currentParam;

				CommandsMessage.Add(CommandHelperMessage.New(cmd.GetParamName(currentParam),
				                                            	 "Type of " + cmd.GetParamType(currentParam).ToString(), 
				                                            	 CommandHelperWindow.ScrollZone,
				                                            	 CommandHelperWindow.UIElementPrefab));

				SelectedCommmandHelperMessageID = -1;
			}
			else if (hierarchy == CommandHelperHierarchy.InvalidCommand)
			{
				CommandHelperMessage message = CommandHelperMessage.New("Invalid",
				                                                        CmdHelperStruct.InvalidCommandMessage,
				                                                        CommandHelperWindow.ScrollZone,
				                                                        CommandHelperWindow.UIElementPrefab);
				CommandsMessage.Add(message);

				SelectedCommmandHelperMessageID = -1;
			}
			else if (hierarchy == CommandHelperHierarchy.NoMoreParams)
			{
				CommandHelperMessage message = CommandHelperMessage.New("Stop",
				                                                        CmdHelperStruct.InvalidCommandMessage,
				                                                        CommandHelperWindow.ScrollZone,
				                                                        CommandHelperWindow.UIElementPrefab);
				CommandsMessage.Add(message);
				
				SelectedCommmandHelperMessageID = -1;
			}

			CommandHelperWindow.CalculateZone(CommandsMessage.Count);
		}
			
		private void HighlightCommandHelper(int id)
		{
			if (id >= 0 && id < CommandsMessage.Count)
			{
				if (SelectedCommmandHelperMessageID >= 0 && SelectedCommmandHelperMessageID < CommandsMessage.Count)
					SelectedCommandMessage.graphicsImg.color = Color.white;
				CommandsMessage[id].graphicsImg.color = Color.gray;

				CommandHelperWindow.ScrollZone.anchoredPosition = new Vector2(0.0f, 0.0f);
				int HighlightedPos = (-(GetChildPosition(CommandHelperWindow.ScrollZone, CommandsMessage[id].graphicsMessage.transform) - CommandsMessage.Count)) - 1;

				float MoveValue = HighlightedPos * (1.0f/ (CommandsMessage.Count - 1));
				CommandHelperWindow.Scrollbar.value = MoveValue;

				SelectedCommmandHelperMessageID = id;

				if (Manager.Commands.Exists(CommandsMessage[id].primaryText))
				{
					Command cmd = Manager.Commands.Get(CommandsMessage[id].primaryText);

					string t1 = "Command name : " + cmd.Name + "\n" + "Description : " + cmd.Description + "\ncalled on object : ";
					foreach(object obj in cmd.Method.Keys)
						t1 += obj.GetType().Name + " ";


					string t2 = "Parameters : \n";
					for (int i = 0; i < cmd.ParamCount; i++)
						t2 += "Param name : " + cmd.GetParamName(i) + ", Param type : " + cmd.GetParamType(i) + "\n";

					FillSelectedElement (t1, t2);
				}
				else FillSelectedElement (CommandsMessage[id].primaryText, CommandsMessage[id].secondaryText);
			}
		}

		private void FillSelectedElement (string t1, string t2)
		{
			selectedElementText[0].text = t1;
			selectedElementText[1].text = t2;
		}

		private CommandHelperHierarchy CalculateCommandHelper (string input)
		{
			string [] ParsedCmd = input.Split(' ');

			CmdHelperStruct.Clear();

			if (ParsedCmd.Length == 1)
				return CommandHelperHierarchy.Commands;
			//if there is more than one string in ParsedCmd, it means he's not anymore at the command selection, so we should check if his command is valid or not
			else if (ParsedCmd.Length > 1)
			{
				//invalid case
				if (!Manager.Commands.Exists(ParsedCmd[0]))
				{
					CmdHelperStruct.InvalidCommandMessage = "Unknown command";
					return CommandHelperHierarchy.InvalidCommand;
				}

				Command cmd = Manager.Commands.Get(ParsedCmd[0]);
				CmdHelperStruct.cmd = cmd;

				//invalid case
				if (ParsedCmd.Length - 1 > cmd.ParamCount)
				{
					CmdHelperStruct.InvalidCommandMessage = "Command doesnt accept more params";
					return CommandHelperHierarchy.NoMoreParams;
				}

				CmdHelperStruct.currentParam = ParsedCmd.Length - 2;

				for (int i = 1; i < ParsedCmd.Length; i++)
				{
					System.Type paramType = cmd.GetParamType(i - 1);
					if (paramType == typeof(RDCItem))
					{
						/// not last element, check if this element (which is done) is correct
						if (i < ParsedCmd.Length - 1)
						{
							int obj = -1;
							if (int.TryParse(ParsedCmd[i], out obj))
							{
								RDCItem item = Manager.InternalItems.Get(obj);
								if (item != null)
									CmdHelperStruct.obj = item;
								else 
								{
									CmdHelperStruct.InvalidCommandMessage = "Item ID not found";
									return CommandHelperHierarchy.InvalidCommand;
								}
							}
							else
							{
								CmdHelperStruct.InvalidCommandMessage = "Wrong parameters type, should be type of int";
								return CommandHelperHierarchy.InvalidCommand;
							}
						}
						/// its the last element, give the user the choice
						else return CommandHelperHierarchy.Items;
					}
					else if(paramType == typeof(RDCMethodsItem))
					{
						if (CmdHelperStruct.CanShowBehaviours())
						{
							if (i < ParsedCmd.Length - 1)
							{
								if (CmdHelperStruct.obj.components.ContainsKey(ParsedCmd[i]))
								    CmdHelperStruct.behavior = CmdHelperStruct.obj.components[ParsedCmd[i]];
								else
								{
									CmdHelperStruct.InvalidCommandMessage = "No behaviour named " + ParsedCmd[i] + " found on " + CmdHelperStruct.obj.Obj.name;
									return CommandHelperHierarchy.InvalidCommand;
								}
							}
							else return CommandHelperHierarchy.Behaviours;
						}
						else
						{
							CmdHelperStruct.InvalidCommandMessage = "No object to search for behaviours passed in";
							return CommandHelperHierarchy.InvalidCommand;
						}
					}
					else if(paramType == typeof(MethodInfo))
					{
						if(CmdHelperStruct.CanShowMethods())
						{
							if(i < ParsedCmd.Length - 1)
							{
								if (CmdHelperStruct.behavior.MethodLookup.ContainsKey(ParsedCmd[i]))
									CmdHelperStruct.method = CmdHelperStruct.behavior.MethodLookup[ParsedCmd[i]];
								else 
								{
									CmdHelperStruct.InvalidCommandMessage = "No methods named " + ParsedCmd[i] + " found on " + CmdHelperStruct.behavior.nameBehaviour;
									return CommandHelperHierarchy.InvalidCommand;
								}
							}
							else return CommandHelperHierarchy.Methods;
						}
						else
						{
							CmdHelperStruct.InvalidCommandMessage = "No behaviour to search for methods passed in";
							return CommandHelperHierarchy.InvalidCommand;
						}
					}
					else
					{
						return CommandHelperHierarchy.Parameters;
					}
				}
			}

			CmdHelperStruct.InvalidCommandMessage = "Unknown error for invalid command";
			return CommandHelperHierarchy.InvalidCommand;
		}

		private int GetChildPosition (Transform parent, Transform children)
		{
			List<Transform> childs = GetActiveChildList(parent);

			for (int i = 0; i < childs.Count; i++)
			{
				if (childs[i] == children)
					return i;
			}

			return -1;
		}

		private List<Transform> GetActiveChildList (Transform t)
		{
			List<Transform> activeChilds = new List<Transform>();

			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i).gameObject.activeSelf)
					activeChilds.Add(t.GetChild(i));
			}

			return activeChilds;
		}

		public LogMessage GetMessage (int id)
		{
			if (id >= 0 && id < Logs.Count)
				return Logs[id];

			Debug.Log ("RDC : wrong id for message " + id);
			return null;
		}

		public static Sprite TypeToTexture (LogType type)
		{
			return LogTypeTextures[(int)type];
		}
	}

	[System.Serializable]
	public class commandHelperObject
	{
		internal Command 			cmd = null;

		internal int				currentParam = 0;

		internal RDCItem 			obj = null;

		internal RDCMethodsItem 	behavior = null;

		internal MethodInfo 		method = null;

		internal string				InvalidCommandMessage;

		public bool CanShowBehaviours ()
		{
			if (obj != null)
				return true;
			return false;
		}

		public bool CanShowMethods ()
		{
			if (behavior != null)
				return true;
			return false;
		}

		public void Clear ()
		{
			cmd = null;
			currentParam = 0;
			InvalidCommandMessage = string.Empty;
			obj = null;
			behavior = null;
			method = null;
		}
	}

	[System.Serializable]
	public class ScrollWindow
	{
		/// <summary> Scroll bar </summary>
		public Scrollbar 				Scrollbar;
		
		/// <summary> Scroll rect for </summary>
		public ScrollRect				Scrollrect;
		
		/// <summary> Scroll zone </summary>
		public RectTransform			ScrollZone;

		/// <summary> the size to add to the scrollZone each time a UI element is added </summary>
		public float					SizePerMessage;
		
		/// <summary> the count required to trigger the activation of the scroll bar and the scrollzone </summary>
		public int						CountTriggerScroll;

		/// <summary> Prefab for a console message </summary>
		public GameObject				UIElementPrefab;
		
		public void CalculateZone (int uiElements)
		{
			ScrollZone.anchorMax = new Vector2(ScrollZone.anchorMax.x, uiElements * SizePerMessage);

			if (uiElements >= CountTriggerScroll)
			{
				Scrollbar.gameObject.SetActive(true);
				Scrollrect.enabled = true;
			}
			else
			{
				Scrollbar.gameObject.SetActive(false);
				Scrollrect.enabled = false;
				Scrollbar.value = 1.0f;
				ScrollZone.anchoredPosition = new Vector2(0.0f, 0.0f);
			}
		}
	}

	public enum CommandHelperHierarchy 
	{
		None,
		Commands,
		Items,
		Behaviours,
		Methods,
		Parameters,
		InvalidCommand,
		NoMoreParams
	}
}
