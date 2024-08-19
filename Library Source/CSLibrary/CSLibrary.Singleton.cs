using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        private static volatile HighLevelInterface instance;
        
        private static object syncRoot = new Object();

        /// <summary>
        /// HighLevelInterface Instance, Thread Safe
        /// </summary>
        public static HighLevelInterface Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new HighLevelInterface();
                    }
                }

                return instance;
            }
        }
    }
}
