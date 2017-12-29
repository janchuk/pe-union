﻿using BytecodeApi;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace PEunion
{
	public partial class TabRtlo : ObservableUserControl
	{
		private string _SourceFile;
		private string _DestinationFileName;
		private string _DestinationOldExtension;
		private string _DestinationNewExtension;
		private string _DestinationIcon;
		public string SourceFile
		{
			get => _SourceFile;
			set
			{
				_SourceFile = value;
				ctrlBrowseSourceFile.Text = Path.GetFileName(value);
				ctrlBrowseSourceFile.IconImageSource = new FileInfo(value).GetFileIcon(true).ToBitmapSource();
				DestinationFileName = Path.GetFileNameWithoutExtension(value);
				DestinationOldExtension = DestinationNewExtension = PathEx.GetExtension(value);
			}
		}
		public string DestinationFileName
		{
			get => _DestinationFileName;
			set
			{
				Set(() => DestinationFileName, ref _DestinationFileName, value);
				UpdatePreview();
			}
		}
		public string DestinationOldExtension
		{
			get => _DestinationOldExtension;
			set
			{
				Set(() => DestinationOldExtension, ref _DestinationOldExtension, value);
				UpdatePreview();
			}
		}
		public string DestinationNewExtension
		{
			get => _DestinationNewExtension;
			set
			{
				Set(() => DestinationNewExtension, ref _DestinationNewExtension, value);
				UpdatePreview();
			}
		}
		public string DestinationIcon
		{
			get => _DestinationIcon;
			set
			{
				Set(() => DestinationIcon, ref _DestinationIcon, value);
				ctrlDestinationIcon.IconImageSource = File.Exists(DestinationIcon) ? new FileInfo(DestinationIcon).GetFileIcon(true).ToBitmapSource() : null;
				UpdatePreview();
			}
		}

		public TabRtlo()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void ctrlBrowseSourceFile_FilesSelect(object sender, string[] e)
		{
			if (e.First().Contains("\u202e"))
			{
				MessageBoxes.Warning("This file already contains a RTLO character (U+202e).");
			}
			else
			{
				SourceFile = e.First();
			}
		}
		private void ctrlDestinationIcon_FilesSelect(object sender, string[] e)
		{
			DestinationIcon = e.First();
		}
		private void btnResetDestinationIcon_Click(object sender, RoutedEventArgs e)
		{
			DestinationIcon = null;
		}
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (SourceFile == null)
			{
				MessageBoxes.Warning("Select the source file.");
			}
			else if (!File.Exists(SourceFile))
			{
				MessageBoxes.Warning("'" + SourceFile + "' not found.");
			}
			else if (!DestinationOldExtension.CompareCaseInsensitive(PathEx.GetExtension(SourceFile)))
			{
				MessageBoxes.Warning("The old extension must match the source file extension '" + PathEx.GetExtension(SourceFile) + "'.");
			}
			else
			{
				string path = Dialogs.OpenFolder();
				if (path != null)
				{
					try
					{
						string fileName = Path.Combine(path, DestinationFileName + "\u202e" + DestinationNewExtension.Reverse() + "." + DestinationOldExtension);

						if (!File.Exists(fileName) || MessageBoxes.Confirmation("A files named '' already exists in the selected directory.\r\nOverwrite?", true))
						{
							if (DestinationIcon == null)
							{
								File.Copy(SourceFile, fileName, true);
							}
							else
							{
								string tempPath = Path.Combine(path, _DestinationFileName + "~.tmp");
								File.Copy(SourceFile, tempPath, true);
								new FileInfo(tempPath).ChangeExecutableIcon(DestinationIcon);
								File.Copy(tempPath, fileName, true);
								File.Delete(tempPath);
							}
						}
					}
					catch (Exception ex)
					{
						MessageBoxes.Error(ex.GetType() + ": " + ex.Message);
					}
				}
			}
		}

		private void UpdatePreview()
		{
			if (File.Exists(DestinationIcon)) ctrlPreview.ImageSource = new FileInfo(DestinationIcon).GetFileIcon(false).ToBitmapSource();
			else ctrlPreview.ImageSource = File.Exists(SourceFile) ? new FileInfo(SourceFile).GetFileIcon(false).ToBitmapSource() : null;

			ctrlPreview.Text = DestinationFileName + DestinationOldExtension?.Reverse() + "." + DestinationNewExtension;
		}
	}
}