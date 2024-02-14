using System;
using System.Security.Principal;

namespace BiskaUtil
{
    [Serializable]
    public class NotAuthenticatedUser : IIdentity
    {
        public string AuthenticationType
        {
            get
            {
                return "Forms";
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return "None";
            }
        }
    }
}