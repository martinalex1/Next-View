﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
File name:     mainform.cs
Description:   main form to which other forms are docked as tabs
Copyright:     Copyright (c) Martin A. Schnell, 2012
Licence:       GNU General Public License
               This program is free software; you can redistribute it and/or
               modify it under the terms of the GNU General Public License
               as published by the Free Software Foundation.

               This program is free software: you can redistribute it and/or modify
               it under the terms of the GNU General Public License as published by
               the Free Software Foundation, either version 3 of the License, or
               (at your option) any later version.
History:

* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Diagnostics;  // Debug
using System.IO;   // path
using System.Windows.Forms;
using Next_View.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace Next_View
{
	/// <summary>
	/// MainForm, with 3 docked tabs
	/// </summary>
	public partial class frmMain : Form
	{
		private DeserializeDockContent _deserializeDockContent;
		public frmImage  m_Image; //  = new frmImage();

		public frmMain()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			this.recentsToolStripMenuItem1.UpdateList();
			this.recentsToolStripMenuItem1.MaxItems = 5;
			this.recentsToolStripMenuItem1.ItemClick += new System.EventHandler(recentItem_Click);
			this.recentsToolStripMenuItem1.UpdateList();

			_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
		}

		//--------------------------  form  ---------------------------//

		void FrmMainLoad(object sender, EventArgs e)
		{
			if (Properties.Settings.Default.UpgradeRequired)
			{
				Settings.Default.Upgrade();
				Settings.Default.UpgradeRequired = false;
				Settings.Default.Save( );
			}
			this.Width = Settings.Default.MainW;
			this.Height = Settings.Default.MainH;
			this.Left = Settings.Default.MainX;
			this.Top = Settings.Default.MainY;
		}

		void FrmMainShown(object sender, EventArgs e)
		{
			int fHeight = this.Height;
			int fWidth = this.Width;

			m_Image  = new frmImage(fWidth, fHeight);
			m_Image.StatusChanged += new HandleStatusMainChange(HandleStatus);
			m_Image.WindowChanged += new HandleWindowMainChange(HandleWindow);
			m_Image.WindowSize += new HandleWindowSize(HandleSize);

			m_Image.Show(dockPanel1, DockState.Document);      // sequence of tabs
			//m_Image.Show(dockPanel1, DockState.Document);     // set active

			string firstImage = "";
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1){
				firstImage = args[1];
			}
			if (File.Exists(firstImage)) {
				m_Image.PicScan(firstImage, false);
				m_Image.PicLoad(firstImage, true);
			}
			else if (File.Exists(Settings.Default.LastImage)) {
				m_Image.PicScan(Settings.Default.LastImage, false);
				m_Image.PicLoad(Settings.Default.LastImage, true);
			}
			else {
				string userImagePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Pictures";
				if (Directory.Exists(userImagePath)) {
					m_Image.PicScan(userImagePath, false);
				}
				firstImage = Directory.GetCurrentDirectory() + @"\Next-View-0.1.png";
				m_Image.PicLoad(firstImage, true);
			}
		}


		void FrmMainFormClosed(object sender, FormClosedEventArgs e)
		{
		// DockContent has no close event when main form closes
			Settings.Default.MainX = this.Left;
			Settings.Default.MainY = this.Top;
			Settings.Default.MainW = this.Width;
			Settings.Default.MainH = this.Height;
			Settings.Default.Save( );
			Debug.WriteLine("main FormClosed");
		}


		//--------------------------  menu  ---------------------------//
		//--------------------------  menu file ---------------------------//


		void MnuOpenImageClick(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			string lastPath = Settings.Default.LastImage;
			if (File.Exists(lastPath)){
				if (Directory.Exists(Path.GetDirectoryName(lastPath))) {
				  dialog.InitialDirectory = Path.GetDirectoryName(lastPath);
				}
			}
			dialog.Filter = "All images |*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.ico;*.tif;*.wmf;*.emf|JPEG files |*.jpg;*.jpeg|PNG files |*.png|GIF files |*.gif|Bitmap files |*.bmp|Icon files |*.ico|TIF files |*.tif|WMF files |*.wmf|EMF files |*.emf";
			dialog.Title = "Select image";

			if(dialog.ShowDialog() == DialogResult.OK)
			{
				string picPath = dialog.FileName;
				recentsToolStripMenuItem1.AddRecentItem(picPath);
				m_Image.PicScan(picPath, false);
				m_Image.PicLoad(picPath, true);
				Settings.Default.LastImage = picPath;
				Settings.Default.Save( );
			}
		}

		void MnuStartEditorClick(object sender, EventArgs e)
		{
			m_Image.StartEditor();
		}

		private void recentItem_Click(object sender, EventArgs e)
		{
			string picPath = sender.ToString();
			if (File.Exists(picPath))
			{
				recentsToolStripMenuItem1.AddRecentItem(picPath);
				m_Image.PicScan(picPath, false);
				m_Image.PicLoad(picPath, true);
				Settings.Default.LastImage = picPath;
				Settings.Default.Save( );
			}
			else
				MessageBox.Show (sender.ToString(), "File does not exist",
				      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		void MnuRenameClick(object sender, EventArgs e)
		{
			m_Image.RenamePic();
		}

		void MnuDeleteClick(object sender, EventArgs e)
		{
			m_Image.DelPic();
		}

		void MnuExitClick(object sender, EventArgs e)
		{
			this.Close();
			Debug.WriteLine("Exit:");
			Application.Exit();      // exit self
			Environment.Exit(0);     // kill by win
		}

		//--------------------------  menu edit ---------------------------//

		void MnuOptionsClick(object sender, EventArgs e)
		{
			frmOption frm = new frmOption();
			frm.ShowDialog();
		}

		//--------------------------  menu view ---------------------------//

		void MnuNextImageClick(object sender, EventArgs e)
		{
			m_Image.NextPic();
		}

		void MnuPriorImageClick(object sender, EventArgs e)
		{
			m_Image.PriorPic();
		}

		void MnuFirstImageClick(object sender, EventArgs e)
		{
			m_Image.FirstPic();
		}

		void MnuLastImageClick(object sender, EventArgs e)
		{
			m_Image.LastPic();
		}


		void MnuBackClick(object sender, EventArgs e)
		{
			m_Image.BackPic();
		}

		void MnuForwardClick(object sender, EventArgs e)
		{
			m_Image.ForwardPic();
		}

		void MnuRefreshClick(object sender, EventArgs e)
		{
			m_Image.RefreshDir();
		}

		void MnuFullScreenClick(object sender, EventArgs e)
		{
			m_Image.ShowFullScreen();
		}

		void MnuShowPanelClick(object sender, EventArgs e)
		{
			m_Image.Show(dockPanel1, DockState.Document);
		}



		//--------------------------  menu help ---------------------------//
		void MnuAboutClick(object sender, EventArgs e)
		{
			frmAbout frm = new frmAbout();
			frm.ShowDialog();
		}

		void MnuWebClick(object sender, EventArgs e)
		{
			//?
		}

		void MnuHelp1Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, "Next-View.chm");
		}

		void FrmMainHelpRequested(object sender, HelpEventArgs hlpevent)   // F1
		{
			Help.ShowHelp(this, "Next-View.chm", "Main.htm");
		}

		//--------------------------  methods  ------------------------------------//


		private IDockContent GetContentFromPersistString(string persistString)
		{
			if (persistString == typeof(frmImage).ToString())
				return m_Image;
			else
				return null;
		}


		//--------------------------  buttons  ---------------------------//


		void BnOpenClick(object sender, EventArgs e)
		{
			this.mnuOpenImage.PerformClick();
		}

		void BnStartEditorClick(object sender, EventArgs e)
		{
			this.mnuStartEditor.PerformClick();
		}

		void BnDeleteClick(object sender, EventArgs e)
		{
			this.mnuDelete.PerformClick();
		}


		void BnPriorClick(object sender, EventArgs e)
		{
			this.mnuPriorImage.PerformClick();
		}

		void BnNextClick(object sender, EventArgs e)
		{
			this.mnuNextImage.PerformClick();
		}

		void BnFullscreenClick(object sender, EventArgs e)
		{
			this.mnuFullScreen.PerformClick();
		}


		void BnHelpClick(object sender, EventArgs e)
		{
			this.mnuHelp1.PerformClick();
		}


		//--------------------------  test  ---------------------------//

		void MnuTestClick(object sender, EventArgs e)
		{
			int i = 4;
			int b = 100/(i-4);

		}

		void TestScreen()
		{
			foreach (var screen in System.Windows.Forms.Screen.AllScreens)
			{
				Debug.WriteLine("Device Name: " + screen.DeviceName);
				Debug.WriteLine("Bounds: " + screen.Bounds.ToString());
				Debug.WriteLine("Type: " + screen.GetType().ToString());
				Debug.WriteLine("Working Area: " + screen.WorkingArea.ToString());
				Debug.WriteLine("Primary : " + screen.Primary.ToString());

			}
		}


		//--------------------------  events  ------------------------------------//


		private void HandleStatus(object sender, SetStatusMainEventArgs e)
		// called by: SetStatusText
		{
			string par1 = e.NewValue;
			this.statusLabel1.Text = par1;
		}

		private void HandleWindow(object sender, SetStatusMainEventArgs e)
		// called by: SetWindowText
		{
			string pPath = e.NewValue;
			this.Text = pPath + "  -  Next-View";
			// this.Text = Path.GetFileName(pPath)	+	"  -  Next-View";
			recentsToolStripMenuItem1.AddRecentItem(pPath);
		}


		private void HandleSize(object sender, SetSizeEventArgs e)
		// called by: SetWindowSize
		{
			int w = e.nWidth;
			int h = e.nHeight;
			this.Width = w;
			this.Height = h;
			Debug.WriteLine("set size W / H: {0}/{1}", w, h);
		}


	}
	//--------------------------------------------------------------//


	public delegate void HandleStatusMainChange(object sender, SetStatusMainEventArgs e);

	public delegate void HandleWindowMainChange(object sender, SetStatusMainEventArgs e);

	public delegate void HandleWindowSize(object sender, SetSizeEventArgs e);

}