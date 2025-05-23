﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;
using System.Diagnostics;
using Turbine.Producer.Contracts;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;


namespace Turbine.Producer
{
    public class Container
    {
        private static IUnityContainer container = new UnityContainer();
        static Container()
        {
            try
            {
                var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
                section.Configure(container, "producerX");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("{0}: {1}", ex.ToString(), ex.Message), 
                    "Turbine.Producer.Container");
                Debug.WriteLine(ex.StackTrace, 
                    "Turbine.Producer.Container");
                throw;
            }
        }

        public static IConsumerResourceAccessor GetConsumerResourceAccessor() 
        {
            try
            {
                return container.Resolve<IConsumerResourceAccessor>();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message, 
                    "Turbine.Producer.Container.GetConsumerResourceAccessor");
                Debug.WriteLine(ex.InnerException, 
                    "Turbine.Producer.Container.GetConsumerResourceAccessor");
                throw;
            }
        }

        public static IUnityContainer GetProducerContainer()
        {
            return container;
        }

        public static IProducerContext GetAppContext()
        {
            try
            {
                return container.Resolve<IProducerContext>();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message, 
                    "Turbine.Producer.Container.GetAppContext");
                Debug.WriteLine(ex.InnerException, 
                    "Turbine.Producer.Container.GetAppContext");
                throw;
            }
        }
    }
}