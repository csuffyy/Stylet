﻿using Stylet.Logging;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Stylet
{
    /// <summary>
    /// Bootstrapper to be extended by applications which don't want to use StyletIoC as the IoC container.
    /// </summary>
    public abstract class BootstrapperBase : IBootstrapper, IViewManagerConfig, IDisposable
    {
        /// <summary>
        /// Gets the current application
        /// </summary>
        public Application Application { get; private set; }

        /// <summary>
        /// Gets or sets assemblies which are used for IoC container auto-binding and searching for Views.
        /// Set this in Configure() if you want to override it
        /// </summary>
        public IReadOnlyList<Assembly> Assemblies { get; protected set; }

        /// <summary>
        /// Gets the command line arguments that were passed to the application from either the command prompt or the desktop.
        /// </summary>
        public string[] Args { get; private set; }

        /// <summary>
        /// Gets the instance of the root ViewMode, which is displayed at launch
        /// </summary>
        protected abstract object RootViewModel { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BootstrapperBase"/> class
        /// </summary>
        protected BootstrapperBase()
        {
            this.Assemblies = new List<Assembly>() { typeof(BootstrapperBase).Assembly, this.GetType().Assembly };
        }

        /// <summary>
        /// Called by the ApplicationLoader when this bootstrapper is loaded
        /// </summary>
        /// <param name="application">Application within which Stylet is running</param>
        public void Setup(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            this.Application = application;

            // Use the current application's dispatcher for Execute
            Execute.Dispatcher = new DispatcherWrapper(this.Application.Dispatcher);

            this.Application.Startup += (o, e) => this.Start(e.Args);
            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Exit += (o, e) =>
            {
                this.OnExit(e);
                this.Dispose();
            };

            // Fetch this logger when needed. If we fetch it now, then no-one will have been given the change to enable the LogManager, and we'll get a NullLogger
            this.Application.DispatcherUnhandledException += (o, e) =>
            {
                LogManager.GetLogger(typeof(BootstrapperBase)).Error(e.Exception, "Unhandled exception");
                this.OnUnhandledException(e);
            };
        }

        /// <summary>
        /// Called on Application.Startup, this does everything necessary to start the application
        /// </summary>
        /// <param name="args">Command-line arguments used to start this executable</param>
        public virtual void Start(string[] args)
        {
            // Set this before anything else, so everything can use it
            this.Args = args;
            this.OnStart();

            this.ConfigureBootstrapper();

            View.ViewManager = (IViewManager)this.GetInstance(typeof(IViewManager));

            this.Launch();
            this.OnLaunch();
        }

        /// <summary>
        /// Launch the root view
        /// </summary>
        protected virtual void Launch()
        {
            var windowManager = (IWindowManager)this.GetInstance(typeof(IWindowManager));
            windowManager.ShowWindow(this.RootViewModel);
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void ConfigureBootstrapper() { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type of instance to fetch</param>
        /// <returns>Fetched instance</returns>
        public abstract object GetInstance(Type type);

        /// <summary>
        /// Called on application startup. This occur after this.Args has been assigned, but before the IoC container has been configured
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Called just after the root View has been displayed
        /// </summary>
        protected virtual void OnLaunch() { }

        /// <summary>
        /// Hook called on application exit
        /// </summary>
        /// <param name="e">The exit event data</param>
        protected virtual void OnExit(ExitEventArgs e) { }

        /// <summary>
        /// Hook called on an unhandled exception
        /// </summary>
        /// <param name="e">The event data</param>
        protected virtual void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e) { }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose() { }
    }
}
