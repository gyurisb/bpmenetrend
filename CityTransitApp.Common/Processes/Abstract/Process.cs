using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.Processes
{
    public abstract class Process<TDerived, TProg, TRes> where TDerived : Process<TDerived, TProg, TRes>, new()
    {
        public event EventHandler<TProg> ProgressChanged;
        protected ICommonServices Services = CommonComponent.Current.Services;

        protected void PerformProgressChanged(TProg args)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, args);
        }

        protected virtual TRes Start()
        {
            throw new NotImplementedException();
        }
        protected virtual TRes Start(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task<TRes> StartAsync()
        {
            return await Task.Run((Func<TRes>)Start);
        }

        protected virtual async Task<TRes> StartAsync(params object[] parameters)
        {
            return await Task.Run(() => Start(parameters));
        }

        public static TRes Run()
        {
            TDerived proc = new TDerived();
            return proc.Start();
        }
        public static TRes Run(params object[] parameters)
        {
            TDerived proc = new TDerived();
            return proc.Start(parameters);
        }
        public static async Task<TRes> RunAsync(Action<TProg> progressCallback = null)
        {
            TDerived proc = new TDerived();
            if (progressCallback != null)
                proc.ProgressChanged += (sender, arg) => progressCallback(arg);
            return await proc.StartAsync();
        }
        public static async void RunAsync(Action<TProg> progressCallback, Action<TRes> resultCallback)
        {
            TDerived proc = new TDerived();
            if (progressCallback != null)
                proc.ProgressChanged += (sender, arg) => progressCallback(arg);
            resultCallback(await proc.StartAsync());
        }
        public static async Task<TRes> RunWithParametersAsync(params object[] parameters)
        {
            TDerived proc = new TDerived();
            return await proc.StartAsync(parameters);
        }

        //public static TRes Run<TDerived>() where TDerived : Process<TDerived, TProg, TRes>, new()
        //{
        //    var derivedType = MethodBase.GetCurrentMethod().DeclaringType;
        //    object process = Activator.CreateInstance(derivedType);
        //    derivedType.GetMethod("Start").Invoke(process, null);

        //    TDerived proc = new TDerived();
        //    return proc.Start();
        //}
    }
}
