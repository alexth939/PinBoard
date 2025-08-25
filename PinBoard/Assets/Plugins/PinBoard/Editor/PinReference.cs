using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinBoard.Editor
{
    [Serializable]
    internal sealed class PinReference
    {
        [SerializeField] private string _globalIdString;

        public string GlobalIdString => _globalIdString;

        public static PinReference FromObject(Object globalObject)
        {
            if(globalObject == null)
                throw new ArgumentNullException(nameof(globalObject));

            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(globalObject);
            PinReference reference = new() { _globalIdString = globalId.ToString() };

            return reference;
        }

        private const string DefaultNullId = "GlobalObjectId_V1-0-00000000000000000000000000000000-0-0";

        public static bool TryGetFromObject(Object globalObject, out PinReference pin)
        {
            if(globalObject == null)
                throw new ArgumentNullException(nameof(globalObject));

            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(globalObject);
            pin = new() { _globalIdString = globalId.ToString() };

            return globalId.ToString() is not DefaultNullId;
        }

        public Object Resolve()
        {
            Object objectInstance = null;

            if(string.IsNullOrEmpty(_globalIdString))
            {
                Debug.LogWarning($"Reference id corrupted!");
            }
            else if(GlobalObjectId.TryParse(_globalIdString, out GlobalObjectId globalId))
            {
                objectInstance = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
            }
            else
            {
                Debug.LogWarning($"Failed to resolve object by id:[{_globalIdString}]");
            }

            return objectInstance;
        }
    }
}
