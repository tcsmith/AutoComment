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
					//Add a command to the Commands collection:
					Command command = commands.AddNamedCommand2(_addInInstance, "AutoComment", "AutoComment", "Executes the command for AutoComment", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}

					//Add a command to the Commands collection:
					command = commands.AddNamedCommand2(_addInInstance, "InsertCppComment", "InsertCppComment", "Inserts a one line C++ style comment", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}

					//Add a command to the Commands collection:
					command = commands.AddNamedCommand2(_addInInstance, "InsertCppCommentWithDateStamp", "InsertCppCommentWithDateStamp", "Inserts a one line C++ style comment with the current date.", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}
					//Add a command to the Commands collection:
					command = commands.AddNamedCommand2(_addInInstance, "InsertFiraxisHeader", "InsertFiraxisHeader", "Inserts the standard Firaxis header at the top of the file.", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}
				}
				catch(System.ArgumentException)
				{
					//If we are here, then the exception is probably because a command with that name
					//  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
				}
			}
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
				if(commandName == "AutoComment.Connect.AutoComment")
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
				if(commandName == "AutoComment.Connect.AutoComment")
				{
					handled = true;
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

		/// <summary>Inserts the stand Firaxis header at the top of the current document.</summary>
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

		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}