﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class IRCConfig
    {
        public bool Enabled { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int MaxMessageLength { get; set; }

        public IRCConfig()
        {
            Host = string.Empty;
            Port = 0;
            MaxMessageLength = 400;
        }
    }
}
