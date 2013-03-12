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

namespace AutoComment
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
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
			if(connectMode == ext_ConnectMode.ext_cm_UISetup)
			{
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
                    CommandBarPopup toolsSubmenu = (CommandBarPopup)toolsPopup.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, toolsPopup.Controls.Count + 1, true);
                    toolsSubmenu.BeginGroup = true;
                    toolsSubmenu.CommandBar.Name = "AutoComment";
                    toolsSubmenu.Caption = "AutoComment";
                    toolsSubmenu.Visible = true;

                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertCComment", "InsertCComment", "Inserts a one line C style comment.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertCCommentWithDateStamp", "InsertCCommentWithDateStamp", "Inserts a one line C style comment with the current date.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertCppComment", "InsertCppComment", "Inserts a one line C++ style comment.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertCppCommentWithDateStamp", "InsertCppCommentWithDateStamp", "Inserts a one line C++ style comment with the current date.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertFiraxisHeader", "InsertFiraxisHeader", "Inserts the standard Firaxis header at the top of the file.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertFiraxisAddition", "InsertFiraxisAddition", "Inserts the Firaxis addition note.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertFiraxisBegin", "InsertFiraxisBegin", "Inserts the Firaxis begin note.");
                    AddCommandToToolsSubmenu(commands, _addInInstance, toolsPopup, toolsSubmenu, ref contextGUIDS, "InsertFiraxisEnd", "InsertFiraxisEnd", "Inserts the Firaxis end note.");
				}
				catch(System.ArgumentException)
				{
					//If we are here, then the exception is probably because a command with that name
					//  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
				}
			}
		}

        private Command AddCommandToToolsSubmenu(Commands2 kCommands, AddIn kAddInInstance, CommandBarPopup kToolsPopup, CommandBarPopup kToolsSubmenu, ref object[] contextGUIDS, string strCommandName, string strButtonText, string strTooltip)
        {
            Command command = null;

            if(kCommands != null && kAddInInstance != null && kToolsPopup != null)
            {
                command = kCommands.AddNamedCommand2(kAddInInstance, strCommandName, strButtonText, strTooltip, true, System.Type.Missing /*59 smiley icon*/, contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                if(command != null)
                {
                    CommandBarButton toolsSubmenuButton = (CommandBarButton)command.AddControl(kToolsSubmenu.CommandBar, kToolsSubmenu.Controls.Count + 1);
                    toolsSubmenuButton.Caption = strButtonText;
                }
            }

            return command;
        }

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
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

		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}