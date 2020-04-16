using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace MEE7.Backend.HelperFunctions
{
    public static class Extensions
    {
        public static bool IsFileLocked(this FileInfo file) // from https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public static void InvokeParallel(this Delegate del, params object[] args)
        {
            foreach (var d in del.GetInvocationList())
                Task.Run(() =>
                {
                    try { d.DynamicInvoke(args); }
                    catch { }
                });
        }
        public static string ToReadableString(this Type type)
        {
            return "`" + type.ToString().Split('.', '+').Last() + "`";
        }
        public static byte[] ToArray(this Stream stream)
        {
            byte[] buffer = new byte[4096];
            int reader = 0;
            MemoryStream memoryStream = new MemoryStream();
            while ((reader = stream.Read(buffer, 0, buffer.Length)) != 0)
                memoryStream.Write(buffer, 0, reader);
            return memoryStream.ToArray();
        }
        public static Vector2 Normalize(this Vector2 v)
        {
            float l = v.Length();
            v.X = v.X / l;
            v.Y = v.Y / l;
            return v;
        }
        public static int ReLU(this int i)
        {
            return i < 0 ? 0 : i;
        }
    }
}
