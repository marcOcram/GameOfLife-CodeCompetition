using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GameOfLifeWPF.Services
{
    /// <summary>
    /// A service for interacting with the user. It's not the "real" MVVM way but it's enough for our application.
    /// </summary>
    internal class InteractivityService
    {
        #region Public Methods

        /// <summary>
        /// Asks the user a question which can be answered with yes / no.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="question">The question.</param>
        /// <returns></returns>
        public bool Ask(string title, string question)
        {
            return MessageBox.Show(question, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// The user should confirm something.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="confirmation">The confirmation.</param>
        /// <returns></returns>
        public bool Confirm(string title, string confirmation)
        {
            return MessageBox.Show(confirmation, title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        /// <summary>
        /// Gets a file path from the user.
        /// </summary>
        /// <param name="isWrite">if set to <c>true</c> the user is asked to select a file to write; otherwise to select a file to read from.</param>
        /// <returns></returns>
        public string GetFilePath(bool isWrite)
        {
            FileDialog fileDialog;
            if (isWrite) {
                fileDialog = new SaveFileDialog();
            } else {
                fileDialog = new OpenFileDialog();
            }

            fileDialog.Filter = "Conways Game Of Life Save (*.cgol)|*.cgol";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value) {
                return fileDialog.FileName;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Notifies the user about something.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="notification">The notification.</param>
        public void Notify(string title, string notification)
        {
            MessageBox.Show(notification, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Public Methods
    }
}