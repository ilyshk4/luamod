using System;
using System.Collections.Generic;

using UnityEngine;

namespace UniLua
{
    public delegate string PathHook(string filename);
    public class LuaFile
    {
        //private static readonly string LUA_ROOT = System.IO.Path.Combine(Application.streamingAssetsPath, "LuaRoot");
        private static PathHook pathhook = (s) => System.IO.Path.Combine("LuaRoot", s);
        public static void SetPathHook(PathHook hook)
        {
            pathhook = hook;
        }

        public static FileLoadInfo OpenFile(string filename)
        {
            //var path = System.IO.Path.Combine(LUA_ROOT, filename);
            var path = pathhook(filename);
            return new FileLoadInfo(Modding.ModIO.Open(path, System.IO.FileMode.Open));
        }

        public static bool Readable(string filename)
        {
            //var path = System.IO.Path.Combine(LUA_ROOT, filename);
            var path = pathhook(filename);
            try
            {
                using (var stream = Modding.ModIO.Open(path, System.IO.FileMode.Open))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class FileLoadInfo : ILoadInfo, IDisposable
    {
        public FileLoadInfo(System.IO.Stream stream)
        {
            Reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8);
            Buf = new Queue<char>();
        }

        public int ReadByte()
        {
            if (Buf.Count > 0)
                return (int) Buf.Dequeue();
            else
                return Reader.Read();
        }

        public int PeekByte()
        {
            if (Buf.Count > 0)
                return (int) Buf.Peek();
            else
            {
                var c = Reader.Read();
                if (c == -1)
                    return c;
                Save((char) c);
                return c;
            }
        }

        public void Dispose()
        {
            Reader.Close();
        }

        private const string UTF8_BOM = "\u00EF\u00BB\u00BF";
        private System.IO.BinaryReader Reader;
        private Queue<char> Buf;

        private void Save(char b)
        {
            Buf.Enqueue(b);
        }

        public void SkipComment()
        {
            var c = Reader.Read();//SkipBOM();

            // first line is a comment (Unix exec. file)?
            if (c == '#')
            {
                do
                {
                    c = Reader.Read();
                } while (c != -1 && c != '\n');
                Save((char) '\n'); // fix line number
            }
            else if (c != -1)
            {
                Save((char) c);
            }
        }
    }

}

