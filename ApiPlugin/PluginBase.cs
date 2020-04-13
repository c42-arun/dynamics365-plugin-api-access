using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace ApiPlugin
{
    /// <summary>
    /// Adapted from https://github.com/rappen/JonasPluginBase/blob/master/JonasPluginBase/JonasPluginBase.cs &
    /// https://github.com/rappen/JonasPluginBase/blob/master/JonasPluginBase/JonasPluginBag.cs
    /// </summary>
    public abstract class PluginBase : IPlugin
    {

        protected SpfTracingService TracingService { get; set; }
        protected IOrganizationService Service { get; set; }
        protected string PluginName => GetType().Name;
        protected string MessageName => Context != null ? Context.MessageName : string.Empty;
        protected int Depth => Context?.Depth ?? int.MaxValue;
        protected ParameterCollection SharedVariables => Context.SharedVariables;

        private IPluginExecutionContext Context { get; set; }
        public void Execute(IServiceProvider serviceProvider)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                TracingService = new SpfTracingService((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
                Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                Service = serviceFactory.CreateOrganizationService(Context.InitiatingUserId);

                TracingService.Trace($"In {PluginName} plugin");

                if (!Context.InputParameters.Contains("Target"))
                {
                    TracingService.Trace($"{PluginName}: Context does not contain 'Target' attribute");
                    return;
                }

                object targetValue = Context.InputParameters["Target"];

                if (targetValue == null)
                    return;

                if (targetValue is Entity)
                {
                    // Obtain the target entity from the input parameters.
                    var entity = (Entity)targetValue;
                    ExecuteOnEntity(entity);
                }
                else if (targetValue is EntityReference)
                {
                    var entityReference = (EntityReference)targetValue;
                    ExecuteOnEntityReference(entityReference);
                }
            }
            catch (Exception e)
            {
                TraceBlockStart(e.GetType().ToString());
                Trace(e.ToString());
                TraceBlockEnd();
                throw new InvalidPluginExecutionException($"An error occured in the {PluginName} plugin.", e);
            }
            finally
            {
                watch.Stop();
                Trace($"Internal execution time for {PluginName} plugin : {0} ms", watch.ElapsedMilliseconds);
            }
        }

        protected virtual void ExecuteOnEntity(Entity entity)
        {
            // no-op by default 
        }

        protected virtual void ExecuteOnEntityReference(EntityReference entityReference)
        {
            // no op by default
        }

        protected Entity GetPreImage(string imageName)
        {
            return Context.PreEntityImages.Contains(imageName) ? Context.PreEntityImages[imageName] : null;
        }

        protected Entity GetPostImage(string imageName)
        {
            return Context.PostEntityImages.Contains(imageName) ? Context.PostEntityImages[imageName] : null;
        }

        protected T GetSharedVariable<T>(string variableName) where T : class
        {
            T sharedVariable = Context.SharedVariables.Contains(variableName)
                ? Context.SharedVariables[variableName] as T
                : default(T);

            return sharedVariable;
        }

        /// <summary>
        /// Call this function to start a block in the log.
        /// Log lines will be indented, until next call to TraceBlockEnd.
        /// Block label with be the name of the calling method.
        /// </summary>
        protected void TraceBlockStart()
        {
            var label = new StackTrace().GetFrame(1).GetMethod().Name;
            TraceBlockStart(label);
        }

        /// <summary>
        /// Trace method automatically adding timestamp to each traced item
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        /// <summary>
        /// Call this to en a block in the log.
        /// </summary>
        protected void TraceBlockEnd()
        {
            TracingService.BlockEnd();
        }

        /// <summary>
        /// Call this function to start a block in the log.
        /// Log lines will be indented, until next call to TraceBlockEnd.
        /// </summary>
        /// <param name="label">Label to set for the block</param>
        protected void TraceBlockStart(string label)
        {
            TracingService.BlockBegin(label);
        }
    }
}
