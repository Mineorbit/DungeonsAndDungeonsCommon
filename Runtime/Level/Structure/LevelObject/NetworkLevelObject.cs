using System;
using System.Collections.Generic;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkLevelObject : LevelObject
    {
        public LevelObjectNetworkHandler levelObjectNetworkHandler;

        private readonly Queue<Action> todo = new Queue<Action>();

	
        public int _Identity;

        public bool identified = false;

	public int Identity
        {
            get => _Identity;
            set
            {
	            if(!identified)
	            {
					identified = true;
					_Identity = value;
	            }
	            else
	            {
		            GameConsole.Log($"{this} is already identified with {_Identity}");
	            }
	            OnIdentify();
            }
        }
        
        public virtual void OnIdentify()
        {
        	GameConsole.Log($"Adding Identity {Identity} to Look Up Table");
            
            if(!identifiedLevelObjects.ContainsKey(Identity))
            {
        		identifiedLevelObjects.Add(Identity,this);
            }
            else
            {
	            GameConsole.Log($"List already had Identity {Identity}");
            }
        }

	public void OnDestroy()
	{
		RemoveFromIdentified();
	}

	public void RemoveFromIdentified()
	{
		if(identifiedLevelObjects.ContainsKey(Identity))
			identifiedLevelObjects.Remove(Identity);
	}

	public void Identify()
	{
		int newID = GetInstanceID();
		GameConsole.Log($"Assigning {this} ID: {newID}");
		Identity = newID;
	}
	
	
	
	public static Dictionary<int,NetworkLevelObject> identifiedLevelObjects = new Dictionary<int,NetworkLevelObject>();
	
	public static NetworkLevelObject FindByIdentity(int id)
	{
		NetworkLevelObject networkLevelObject;
		if(identifiedLevelObjects.TryGetValue(id, out networkLevelObject))
		{
			return networkLevelObject;
		}
		else
			return null;
	}
	
        public virtual void FixedUpdate()
        {
            //this needs a safety later on to stop stupid behaviour
            if (todo.Count > 0)
                todo.Dequeue().Invoke();
        }


        public override void OnInit()
        {
            base.OnInit();

            levelObjectNetworkHandler = GetComponent<LevelObjectNetworkHandler>();
            
			Identify();
            //
            //if(levelObjectNetworkHandler == null) 
            //levelObjectNetworkHandler = gameObject.AddComponent<LevelObjectNetworkHandler>();
        }
        //This marks a message for transport through network
		// It will allways be called when the level is  in test mode (see  edit mode -> edit mode)
		// but in real multiplayer only depending on the conditionals
        public bool CallOnThisSide(bool doLocal, bool doServer)
        {
	        return Level.instantiateType == Level.InstantiateType.Edit ||
	               Level.instantiateType == Level.InstantiateType.Test ||
	               doLocal && (Level.instantiateType == Level.InstantiateType.Online)
	               || doServer && Level.instantiateType == Level.InstantiateType.Play;
        }
        public void Invoke<T>(Action<T> a, T argument, bool doLocal = true, bool doServer = true)
        {
	        if (CallOnThisSide(doLocal,doServer))
	        {
		        GameConsole.Log($"Calling {a.Method.Name} locally");
		        a.DynamicInvoke(argument);
	        }
	        if(levelObjectNetworkHandler.CallActionOnOther(doLocal,doServer))
	        {
		        GameConsole.Log($"Asking other to do {a.Method.Name}");
				if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
					if (identified)
						levelObjectNetworkHandler.SendAction(a.Method.Name,
                        LevelObjectNetworkHandler.ActionParam.From(argument));
					else
						todo.Enqueue(() => { Invoke(a, argument); });
	        }
        }

        public void Invoke(Action a, bool doLocal = true, bool doServer = true)
        {
	        if (CallOnThisSide(doLocal,doServer))
	        {
		        GameConsole.Log($"Calling {a.Method.Name} locally");
		        a.DynamicInvoke();
	        }

	        if(levelObjectNetworkHandler != null)
	        {
				if (levelObjectNetworkHandler.CallActionOnOther(doLocal, doServer))
				{
					if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
						if (identified)
							levelObjectNetworkHandler.SendAction(a.Method.Name);
						else
							todo.Enqueue(() => { Invoke(a); });
				}
	        }
	        else
	        {
		        GameConsole.Log($"{this} has no Networkhandler to Call {a.Method.Name} from");
	        }
        }
        /*

        public void Invoke(Action a)
        {
            MethodInfo methodInfo = a.Method;
            if (this.enabled) a.Invoke();
            if (h != null) h.Marshall(a);
        }*/
    }
}
