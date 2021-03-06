using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace AutoComment
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private class CommandInfo
        {
            public readonly string _commandName;
            public readonly string _commandCaption;
            public readonly string _commandTooltip;
            public readonly int _commandIcon;

            public CommandInfo(string name, string caption, string tooltip, int icon)
            {
                _commandName = name;
                _commandCaption = caption;
                _commandTooltip = tooltip;
                _commandIcon = icon;
            }
        }

		private DTE2 _applicationObject;
		private AddIn _addInInstance;
        // used to spew output in installed version of the add-in
        private OutputWindowPane _owP;
        // need to keep menu around to remove it when the add-in is unloaded.
        private CommandBarPopup _toolsSubmenu;
        private List<CommandInfo> _commandInfos;

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
            _commandInfos = new List<CommandInfo>();
            _commandInfos.Add(new CommandInfo("InsertCComment", "InsertCComment", "Inserts a one line C style comment.", 0));
            _commandInfos.Add(new CommandInfo("InsertCCommentWithDateStamp", "InsertCCommentWithDateStamp", "Inserts a one line C style comment with the current date.", 0));
            _commandInfos.Add(new CommandInfo("InsertCppComment", "InsertCppComment", "Inserts a one line C++ style comment.", 0));
            _commandInfos.Add(new CommandInfo("InsertCppCommentWithDateStamp", "InsertCppCommentWithDateStamp", "Inserts a one line C++ style comment with the current date.", 0));
            _commandInfos.Add(new CommandInfo("InsertFiraxisHeader", "InsertFiraxisHeader", "Inserts the standard Firaxis header at the top of the file.", 0));
            _commandInfos.Add(new CommandInfo("InsertFiraxisAddition", "InsertFiraxisAddition", "Inserts the Firaxis addition note.", 0));
            _commandInfos.Add(new CommandInfo("InsertFiraxisBegin", "InsertFiraxisBegin", "Inserts the Firaxis begin note.", 0));
            _commandInfos.Add(new CommandInfo("InsertFiraxisEnd", "InsertFiraxisEnd", "Inserts the Firaxis end note.", 0));
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
            CreateOutputWindowPane(_applicationObject);

            switch (connectMode)
            {
                case ext_ConnectMode.ext_cm_UISetup:
                    // this is where permanent UI elements go. since we are going to have an extensible interface, we will create the UI on each startup -tsmith
                    OutputWindowPaneLog("AutoComment: OnConnection: ext_ConnectMode.ext_cm_UISetup");
                    CreateCommands();
                    break;
                case ext_ConnectMode.ext_cm_Startup:
                    // Do nothing yet, wait until the IDE is fully initialized (OnStartupComplete will be called)
                    OutputWindowPaneLog("AutoComment: OnConnection: ext_ConnectMode.ext_cm_Startup");
                    break;
                case ext_ConnectMode.ext_cm_AfterStartup:
                    // this is called after the UI has been enabled using the Add-In Manager
                    OutputWindowPaneLog("AutoComment: OnConnection: ext_ConnectMode.ext_cm_AfterStartup");
                    InitializeAddIn();
                    break;
            }
		}



		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            try
            {
                switch(disconnectMode)
                {
                    case ext_DisconnectMode.ext_dm_HostShutdown:
                    case ext_DisconnectMode.ext_dm_UserClosed:
                        if (_toolsSubmenu != null)
                        {
                            OutputWindowPaneLog("AutoComment: OnDisconnection: Mode=" + disconnectMode + ", Removing _toolsSubmenu");
                            // recreate the commands because that saves the keybinding and destroys the command. destroying the command also deletes the UI control.
                            RecreateCommands();
                            _toolsSubmenu.Delete();
                        }
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
            OutputWindowPaneLog("AutoComment: OnStartupComplete:");
            InitializeAddIn();
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if(commandName == "AutoComment.Connect.InsertCComment")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCCommentWithDateStamp")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCppComment")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCppCommentWithDateStamp")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisHeader")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisAddition")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisBegin")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisEnd")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if(commandName == "AutoComment.Connect.InsertCComment")
				{
					handled = true;
                    InsertCComment();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCCommentWithDateStamp")
				{
					handled = true;
                    InsertCCommentWithDateStamp();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCppComment")
				{
					handled = true;
                    InsertCppComment();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertCppCommentWithDateStamp")
				{
					handled = true;
                    InsertCppCommentWithDateStamp();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisHeader")
				{
					handled = true;
                    InsertFiraxisHeader();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisAddition")
				{
					handled = true;
                    InsertFiraxisAddition();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisBegin")
				{
					handled = true;
                    InsertFiraxisBegin();
					return;
				}
				else if(commandName == "AutoComment.Connect.InsertFiraxisEnd")
				{
					handled = true;
                    InsertFiraxisEnd();
					return;
				}
			}
		}

        private  void CreateOutputWindowPane(DTE2 dte)
        {
            // Create a tool window reference for the Output window
            // and window pane.
            if(_owP == null)
            {
                OutputWindow ow = dte.ToolWindows.OutputWindow;
                foreach(OutputWindowPane owp in ow.OutputWindowPanes)
                {
                    if(owp.Name == "AutoComment")
                    {
                        _owP = owp;
                        return;
                    }

                }
                // Add a new pane to the Output window.
                _owP = ow.OutputWindowPanes.Add("AutoComment");
            }
        }

        private void OutputWindowPaneLog(string strLog)
        {
            if(_owP != null)
            {
                _owP.OutputString(strLog + "\n");
            }
        }

        private void RecreateCommands()
        {
            foreach(CommandInfo ci in _commandInfos)
            {
                RecreateCommand(ci._commandName, ci._commandCaption, ci._commandTooltip, ci._commandIcon);
            }
        }

        private void RecreateCommand(string commandShortName, string commandCaption, string commandTooltip, int commandImage)
        {
            Command existingCommand = null;
            object bindings = null;
            string commandFullName = null;

            commandFullName = _addInInstance.ProgID + "." + commandShortName;

            // Try to get the command if it exists to get the current binding
            try
            {
                existingCommand = _applicationObject.Commands.Item(commandFullName, -1);
            }
            catch
            {
            }

            if (existingCommand == null)
            {
                // This should not happen!
            }
            else
            {
                // We must preserve the command bindings
                bindings = existingCommand.Bindings;

                // Remove the existing command
                existingCommand.Delete();

                // Create it again
                CreateCommand(commandShortName, commandCaption, commandTooltip, commandImage, bindings);
            }
        }

        private void CreateCommands()
        {
            OutputWindowPaneLog("AutoComment: CreateCommands:");
            foreach(CommandInfo ci in _commandInfos)
            {
                CreateCommand(ci._commandName, ci._commandCaption, ci._commandTooltip, ci._commandIcon, null);
            }
        }

        private void CreateCommand(string commandShortName, string commandCaption, string commandTooltip, int commandImage, object bindings)
        {
            EnvDTE.Command command = null;
            object[] contextUIGuids = new object[] { };

            try
            {
                OutputWindowPaneLog("AutoComment: CreateCommand: Name=" + commandShortName);
                command = _applicationObject.Commands.AddNamedCommand(_addInInstance, commandShortName, commandCaption, commandTooltip, true,
                    commandImage, ref contextUIGuids, (int)vsCommandStatus.vsCommandStatusSupported);

                 if (bindings != null)
                 {
                    command.Bindings = bindings;
                 }
            }
            catch(Exception ex)
            {
                // This should not happen!
                OutputWindowPaneLog("AutoComment: CreateCommand: Exception! " + ex.ToString());
            }
        }

        private void InitializeAddIn()
        {
            OutputWindowPaneLog("AutoComment: InitializeAddIn:");
            _toolsSubmenu = CreateToolsSubmenu();
            if(_toolsSubmenu != null)
            {
                foreach(CommandInfo ci in _commandInfos)
                {
                    AddCommandBarButtonToSubmenu(_toolsSubmenu, ci._commandName);
                }
            }
        }

        private void AddCommandBarButtonToSubmenu(CommandBarPopup submenu, string commandShortName)
        {
            OutputWindowPaneLog("AutoComment: AddCommandBarButtonToSubmenu: command=" + commandShortName);
            Command command = null;
            CommandBarButton commandBarButton = null;

            // Retrieve the command created in the ext_cm_UISetup phase of the OnConnection method
            command = _applicationObject.Commands.Item(_addInInstance.ProgID + "." + commandShortName, -1);

            // Add a control to the submenu
            commandBarButton = (CommandBarButton)command.AddControl(submenu.CommandBar, submenu.Controls.Count + 1);

            // Cast it to CommandBarButton to set some properties
            commandBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption;
            commandBarButton.Visible = true;
        }

        private CommandBarPopup CreateToolsSubmenu()
        {
            OutputWindowPaneLog("AutoComment: CreateToolsSubmenu:");

            CommandBarPopup toolsSubmenu = null;
			object []contextGUIDS = new object[] { };
			Commands2 commands = (Commands2)_applicationObject.Commands;
			string toolsMenuName = "Tools";

			//Place the command on the tools menu.
			//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
			Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

			//Find the Tools command bar on the MenuBar command bar:
			CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
			CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

			//This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
			//  just make sure you also update the QueryStatus/Exec method to include the new command names.
			try
			{
                // New submenu under the "Tools" menu
                toolsSubmenu = (CommandBarPopup)toolsPopup.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, toolsPopup.Controls.Count + 1, true);
                toolsSubmenu.BeginGroup = true;
                toolsSubmenu.CommandBar.Name = "AutoComment";
                toolsSubmenu.Caption = "AutoComment";
                toolsSubmenu.Visible = true;
            }
			catch(System.ArgumentException sysArgExc)
			{
				//If we are here, then the exception is probably because a command with that name
				//  already exists. If so there is no need to recreate the command and we can 
                //  safely ignore the exception.
                OutputWindowPaneLog("AutoComment: CreateToolsSubmenu: Exception! " + sysArgExc.ToString());
			}

            return toolsSubmenu;
        }

        private Command AddCommandToToolsSubmenu(Commands2 kCommands, AddIn kAddInInstance, CommandBarPopup kToolsPopup, CommandBarPopup kToolsSubmenu, ref object[] contextGUIDS, string strCommandName, string strButtonText, string strTooltip)
        {
            Command command = null;

            if(kCommands != null && kAddInInstance != null && kToolsPopup != null)
            {
                command = kCommands.AddNamedCommand2(kAddInInstance, strCommandName, strButtonText, strTooltip, true, System.Type.Missing /*59 smiley icon*/, contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                if(command != null)
                {
                    OutputWindowPaneLog("AutoComment: AddCommandToToolsSubmenu: Command=" + strCommandName);
                    CommandBarButton toolsSubmenuButton = (CommandBarButton)command.AddControl(kToolsSubmenu.CommandBar, kToolsSubmenu.Controls.Count + 1);
                    toolsSubmenuButton.Caption = strButtonText;
                }
            }

            return command;
        }
		/// <summary>Inserts a C style comment at the beginning of the current line.</summary>
        public void InsertCComment()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                string strCommentString;
                int iCommentLength;
                strCommentString = "/*  -tsmith */";
                iCommentLength = strCommentString.Length;
                objTextDoc.Selection.Text = strCommentString;
                while(iCommentLength > 3)
                {
                    objTextDoc.Selection.CharLeft();
                    iCommentLength--;
                }
            }
        }

		/// <summary>Inserts a C style comment with a date and time stamp at the beginning of the current line.</summary>
        public void InsertCCommentWithDateStamp()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                DateTime kTimeNow = DateTime.Now;
                string strCommentString = "/*  -tsmith " + kTimeNow.Month + "." + kTimeNow.Day + "." + kTimeNow.Year + " */";
                int iCommentLength = strCommentString.Length;
                
                objTextDoc.Selection.Text = strCommentString;
                while(iCommentLength > 3)
                {
                    objTextDoc.Selection.CharLeft();
                    iCommentLength--;
                }

            }
        }

		/// <summary>Inserts a C++ style comment at the beginning of the current line.</summary>
        public void InsertCppComment()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                string strCommentString;
                int iCommentLength;
                strCommentString = "//  -tsmith";
                iCommentLength = strCommentString.Length;
                objTextDoc.Selection.Text = strCommentString;
                while(iCommentLength > 3)
                {
                    objTextDoc.Selection.CharLeft();
                    iCommentLength--;
                }

            }
        }

		/// <summary>Inserts a C++ style comment with a date and time stamp at the beginning of the current line.</summary>
        public void InsertCppCommentWithDateStamp()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                DateTime kTimeNow = DateTime.Now;
                string strCommentString = "//  -tsmith " + kTimeNow.Month + "." + kTimeNow.Day + "." + kTimeNow.Year;
                int iCommentLength = strCommentString.Length;
                
                objTextDoc.Selection.Text = strCommentString;
                while(iCommentLength > 3)
                {
                    objTextDoc.Selection.CharLeft();
                    iCommentLength--;
                }

            }
        }

		/// <summary>Inserts the standard Firaxis header at the top of the current document.</summary>
        public void InsertFiraxisHeader()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                DateTime kTimeNow = DateTime.Now;
                string strFileName = _applicationObject.ActiveDocument.Name;

                // Set selection to top of document
                objTextDoc.Selection.StartOfDocument();
                objTextDoc.Selection.NewLine();

                objTextDoc.Selection.LineUp();
                objTextDoc.Selection.Text = "//---------------------------------------------------------------------------------------";
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//  *********   FIRAXIS SOURCE CODE   ******************";
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//  FILE:    " + strFileName;
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//  AUTHOR:  Todd Smith  --  " + kTimeNow.Month + "/" + kTimeNow.Day + "/" + kTimeNow.Year;
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//  PURPOSE: This file is used for the following stuff..blah";
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//---------------------------------------------------------------------------------------";
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//  Copyright (c) " + kTimeNow.Year + " Firaxis Games Inc. All rights reserved.";
                objTextDoc.Selection.NewLine();
                objTextDoc.Selection.Text = "//--------------------------------------------------------------------------------------- ";
                objTextDoc.Selection.NewLine();
            }
        }

		/// <summary>Inserts the Firaxis addition note.</summary>
        public void InsertFiraxisAddition()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                objTextDoc.Selection.Text = "// FIRAXIS addition -tsmith";
            }
        }

		/// <summary>Inserts the Firaxis begin note.</summary>
        public void InsertFiraxisBegin()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                objTextDoc.Selection.Text = "// FIRAXIS begin -tsmith";
            }
        }

		/// <summary>Inserts the Firaxis end note.</summary>
        public void InsertFiraxisEnd()
        {
            TextDocument objTextDoc = (TextDocument)_applicationObject.ActiveDocument.Object("TextDocument");
            if(objTextDoc != null)
            {
                objTextDoc.Selection.Text = "// FIRAXIS end -tsmith";
            }
        }

	}
}