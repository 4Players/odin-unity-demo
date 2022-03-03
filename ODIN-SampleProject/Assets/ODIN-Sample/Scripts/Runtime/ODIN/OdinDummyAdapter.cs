using System;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinDummyAdapter : AOdinMultiplayerAdapter
    {
        private string _uniqueId;

        private void Awake()
        {
            _uniqueId = Guid.NewGuid().ToString();
        }

        public override string GetUniqueUserId()
        {
            return _uniqueId;
        }

        public override bool IsLocalUser()
        {
            return true;
        }
    }
}