﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Contract.Behaviors;

namespace GPromsSinterConsumerWindowsService
{
    class WindowsServiceContext : IContext
    {

        public string BaseWorkingDirectory
        {
            get {
                return Properties.Settings.Default.BaseDirectory;
            }
        }
    }
}
