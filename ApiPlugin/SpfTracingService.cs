﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace ApiPlugin
{
    /// <summary>
    /// https://github.com/rappen/JonasPluginBase/blob/master/JonasPluginBase/JonasTracingService.cs
    /// </summary>
    public class SpfTracingService : ITracingService, IDisposable
    {
        private readonly ITracingService trace;
        private List<string> blockstack = new List<string>();

        /// <summary>
        /// Set this property to True to enable extensive tracing of details regarding queries, entities etc.
        /// Note! This may affect performance!
        /// </summary>
        public bool Verbose { get; set; } = false;

        public SpfTracingService(ITracingService Trace)
        {
            trace = Trace;
            this.Trace("*** Enter");
        }

        public void Dispose()
        {
            if (blockstack.Count > 0)
            {
                trace.Trace("[SpfTracingService] Ending unended blocks - check code consistency!");
                while (blockstack.Count > 0)
                {
                    BlockEnd();
                }
            }
            Trace("*** Exit");
        }

        public void Trace(string format, params object[] args)
        {
            if (trace != null)
            {
                var indent = new string(' ', blockstack.Count * 2);
                var s = string.Format(format, args);
                trace.Trace(DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + indent + s);
            }
        }

        internal void BlockBegin(string label)
        {
            Trace("BEGIN {0}", label);
            blockstack.Add(label);
        }

        internal void BlockEnd()
        {
            var label = "?";
            var pos = blockstack.Count - 1;
            if (pos >= 0)
            {
                label = blockstack[pos];
                blockstack.RemoveAt(pos);
            }
            Trace("END {0}", label);
        }
    }
}
