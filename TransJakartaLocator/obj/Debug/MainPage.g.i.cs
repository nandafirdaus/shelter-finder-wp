﻿#pragma checksum "D:\Project\Visual Studio 2012\Windows Phone\TransJakartaLocator\TransJakartaLocator\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3AC0CBD82891AFE6D42D87A6033983C0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace TransJakartaLocator {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Image ButtonFromPoint;
        
        internal System.Windows.Controls.Image ButtonNearest;
        
        internal System.Windows.Controls.Image ButtonAbout;
        
        internal System.Windows.Controls.Image ButtonHelp;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/TransJakartaLocator;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.ButtonFromPoint = ((System.Windows.Controls.Image)(this.FindName("ButtonFromPoint")));
            this.ButtonNearest = ((System.Windows.Controls.Image)(this.FindName("ButtonNearest")));
            this.ButtonAbout = ((System.Windows.Controls.Image)(this.FindName("ButtonAbout")));
            this.ButtonHelp = ((System.Windows.Controls.Image)(this.FindName("ButtonHelp")));
        }
    }
}

