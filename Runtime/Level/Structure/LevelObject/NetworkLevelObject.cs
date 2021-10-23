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
        public void Invoke<T>(Action<T> a, T argument, bool doLocal = false, bool allowLocal = true)
        {
	        if (allowLocal && (doLocal || Level.instantiateType == Level.InstantiateType.Play ||
	                           Level.instantiateType == Level.InstantiateType.Test))
	        {
		        GameConsole.Log($"Calling {a.Method.Name} locally");
		        a.DynamicInvoke(argument);
	        }
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
                if (identified)
                    levelObjectNetworkHandler.SendAction(a.Method.Name,
                        LevelObjectNetworkHandler.ActionParam.From(argument));
                else
                    todo.Enqueue(() => { Invoke(a, argument); });
        }

        public void Invoke(Action a, bool doLocal = false, bool allowLocal = true)
        {
	        if (allowLocal && (doLocal || Level.instantiateType == Level.InstantiateType.Play ||
	                           Level.instantiateType == Level.InstantiateType.Test))
	        {
		        GameConsole.Log($"Calling {a.Method.Name} locally");
		        a.DynamicInvoke();
	        }
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
                if (identified)
                    levelObjectNetworkHandler.SendAction(a.Method.Name);
                else
                    todo.Enqueue(() => { Invoke(a); });
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
