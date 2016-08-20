using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Event
    {
        static Dictionary<string, Event> cachedEvents = new Dictionary<string, Event>();

        /// <summary>
        /// Creates a new instance of Tril.Models.Event
        /// </summary>
        /// <param name="_event"></param>
        private Event(EventReference _event) : base(_event) { }

        /// <summary>
        /// Returns either a cached Event if the specified EventReference has been used before, otherwise a new Event
        /// </summary>
        /// <param name="_event"></param>
        /// <returns></returns>
        public static Event GetCachedEvent(EventReference _event) 
        {
            return new Event(_event);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (_event.FullName == null || _event.FullName == "")
            //    return new Event(_event);

            //string key = _event.FullName + "," + _event.Module.FullyQualifiedName;
            //if (cachedEvents.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedEvents[key];
            //else
            //{
            //    Event evt = new Event(_event);
            //    cachedEvents.Add(key, evt);
            //    return evt;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate type definition for the declaring type reference
        /// </summary>
        /// <returns></returns>
        public TypeDefinition GetDeclaringTypeDefinition()
        {
            return UnderlyingEvent.DeclaringType.Resolve();
        }
        /// <summary>
        /// Returns the appropriate event definition for the underlying event reference
        /// </summary>
        /// <returns></returns>
        public EventDefinition GetEventDefinition()
        {
            return UnderlyingEvent.Resolve();
        }

        EventDefinition namesakeevent;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake()
        {
            if (namesakeevent != null)
                return namesakeevent;

            TypeDefinition decKindNamesake = DeclaringKind.GetNamesake();
            if (decKindNamesake != null)
            {
                foreach (EventDefinition _event in decKindNamesake.Events)
                {
                    if (Utility.AreNamesakes(_event, UnderlyingEvent))
                    {
                        namesakeevent = _event;
                        break;
                    }
                }
                return namesakeevent;
            }

            return null;
        }
    }
}
