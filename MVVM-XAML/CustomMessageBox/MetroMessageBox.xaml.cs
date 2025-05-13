/*

   Copyright 2025 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
/*
COPYRIGHT 2013-2016 ESRI

TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the
Copyright Laws of the United States.

For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373

email: contracts@esri.com
*/

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Security.Permissions;
using System.Security;
using ArcGIS.Desktop.Internal.Framework.Metro;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Automation;

namespace ArcGIS.Desktop.Internal.Framework.Metro
{
  /// <summary>
  /// Interaction logic for InternalMessageBox.xaml
  /// </summary>
  internal partial class InternalMessageBox : MetroWindow
  {
    #region Private Members
    private const string Const_CancelButton = "CancelButton";
    private const string Const_NoButton = "NoButton";
    private const string Const_OkButton = "OkButton";
    private const string Const_YesButton = "YesButton";
    private const string Const_CloseButton = "PART_Close";
    private const string Const_MetroCloseButton = "closeButton";

    private MessageBoxButton _button = MessageBoxButton.OK;
    private MessageBoxResult _defaultResult = MessageBoxResult.None;
    private MessageBoxResult _dialogResult = MessageBoxResult.None;
    private string _helpContextID;

    #endregion //Private Members

    #region Constructor

    public InternalMessageBox()
    {
      InitializeComponent();

      // Check for RTL
      if (System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
        this.FlowDirection = System.Windows.FlowDirection.RightToLeft;
      else
        this.FlowDirection = System.Windows.FlowDirection.LeftToRight;

      this.SaveWindowPosition = false;

      this.DataContext = this;
      this.Visibility = Visibility.Collapsed;
      this.InitHandlers();
      this.Loaded += InternalMessageBox_Loaded;
      this.KeyDown += InternalMessageBox_KeyDown;

      this.MaxWidth = 380;
      this.MinWidth = 380;
    }

    private void InternalMessageBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        this.OnClose();
      }
    }

    #endregion //Constructor

    #region Dependency Properties

    #region ImageSource

    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(InternalMessageBox), new UIPropertyMetadata(default(ImageSource)));
    public ImageSource ImageSource
    {
      get
      {
        return (ImageSource)GetValue(ImageSourceProperty);
      }
      set
      {
        SetValue(ImageSourceProperty, value);
      }
    }

    #endregion //ImageSource

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InternalMessageBox), new UIPropertyMetadata(String.Empty));
    public string Text
    {
      get
      {
        return (string)GetValue(TextProperty);
      }
      set
      {
        SetValue(TextProperty, value);
      }
    }

    #endregion //Text

    #region Caption

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(InternalMessageBox), new UIPropertyMetadata(String.Empty));
    public string Caption
    {
      get
      {
        return (string)GetValue(CaptionProperty);
      }
      set
      {
        SetValue(CaptionProperty, value);
      }
    }

    #endregion

    #region ShowCheckbox

    public static readonly DependencyProperty ShowCheckboxProperty = DependencyProperty.Register("ShowCheckbox", typeof(bool), typeof(InternalMessageBox), new UIPropertyMetadata(false));
    public bool ShowCheckbox
    {
      get
      {
        return (bool)GetValue(ShowCheckboxProperty);
      }
      set
      {
        SetValue(ShowCheckboxProperty, value);
      }
    }

    #endregion

    #region NoRemind

    public static readonly DependencyProperty NoRemindProperty = DependencyProperty.Register("NoRemind", typeof(bool), typeof(InternalMessageBox), new UIPropertyMetadata(false));
    public bool NoRemind
    {
      get
      {
        return (bool)GetValue(NoRemindProperty);
      }
      set
      {
        SetValue(NoRemindProperty, value);
      }
    }

    #endregion

    #region CheckBoxCaption

    public static readonly DependencyProperty CheckBoxCaptionProperty = DependencyProperty.Register("CheckBoxCaptionProperty", typeof(string), typeof(InternalMessageBox), new UIPropertyMetadata(ArcGIS.Desktop.Framework.Properties.Resources.MessageBoxNag));
    public string CheckBoxCaption
    {
      get
      {
        return (string)GetValue(CheckBoxCaptionProperty);
      }
      set
      {
        SetValue(CheckBoxCaptionProperty, value);
      }
    }

    #endregion

    #region MoreInformationText

    public static readonly DependencyProperty MoreInformationTextProperty = DependencyProperty.Register("MoreInformationTextProperty", typeof(string), typeof(InternalMessageBox));
    public string MoreInformationText
    {
      get
      {
        return (string)GetValue(MoreInformationTextProperty);
      }
      set
      {
        SetValue(MoreInformationTextProperty, value);
      }
    }

    #endregion

    #region HelpLinkText

    public static readonly DependencyProperty HelpLinkTextProperty = DependencyProperty.Register("HelpLinkTextProperty", typeof(string), typeof(InternalMessageBox), new UIPropertyMetadata(ArcGIS.Desktop.Framework.Properties.Resources.LearnMore));
    public string HelpLinkText
    {
      get
      {
        return (string)GetValue(HelpLinkTextProperty);
      }
      set
      {
        SetValue(HelpLinkTextProperty, value);
      }
    }

    #endregion

    #region ShowLearnMore

    public static readonly DependencyProperty ShowLearnMoreProperty = DependencyProperty.Register("ShowLearnMoreProperty", typeof(bool), typeof(InternalMessageBox), new UIPropertyMetadata(false));
    public bool ShowLearnMore
    {
      get
      {
        return (bool)GetValue(ShowLearnMoreProperty);
      }
      set
      {
        SetValue(ShowLearnMoreProperty, value);
      }
    }

    #endregion

    #region ShowMoreInfo

    public static readonly DependencyProperty ShowMoreInfoProperty = DependencyProperty.Register("ShowMoreInfoProperty", typeof(bool), typeof(InternalMessageBox), new UIPropertyMetadata(false));
    public bool ShowMoreInfo
    {
      get
      {
        return (bool)GetValue(ShowMoreInfoProperty);
      }
      set
      {
        SetValue(ShowMoreInfoProperty, value);
      }
    }

    #endregion
    #endregion //Dependency Properties

    #region Public Properties

    public string OKButtonContent
      => string.IsNullOrEmpty(CustomOkButtonText) ? Desktop.Framework.Properties.Resources.OkButtonText
                                                  : CustomOkButtonText;

    public string CancelButtonContent
      => string.IsNullOrEmpty(CustomCancelButtonText) ? Desktop.Framework.Properties.Resources.CancelButtonText
                                                      : CustomCancelButtonText;

    public string YesButtonContent
      => string.IsNullOrEmpty(CustomYesButtonText) ? Desktop.Framework.Properties.Resources.YesButtonText
                                                   : CustomYesButtonText;

    public string NoButtonContent
      => string.IsNullOrEmpty(CustomNoButtonText) ? Desktop.Framework.Properties.Resources.NoButtonText
                                                  : CustomNoButtonText;

    public FlowDirection HelpFlowDirection
    {
      get
      {
        if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ar")
          return System.Windows.FlowDirection.RightToLeft;
        else
          return System.Windows.FlowDirection.LeftToRight;
      }
    }

    public string CustomOkButtonText { get; set; }
    public string CustomCancelButtonText { get; set; }
    public string CustomYesButtonText { get; set; }
    public string CustomNoButtonText { get; set; }
    #endregion

    #region MessageBoxResult

    /// <summary>
    /// Gets the MessageBox result, which is set when the "Closed" event is raised.
    /// </summary>
    public MessageBoxResult MessageBoxResult
    {
      get { return _dialogResult; }
    }

    #endregion //MessageBoxResult

    #region Base Class Override

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      ChangeVisualState(_button.ToString(), true);
    }

    internal string HelpContextID
    {
      get
      {
        return _helpContextID;
      }
    }

    #endregion

    #region Protected

    protected internal void InitializeMessageBox(Window owner, string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult, string helpContextID, string moreInfoText, string helpLinkText)
    {
      Text = text;
      Caption = caption;
      _button = button;
      _defaultResult = defaultResult;
      _helpContextID = helpContextID;
      ShowLearnMore = !string.IsNullOrEmpty(_helpContextID);
      ShowMoreInfo = !string.IsNullOrEmpty(moreInfoText);
      MoreInformationText = moreInfoText;

      if (!string.IsNullOrEmpty(helpLinkText))
        HelpLinkText = helpLinkText;

      SetImageSource(image);

      if (owner == null)
      {
        if (ArcGIS.Desktop.Framework.FrameworkApplication.Current.MainWindow.IsVisible == true)
          this.Owner = ArcGIS.Desktop.Framework.FrameworkApplication.Current.MainWindow;
        else
          this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      }
      else
        this.Owner = owner;

      this.ShowCloseButton = !object.Equals(_button, MessageBoxButton.YesNo) ? true : false;
    }

    // Changes the control's visual state(s).
    protected void ChangeVisualState(string name, bool useTransitions)
    {
      VisualStateManager.GoToElementState(buttonsGrid, name, useTransitions);
    }

    #endregion //Protected

    #region Private

    private void OnClose()
    {
      if (_dialogResult == System.Windows.MessageBoxResult.None)
        _dialogResult = object.Equals(_button, MessageBoxButton.OK) ? MessageBoxResult.OK : MessageBoxResult.Cancel;

      this.Visibility = Visibility.Collapsed;
      this.Close();
      this.OnClosed(EventArgs.Empty);
    }

    private void InternalMessageBox_Loaded(object sender, RoutedEventArgs e)
    {
      SetDefaultResult();

      // Fix for licensing message at startup with splashscreen on.
      if (this.Owner == null)
        this.Activate();

      // Programmatically adding hyperlink so it is not on the visual tree when the textblock is hidden, to fix narrator problem.
      if (ShowLearnMore && !ShowMoreInfo)
      {
        Run run = new Run();
        run.Text = ArcGIS.Desktop.Framework.Properties.Resources.LearnMore;
        run.SetValue(AutomationProperties.NameProperty, run.Text);
        Hyperlink hyperlink = new Hyperlink(new Run(ArcGIS.Desktop.Framework.Properties.Resources.LearnMore));
        hyperlink.Click += Help_Click;
        showLearnMoreTextblock.Inlines.Add(hyperlink);
        showLearnMoreTextblock.Visibility = Visibility.Visible;
      }
    }

    // Sets the button that represents the _defaultResult to the default button and gives it focus.
    private void SetDefaultResult()
    {
      Button defaultButton = GetDefaultButtonFromDefaultResult();
      if (defaultButton != null)
      {
        defaultButton.IsDefault = true;
        defaultButton.Focus();
      }
    }

    // Gets the default button from the _defaultResult.
    private Button GetDefaultButtonFromDefaultResult()
    {
      Button defaultButton = null;
      switch (_defaultResult)
      {
        case MessageBoxResult.Cancel:
          defaultButton = CancelButton;
          break;
        case MessageBoxResult.No:
          defaultButton = NoButton;
          break;
        case MessageBoxResult.OK:
          if (_button == MessageBoxButton.YesNo || _button == MessageBoxButton.YesNoCancel)
            defaultButton = YesButton;
          else
            defaultButton = OkButton;
          break;
        case MessageBoxResult.Yes:
          defaultButton = YesButton;
          break;
        case MessageBoxResult.None:
          defaultButton = GetDefaultButton();
          break;
      }
      return defaultButton;
    }

    // Gets the default button.
    private Button GetDefaultButton()
    {
      Button defaultButton = null;
      switch (_button)
      {
        case MessageBoxButton.OK:
        case MessageBoxButton.OKCancel:
          defaultButton = OkButton;
          break;
        case MessageBoxButton.YesNo:
        case MessageBoxButton.YesNoCancel:
          defaultButton = NoButton;
          break;
      }
      return defaultButton;
    }

    private void InitHandlers()
    {
      AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.Button_Click));

      CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(ExecuteCopy)));
    }

    // Sets the message image source.
    private void SetImageSource(MessageBoxImage image)
    {
      string iconName;

      switch (image)
      {
        case MessageBoxImage.Error:
          {
            iconName = "GenericError32";
            break;
          }
        case MessageBoxImage.Information:
          {
            iconName = "GenericInformation32";
            break;
          }
        case MessageBoxImage.Question:
          {
            iconName = "DialogQuestionAlert32";
            break;
          }
        case MessageBoxImage.Warning:
          {
            iconName = "GenericWarning32";
            break;
          }
        case MessageBoxImage.None:
        default:
          {
            return;
          }
      }

      //this.ImageSource = BitmapUtil.GetSystemIcon(iconID);
      // Use this syntax for other themes to get the icons
      this.ImageSource = Application.Current.Resources[iconName] as ImageSource;
    }

    // Sets the MessageBoxResult according to the button pressed and then closes the MessageBox.
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Button button = e.OriginalSource as Button;

      if (button == null)
        return;

      switch (button.Name)
      {
        case Const_NoButton:
          _dialogResult = MessageBoxResult.No;
          this.OnClose();
          break;
        case Const_YesButton:
          _dialogResult = MessageBoxResult.Yes;
          this.OnClose();
          break;
        case Const_CancelButton:
          _dialogResult = MessageBoxResult.Cancel;
          this.OnClose();
          break;
        case Const_OkButton:
          _dialogResult = MessageBoxResult.OK;
          this.OnClose();
          break;
        case Const_CloseButton:
        case Const_MetroCloseButton:
          this.OnClose();
          break;
      }

      e.Handled = true;
    }

    #endregion //Private

    #region Events
    #endregion

    #region COMMANDS

    private void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("---------------------------");
      sb.AppendLine();
      sb.Append(Caption);
      sb.AppendLine();
      sb.Append("---------------------------");
      sb.AppendLine();
      sb.Append(Text);
      sb.AppendLine();
      sb.Append("---------------------------");
      sb.AppendLine();
      switch (_button)
      {
        case MessageBoxButton.OK:
          sb.Append(OKButtonContent.ToString());
          break;
        case MessageBoxButton.OKCancel:
          sb.Append(OKButtonContent + "     " + CancelButtonContent);
          break;
        case MessageBoxButton.YesNo:
          sb.Append(YesButtonContent + "     " + NoButtonContent);
          break;
        case MessageBoxButton.YesNoCancel:
          sb.Append(YesButtonContent + "     " + NoButtonContent + "     " + CancelButtonContent);
          break;
      }
      sb.AppendLine();
      sb.Append("---------------------------");

      try
      {
        new UIPermission(UIPermissionClipboard.AllClipboard).Demand();
        Clipboard.SetText(sb.ToString());
      }
      catch (SecurityException)
      {
        throw new SecurityException();
      }
    }

    #endregion COMMANDS

    private void Help_Click(object sender, RoutedEventArgs e)
    {
      ArcGIS.Desktop.Framework.FrameworkApplication.ShowHelpTopic(HelpContextID);
    }
  }
}

namespace ArcGIS.Desktop.Framework.Dialogs
{
  /// <summary>
  /// Represents a dialog box for simple messages.
  /// </summary>
  public class MessageBox
  {
    // internal ctor
    internal MessageBox() { }

    #region Methods

    #region Public Static

    /// <summary>
    ///  Displays a message box that has a message and that returns a result.  
    /// </summary>
    /// <param name="messageText"></param>
    /// <returns>Specifies which message box button the user clicked.</returns>
    public static MessageBoxResult Show(string messageText)
    {
      return ShowCore(null, messageText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box that has a message and title bar caption; and that returns a result. 
    /// </summary>
    /// <param name="messageText">The message.</param>
    /// <param name="caption">The window caption.</param>
    /// <returns>Specifies which message box button the user clicked.</returns>
    public static MessageBoxResult Show(string messageText, string caption)
    {
      return ShowCore(null, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box that has a message and title bar caption; and that returns a result. 
    /// </summary>
    /// <param name="messageText">The message.</param>
    /// <param name="caption">The window caption.</param>
    /// <param name="helpContextID">Help ID.</param>
    /// <returns>Specifies which message box button the user clicked.</returns>
    public static MessageBoxResult Show(string messageText, string caption, string helpContextID)
    {
      return ShowCore(null, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, helpContextID, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message and returns a result. 
    /// </summary>
    /// <param name="owner">The parent window.</param>
    /// <param name="messageText">The message.</param>
    /// <returns>Specifies which message box button the user clicked.</returns>
    public static MessageBoxResult Show(Window owner, string messageText)
    {
      return ShowCore(owner, messageText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result. 
    /// </summary>
    /// <param name="owner">The parent window.</param>
    /// <param name="messageText">The message.</param>
    /// <param name="caption">The window caption.</param>
    /// <returns>Specifies which message box button the user clicked.</returns>
    public static MessageBoxResult Show(Window owner, string messageText, string caption)
    {
      return ShowCore(owner, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, and button; and that returns a result. 
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
    {
      return ShowCore(null, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result. 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button)
    {
      return ShowCore(owner, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result. 
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
      return ShowCore(null, messageText, caption, button, icon, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; 
    /// and it also returns a result. 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
      return ShowCore(owner, messageText, caption, button, icon, MessageBoxResult.None, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; 
    /// and accepts a default message box result and returns a result. 
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <param name="defaultResult"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
      return ShowCore(null, messageText, caption, button, icon, defaultResult, string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; 
    /// and accepts a default message box result, complies with the specified options, and returns a result. 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <param name="defaultResult"></param>
    /// <param name="helpContextID"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string helpContextID = "")
    {
      return ShowCore(owner, messageText, caption, button, icon, defaultResult, helpContextID, "", "");
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a checkbox where users may 
    /// opt to disable its appearance in the future.  
    /// The message box also displays a message, title bar caption, button, and icon. 
    /// The caller may also specify a default message box result. 
    /// On return, the ref Boolean argument will be set to true if the checkbox was checked by the user. 
    /// </summary>
    /// <param name="noRemind"></param>
    /// <param name="checkBoxMessage"></param>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <param name="defaultResult"></param>
    /// <param name="helpContextID"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(ref bool noRemind, string checkBoxMessage, Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string helpContextID = "")
    {
      // Shows MessageBox with checkbox on UI thread.
      if (FrameworkApplication.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
      {
        bool copy = noRemind;

        var tuple = FrameworkApplication.Current.Dispatcher.Invoke<MessageBoxResult>(() =>
        {
          return ShowInternalMessageBoxWithCheckbox(ref copy, checkBoxMessage, owner, messageText, caption, button, icon, defaultResult, helpContextID, "", "");
        });

        noRemind = copy;

        return tuple;

      }
      else
        return ShowInternalMessageBoxWithCheckbox(ref noRemind, checkBoxMessage, owner, messageText, caption, button, icon, defaultResult, helpContextID, "", "");
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; 
    /// and accepts a default message box result, complies with the specified options, and returns a result. 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <param name="defaultResult"></param>
    /// <param name="helpContextID"></param>
    /// <param name="moreInfoText"></param>
    /// <param name="helpLinkText"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string moreInfoText, string helpContextID = "", string helpLinkText = "")
    {
      return ShowCore(owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box displays a checkbox where users may 
    /// opt to disable its appearance in the future.  
    /// The message box also displays a message, title bar caption, button, and icon. 
    /// The caller may also specify a default message box result. 
    /// On return, the ref Boolean argument will be set to true if the checkbox was checked by the user. 
    /// </summary>
    /// <param name="noRemind"></param>
    /// <param name="checkBoxMessage"></param>
    /// <param name="owner"></param>
    /// <param name="messageText"></param>
    /// <param name="caption"></param>
    /// <param name="button"></param>
    /// <param name="icon"></param>
    /// <param name="defaultResult"></param>
    /// <param name="helpContextID"></param>
    /// <param name="moreInfoText"></param>
    /// <param name="helpLinkText"></param>
    /// <returns></returns>
    public static MessageBoxResult Show(ref bool noRemind, string checkBoxMessage, Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string moreInfoText, string helpContextID = "", string helpLinkText = "")
    {
      // Shows MessageBox with checkbox on UI thread.
      if (FrameworkApplication.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
      {
        bool copy = noRemind;

        var tuple = FrameworkApplication.Current.Dispatcher.Invoke<MessageBoxResult>(() =>
        {
          return ShowInternalMessageBoxWithCheckbox(ref copy, checkBoxMessage, owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText);
        });

        noRemind = copy;

        return tuple;

      }
      else
        return ShowInternalMessageBoxWithCheckbox(ref noRemind, checkBoxMessage, owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText);
    }

    #endregion //Public Static

    #region Internal
    internal static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, string okButtonText, string cancelButtonText, string yesButtonText,
      string noButtonText, MessageBoxImage image = MessageBoxImage.Question)
    {
      return ShowCore(owner:null, messageText, caption, button, icon:image, defaultResult:MessageBoxResult.None, helpContextID:string.Empty, moreInfoText:string.Empty, helpLinkText:string.Empty,
                      okButtonText, cancelButtonText, yesButtonText, noButtonText);
    }
    #endregion

    #region Private

    private static MessageBoxResult ShowCore(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string helpContextID, string moreInfoText, string helpLinkText,
      string okButtonText = null, string cancelButtonText = null, string yesButtonText = null, string noButtonText = null)
    {
      // Shows MessageBox on UI thread.
      if (FrameworkApplication.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
      {
        return FrameworkApplication.Current.Dispatcher.Invoke<MessageBoxResult>(() =>
        {
          return ShowInternalMessageBox(owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText, okButtonText, cancelButtonText, yesButtonText, noButtonText);
        });
      }
      else
        return ShowInternalMessageBox(owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText, okButtonText, cancelButtonText, yesButtonText, noButtonText);
    }

    private static MessageBoxResult ShowInternalMessageBox(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string helpContextID, string moreInfoText, string helpLinkText,
      string okButtonText, string cancelButtonText, string yesButtonText, string noButtonText)
    {
      InternalMessageBox msgBox = new InternalMessageBox();
      msgBox.InitializeMessageBox(owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText);
      msgBox.CustomOkButtonText = okButtonText;
      msgBox.CustomCancelButtonText = cancelButtonText;
      msgBox.CustomYesButtonText = yesButtonText;
      msgBox.CustomNoButtonText = noButtonText;

      msgBox.Visibility = Visibility.Visible;
      msgBox.ShowDialog();

      return msgBox.MessageBoxResult;
    }

    private static MessageBoxResult ShowInternalMessageBoxWithCheckbox(ref bool noRemind, string checkBoxMessage, Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, string helpContextID, string moreInfoText, string helpLinkText)
    {
      // Show nag checkbox
      InternalMessageBox msgBox = new InternalMessageBox();
      msgBox.NoRemind = noRemind;
      msgBox.InitializeMessageBox(owner, messageText, caption, button, icon, defaultResult, helpContextID, moreInfoText, helpLinkText);

      msgBox.ShowCheckbox = true;

      if (!string.IsNullOrEmpty(checkBoxMessage))
        msgBox.CheckBoxCaption = checkBoxMessage;

      msgBox.Visibility = Visibility.Visible;

      msgBox.ShowDialog();

      noRemind = msgBox.NoRemind;

      return msgBox.MessageBoxResult;
    }
    #endregion

    #endregion //Methods
  }
}
