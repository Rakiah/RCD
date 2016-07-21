using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace RakiahDevConsole
{
	public abstract class RDCConsoleLine 
	{
		/// <summary> command that was sent (parameters) </summary>
		public string 		primaryText;
		
		/// <summary> name of the command </summary>
		public string 		secondaryText;

		/// <summary> the holder for the log </summary>
		public Text		primaryTextComponent;

		/// <summary> the holder for the secondaryText component </summary>
		public Text		secondaryTextComponent;

		public Image		graphicsImg;
		
		/// <summary> the holder for the command </summary>
		public GameObject 	graphicsMessage;

		public virtual void InstantiateObject (RectTransform ScrollZone, GameObject Prefab)
		{
			graphicsMessage = GameObject.Instantiate (Prefab) as GameObject;
			graphicsMessage.transform.SetParent(ScrollZone);
			graphicsMessage.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		public void FindComponents ()
		{
			primaryTextComponent = graphicsMessage.transform.GetChild(0).GetComponent<Text>();
			secondaryTextComponent = graphicsMessage.transform.GetChild(1).GetComponent<Text>();
			graphicsImg = graphicsMessage.GetComponent<Image>();
		}

		public void Initialize (string _primary, string _secondary)
		{
			graphicsMessage.SetActive(true);
			graphicsMessage.transform.SetAsLastSibling();

			primaryText = _primary;
			secondaryText = _secondary;

			primaryTextComponent.text = _primary;
			secondaryTextComponent.text = _secondary;
		}

		public void Reset ()
		{
			primaryText = string.Empty;
			secondaryText = string.Empty;

			graphicsImg.color = Color.white;
			graphicsMessage.SetActive(false);
		}
	}

	[System.Serializable]
	public class LogMessage : RDCConsoleLine
	{
		/// <summary> id of the message </summary>
		public float 								timeReceived;

		/// <summary> type of the logged message </summary>
		public LogType								type;

		public Image								typeComponent;

		private const int 							MaxPoolSize
		= 65536;
		
		private static Queue<LogMessage> 			Pool
		= new Queue<LogMessage>();

		public static LogMessage New (float _timeReceived, string _log, string _stackTrace, LogType _type, RectTransform ScrollZone, GameObject Prefab) 
		{
			LogMessage message = CreateFromPool(ScrollZone, Prefab);
			message.Initialize(_timeReceived, _log, _stackTrace, _type);

			return message;
		}

		private LogMessage (RectTransform ScrollZone, GameObject Prefab)
		{
			InstantiateObject(ScrollZone, Prefab);
			base.FindComponents();
			FindComponents();
		}

		public new void FindComponents ()
		{
			typeComponent = graphicsMessage.transform.GetChild(2).GetComponent<Image>();
		}

		public void Initialize (float _timeReceived, string _log, string _stackTrace, LogType _type)
		{
			timeReceived = _timeReceived;
			type = _type;

			base.Initialize(_log, string.Format("{0:0.00}", _timeReceived));
			typeComponent.sprite = RDCInterface.TypeToTexture(type);
		}

		public void Release() 
		{
			if (Pool.Count > MaxPoolSize) return;

			base.Reset();
			Reset();

			if (!Pool.Contains(this)) Pool.Enqueue(this);
		}

		public new void Reset ()
		{
			timeReceived = 0.0f;
			type = LogType.Info;
		}
		
		private static LogMessage CreateFromPool(RectTransform ScrollZone, GameObject Prefab) 
		{
			return Pool.Count == 0 ? new LogMessage(ScrollZone, Prefab) : Pool.Dequeue();
		}
	}

	[System.Serializable]
	public class CommandHelperMessage : RDCConsoleLine
	{
		private const int 									MaxPoolSize 
			= 65536;
		
		private static Queue<CommandHelperMessage> 			Pool
			= new Queue<CommandHelperMessage>();
		
		public static CommandHelperMessage New (string _primary, string _secondary, RectTransform ScrollZone, GameObject Prefab) 
		{
			CommandHelperMessage message = CreateFromPool(ScrollZone, Prefab);
			message.Initialize(_primary, _secondary);
			
			return message;
		}
		
		private CommandHelperMessage (RectTransform ScrollZone, GameObject Prefab)
		{
			InstantiateObject(ScrollZone, Prefab);
			FindComponents();
		}
		
		public void Release() 
		{
			if (Pool.Count > MaxPoolSize) return;
			Reset();
			if (!Pool.Contains(this)) Pool.Enqueue(this);
		}
		
		private static CommandHelperMessage CreateFromPool(RectTransform ScrollZone, GameObject Prefab) 
		{
			return Pool.Count == 0 ? new CommandHelperMessage(ScrollZone, Prefab) : Pool.Dequeue();
		}
	}
}
