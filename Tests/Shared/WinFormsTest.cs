using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xunit;

namespace IntegrationTests
{
    internal static class WinFormsTest
    {
        internal static void Run(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception exception) { failure = exception; }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "The STA test did not finish.");
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        }

        internal static T Field<T>(object instance, string name)
        {
            return (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(instance);
        }

        internal static void Invoke(object instance, string method, params object[] arguments)
        {
            try
            {
                instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(instance, arguments);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            }
        }
    }
}
