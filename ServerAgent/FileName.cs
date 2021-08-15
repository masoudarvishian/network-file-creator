using System;
using System.Collections.Generic;
using System.IO;

namespace ServerAgent
{
    internal static class FileName
    {
        internal static Queue<string> Collection = new Queue<string>();

        private static string _current;

        public static string Current
        {
            get { return _current; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // store previous file name, if file exists
                    if (File.Exists(_current))
                    {
                        Collection.Enqueue(_current);
                    }

                    _current = value;
                }
                else
                {
                    throw new ArgumentNullException("value cannot be null or empty!");
                }
            }
        }
    }
}
