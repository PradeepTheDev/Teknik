﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Stream.ViewModels
{
    public class StreamViewModel : ViewModelBase
    {
        public List<string> Streams;

        public StreamViewModel()
        {
            Streams = new List<string>();
        }
    }
}
