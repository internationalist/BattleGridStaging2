using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CommandQueue
{
    List<CommandQueueElement> data = new List<CommandQueueElement>();
    public struct CommandQueueElement
    {
        public int slot;
        public Transform enemyTransform;
        public Vector3? destination;
        public Command.OnCompleteCallback onComplete;

        public CommandQueueElement(int slot,
                                Transform enemyTransform,
                                Vector3? destination,
                                Command.OnCompleteCallback onComplete)
        {
            this.slot = slot;
            this.enemyTransform = enemyTransform;
            this.destination = destination;
            this.onComplete = onComplete;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CommandQueueElement))
            {
                return false;
            }
            CommandQueueElement cqc = (CommandQueueElement)obj;
            return slot == cqc.slot;
        }

        public override int GetHashCode()
        {
            return this.slot;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Enqueue(CommandQueueElement element)
    {
        data.Add(element);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public CommandQueueElement? Dequeue()
    {
        if(data.Count > 0)
        {
            CommandQueueElement element = data[0];
            data.Remove(element);
            return element;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Replace(CommandQueueElement element)
    {
        int idx = data.FindIndex(e=> e.slot == element.slot);
        if(idx > -1)
        {
            data.Insert(idx, element);
            return true;
        }
        return false;
    }

    public bool Contains(CommandQueueElement element)
    {
        return data.Contains(element);
    }
}
