using System.Diagnostics;

namespace WebApp.Utilities.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetCurrentMethodPath(bool isLineBreakPath = false)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame.GetMethod();
            var declaringType = method.DeclaringType;
            var fullName = declaringType != null ? declaringType.FullName + "." + method.Name : "UnknownClass";
            if (isLineBreakPath) fullName += "\r\n";
            return fullName;
        }

    }
}