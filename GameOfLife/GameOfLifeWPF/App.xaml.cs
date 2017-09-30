using GameOfLifeWPF.Properties;
using GameOfLifeWPF.Services;
using GameOfLifeWPF.View;
using GameOfLifeWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GameOfLifeWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Public Constructors

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleUnhandledException(e.ExceptionObject as Exception);
            DispatcherUnhandledException += (s, e) => {
                e.Handled = true;
                HandleUnhandledException(e.Exception);
            };
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Settings.Default.Save();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InteractivityService interactivityService = new InteractivityService();
            GameService gameService = new GameService();

            ShellViewModel shellViewModel = new ShellViewModel(gameService, interactivityService, Current.Dispatcher);

            shellViewModel.ExitRequest += (s, e2) => Shutdown(0);

            Shell shell = new Shell {
                DataContext = shellViewModel
            };
            shell.Show();
        }

        #endregion Protected Methods

        #region Private Methods

        private void HandleUnhandledException(Exception ex)
        {
            MessageBox.Show($"Ooops! Something went wrong! {Environment.NewLine}{ex.Message}", "Fatal Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }

        #endregion Private Methods
    }
}