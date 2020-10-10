using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

using CefSharp;
using CefSharp.Wpf;

using FFmpeg.AutoGen;
using Unosquare.FFME;

using Scrapper.BrowserHandlers;
using Scrapper.Model;
using Scrapper.Extension;

namespace Scrapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CurrentPath { get; set; }
        public static string DataPath { get; set; }
        public static string JavPath { get; set; }
        public static string LocalAppData { get; set; }
        public static AvDbContext DbContext { get; set; }

        /// <summary>
        /// Determines if the Application is in design mode.
        /// </summary>
        public static bool IsInDesignMode => !(Current is App) ||
            (bool)DesignerProperties.IsInDesignModeProperty
                .GetMetadata(typeof(DependencyObject)).DefaultValue;

        public App()
        {
            log4net.Config.XmlConfigurator.Configure();
            CurrentPath = Directory.GetCurrentDirectory();
            DataPath = @"d:\tmp\";
            JavPath = @"d:\JAV\";
            LocalAppData = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\");
            var di = new DirectoryInfo(DataPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(DataPath);
            }

            // Change the default location of the ffmpeg binaries 
            // (same directory as application)
            Library.FFmpegDirectory = @"c:\ffmpeg" +
                (Environment.Is64BitProcess ? @"\x64" : string.Empty);

            // You can pick which FFmpeg binaries are loaded. See issue #28
            // For more specific control (issue #414) you can set
            // Library.FFmpegLoadModeFlags to: FFmpegLoadMode.LibraryFlags["avcodec"]
            //  | FFmpegLoadMode.LibraryFlags["avfilter"] | ... etc.
            // Full Features is already the default.
            Library.FFmpegLoadModeFlags = FFmpegLoadMode.FullFeatures;

            // Multi-threaded video enables the creation of independent
            // dispatcher threads to render video frames. This is an experimental
            // feature and might become deprecated in the future as no real
            // performance enhancements have been detected.
            //Library.EnableWpfMultiThreadedVideo = !Debugger.IsAttached;
            // test with true and false

            DbContext = new AvDbContext("avDb");
            var studio = new AvStudio { Name = "Init" };
            if (!DbContext.Studios.Any())
            {
                DbContext.Studios.Add(studio);
                DbContext.SaveChanges();
            }
        }

        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));
            if (string.IsNullOrEmpty(resourcePath))
                return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        void InitCefSharp()
        {
            IBrowserProcessHandler browserProcessHandler =
                new ProcessHandler();

            var settings = new CefSettings
            {
                MultiThreadedMessageLoop = true,
                ExternalMessagePump = false,
                RemoteDebuggingPort = 8088,

                //Enables Uncaught exception handler
                UncaughtExceptionStackSize = 10
            };

            // Off Screen rendering (WPF/Offscreen)
            if (settings.WindowlessRenderingEnabled)
            {
                //Disable Direct Composition to test 
                // https://github.com/cefsharp/CefSharp/issues/1634
                //settings.CefCommandLineArgs.Add("disable-direct-composition");

                // DevTools doesn't seem to be working when this is enabled
                // http://magpcss.org/ceforum/viewtopic.php?f=6&t=14095
                settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling");
            }
            settings.CefCommandLineArgs.Add("enable-experimental-web-platform-features");

            //The location where cache data will be stored on disk. If empty an in-memory cache will be used for some features and a temporary disk cache for others.
            //HTML5 databases such as localStorage will only persist across sessions if a cache path is specified. 
            settings.CachePath = LocalAppData + "CEF\\cache";
            //settings.CachePath = LocalAppData + @"Google\Chrome\User Data\Default";

            //This must be set before Cef.Initialized is called
            CefSharpSettings.FocusedNodeChangedEnabled = true;

            //Async Javascript Binding - methods are queued on TaskScheduler.Default.
            //Set this to true to when you have methods that return Task<T>
            //CefSharpSettings.ConcurrentTaskExecution = true;

            //Legacy Binding Behaviour - Same as Javascript Binding in version 57 and below
            //See issue https://github.com/cefsharp/CefSharp/issues/1203 for details
            //CefSharpSettings.LegacyJavascriptBindingEnabled = true;

            //Exit the subprocess if the parent process happens to close
            //This is optional at the moment
            //https://github.com/cefsharp/CefSharp/pull/2375/
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            if (!Cef.Initialize(settings, performDependencyCheck: false,
                browserProcessHandler: browserProcessHandler))
            {
                throw new Exception("Unable to Initialize Cef");
            }
        }

        async void PreLoadFFmpeg()
        {
            // Pre-load FFmpeg libraries in the background. This is optional.
            // FFmpeg will be automatically loaded if not already loaded
            // when you try to open a new stream or file. See issue #242
            try
            {
                // Pre-load FFmpeg
                await Library.LoadFFmpegAsync();
            }
            catch (Exception ex)
            {
                var dispatcher = Current?.Dispatcher;
                if (dispatcher == null)
                    return;

                await dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(MainWindow,
                        $"Unable to Load FFmpeg Libraries from path:\r\n    {Library.FFmpegDirectory}" +
                        $"\r\nMake sure the above folder contains FFmpeg shared binaries (dll files) for the " +
                        $"applicantion's architecture ({(Environment.Is64BitProcess ? "64-bit" : "32-bit")})" +
                        $"\r\nTIP: You can download builds from https://ffmpeg.zeranoe.com/builds/" +
                        $"\r\n{ex.GetType().Name}: {ex.Message}\r\n\r\nApplication will exit.",
                        "FFmpeg Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Current?.Shutdown();
                }));
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();

            InitCefSharp();
            Task.Run(() => PreLoadFFmpeg());
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject,
                    "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception,
                    "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception,
                    "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                AssemblyName assemblyName
                    = Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}",
                    assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Exception in LogUnhandledException");
                Log.Print("Exception in LogUnhandledException", ex);
            }
            finally
            {
                //_logger.Error(exception, message);
                Log.Print(message, exception);
                if (App.Current != null)
                    App.Current.Shutdown(1);
            }
        }
    }
}
