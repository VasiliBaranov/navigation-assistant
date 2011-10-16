using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Contains all the customizable settings for the Navigation Assistant.
    /// </summary>
    public class Settings : ICloneable
    {
        #region Basic

        /// <summary>
        /// Gets or sets the primary navigator 
        /// (a program to be run after folder selection if the navigation assistant was launched above non of the supported navigators).
        /// </summary>
        /// <value>
        /// The primary navigator.
        /// </value>
        public Navigators PrimaryNavigator { get; set; }

        /// <summary>
        /// Gets or sets the supported navigators, i.e. programs where navigation will be assisted.
        /// </summary>
        /// <value>
        /// The supported navigators.
        /// </value>
        public List<Navigators> SupportedNavigators { get; set; }

        public string TotalCommanderPath { get; set; }

        #endregion

        #region Advanced

        /// <summary>
        /// Gets or sets the folders to parse (i.e. root folders) when searching for folder matches;
        /// e.g. "C:\Documents and Settings".
        /// </summary>
        /// <value>
        /// The folders to parse.
        /// </value>
        public List<string> FoldersToParse { get; set; }

        /// <summary>
        /// Gets or sets the exclude folder templates; i.e. directories to be ignored while displaying matches, 
        /// e.g. "temp","Recycle Bin". Each entry is a case-sensitive regular expression.
        /// </summary>
        /// <value>
        /// The exclude folder templates.
        /// </value>
        public List<string> ExcludeFolderTemplates { get; set; }

        /// <summary>
        /// Gets or sets the global key combination, i.e. a key combination that brings the search window to the foreground.
        /// </summary>
        /// <value>
        /// The global key combination.
        /// </value>
        /// <remarks>
        /// Keys enumeration is weird, and is serialized in a weird way.
        /// E.g. "Control, Shift, M" is serialized as "LButton MButton XButton1 Back Tab Clear Return Enter A D E H I L M Shift Control".
        /// </remarks>
        [XmlIgnore]
        public Keys GlobalKeyCombination { get; set; }

        /// <summary>
        /// Gets or sets the global key combination string (used for serialization only).
        /// </summary>
        /// <value>
        /// The global key combination string.
        /// </value>
        public string GlobalKeyCombinationString
        {
            get
            {
                return GlobalKeyCombination.ToString();
            }
            set
            {
                GlobalKeyCombination = (Keys) Enum.Parse(typeof (Keys), value);
            } 
        }

        /// <summary>
        /// Gets or sets a value indicating whether Navigation Assistant is run on Windows startup.
        /// </summary>
        /// <value>
        ///   <c>true</c> if run on startup; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This setting is set through registry.
        /// </remarks>
        [XmlIgnore]
        public bool RunOnStartup { get; set; }

        #endregion

        public object Clone()
        {
            Settings clone = new Settings();

            clone.PrimaryNavigator = PrimaryNavigator;
            clone.SupportedNavigators = SupportedNavigators;
            clone.TotalCommanderPath = TotalCommanderPath;
            clone.FoldersToParse = FoldersToParse;
            clone.ExcludeFolderTemplates = ExcludeFolderTemplates;
            clone.GlobalKeyCombination = GlobalKeyCombination;
            clone.RunOnStartup = RunOnStartup;

            return clone;
        }
    }
}
