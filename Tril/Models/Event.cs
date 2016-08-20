using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// Represents an event in a Tril.Models.Kind
    /// </summary>
    [Serializable]
    public partial class Event : Member
    {
        /// <summary>
        /// Returns the default access modifiers
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string[] GetAccessModifiers_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            List<string> accMods = new List<string>();

            if (AddMethod != null)
            {
                accMods.AddRange(AddMethod.GetAccessModifiers(useDefaultOnly, targetPlats));
            }
            else if (RemoveMethod != null)
            {
                accMods.AddRange(RemoveMethod.GetAccessModifiers(useDefaultOnly, targetPlats));
            }

            return accMods.ToArray();
        }
        /// <summary>
        /// Returns the default attributes
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string[] GetAttributes_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            List<string> attris = new List<string>();

            if (AddMethod != null)
            {
                attris.AddRange(AddMethod.GetAttributes(useDefaultOnly, targetPlats));
            }
            else if (RemoveMethod != null)
            {
                attris.AddRange(RemoveMethod.GetAttributes(useDefaultOnly, targetPlats));
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets the event handler type of this Tril.Models.Event instance.
        /// If there is no user-defined event type, the event type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined event type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <returns></returns>
        public dynamic GetEventHandlerKind()
        {
            return GetEventHandlerKind("*");
        }
        /// <summary>
        /// Gets the event handler type of this Tril.Models.Event instance.
        /// If there is no user-defined event type, the event type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined event type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetEventHandlerKind(bool useDefaultOnly)
        {
            return GetEventHandlerKind(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets the event handler type of this Tril.Models.Event instance.
        /// If there is no user-defined event type, the event type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined event type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetEventHandlerKind(params string[] targetPlats)
        {
            return GetEventHandlerKind(false, targetPlats);
        }
        /// <summary>
        /// Gets the event handler type of this Tril.Models.Event instance.
        /// If there is no user-defined event type, the event type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined event type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetEventHandlerKind(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedEventHandlerKind(targetPlats) && !useDefaultOnly)
            {
                var retTypeAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");
                foreach (CustomAttribute attri in retTypeAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            return attri.ConstructorArguments[0].Value.ToString();
                        }
                    }
                }
            }

            if (UnderlyingEvent.EventType != null)
            {
                return Kind.GetCachedKind(UnderlyingEvent.EventType);
            }

            return null;
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Event, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public override string GetLongName(bool useDefaultOnly, params string[] targetPlats)
        {
            return DeclaringKind.GetLongName(useDefaultOnly, targetPlats).Trim() + "::" + GetName(useDefaultOnly, targetPlats).Trim();
        }
        /// <summary>
        /// Returns the default name
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string GetName_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            string _name = UnderlyingEvent.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes()
        {
            List<string> attris = new List<string>();

            if (AddMethod != null)
            {
                attris.AddRange(AddMethod.GetRawAttributes());
            }
            else if (RemoveMethod != null)
            {
                attris.AddRange(RemoveMethod.GetRawAttributes());
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Tril.Models.Event instance can be accessed.
        /// If the underlying event has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying event has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <returns></returns>
        public bool HasHiddenImplementation()
        {
            return HasHiddenImplementation("*");
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Tril.Models.Event instance can be accessed.
        /// If the underlying event has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying event has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasHiddenImplementation(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var showAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ShowImplementationAttribute");
            if (HasUserDefined(showAttris, targetPlats))
                return false;

            var hideAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.HideImplementationAttribute");
            if (HasUserDefined(hideAttris, targetPlats))
                return true;

            return DeclaringKind.HasHiddenImplementation(targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the event type of this Event instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedEventHandlerKind()
        {
            return HasUserDefinedEventHandlerKind("*");
        }
        /// <summary>
        /// Gets a value indicating whether the event type of this Event instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedEventHandlerKind(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var retAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");

            return HasUserDefined(retAttris, targetPlats);
        }

        /// <summary>
        /// Gets the Kind that owns this event
        /// </summary>
        public Kind DeclaringKind
        {
            get { return Kind.GetCachedKind(UnderlyingEvent.DeclaringType); }
        }
        /// <summary>
        /// Gets the Tril.Models.Method object that represents the add block of the underlying event.
        /// Returns null if the underlying event has no add block.
        /// </summary>
        public Method AddMethod
        {
            get
            {
                var EventDef = GetEventDefinition();
                if (EventDef != null)
                {
                    if (EventDef.AddMethod != null)
                    {
                        return Method.GetCachedMethod(EventDef.AddMethod);
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the the underlying event has an add block.
        /// </summary>
        public bool HasAddMethod
        {
            get { return (AddMethod != null); }
        }
        /// <summary>
        /// Gets a value indicating whether the the underlying event has a remove block.
        /// </summary>
        public bool HasRemoveMethod
        {
            get { return (RemoveMethod != null); }
        }
        //InvokeMethod and HasInvokeMethod
        /// <summary>
        /// Gets the Tril.Models.Method object that represents the remove block of the underlying event.
        /// Returns null if the underlying event has no remove block.
        /// </summary>
        public Method RemoveMethod
        {
            get
            {
                var EventDef = GetEventDefinition();
                if (EventDef != null)
                {
                    if (EventDef.RemoveMethod != null)
                    {
                        return Method.GetCachedMethod(EventDef.RemoveMethod);
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.EventReference from which this instance of Tril.Models.Event was built
        /// </summary>
        public EventReference UnderlyingEvent
        {
            get { return ((EventReference)_underlyingInfo); }
        }
    }
}
