using System;
using System.Text;
using System.Web.Security;

namespace ChampionMains.Pyrobot
{
    public static class SecurityUtil
    {
        public static string Protect(string plainText, string reason)
        {
            if (plainText == null) throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(reason)) throw new ArgumentException("reason");
            return Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(plainText), reason));
        }

        public static string Unprotect(string protectedText, string reason)
        {
            if (protectedText == null) throw new ArgumentNullException("protectedText");
            if (string.IsNullOrEmpty(reason)) throw new ArgumentException("reason");
            return Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(protectedText), reason));
        }
    }
}