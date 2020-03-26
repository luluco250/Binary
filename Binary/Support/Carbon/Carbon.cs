﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlobalLib.Core;
using GlobalLib.Reflection.Abstract;



namespace Binary.Support
{
	public partial class Carbon : Form
	{
		private GlobalLib.Database.Carbon dbC;
		private FastColoredTextBoxNS.FastColoredTextBox ColoredTextForm;

		private void InstantiateControls()
		{
			ColoredTextForm = new FastColoredTextBoxNS.FastColoredTextBox();
			ColoredTextForm.Width = this.EndscriptEditor.Width;
			ColoredTextForm.Height = this.EndscriptEditor.Height;
			ColoredTextForm.BackColor = this.EndscriptEditor.BackColor;
			ColoredTextForm.CurrentLineColor = Color.FromArgb(70, 70, 100);
			ColoredTextForm.ForeColor = this.EndscriptEditor.ForeColor;
			ColoredTextForm.LineNumberColor = Color.FromArgb(220, 220, 220);
			ColoredTextForm.BookmarkColor = this.EndscriptEditor.BackColor;
			ColoredTextForm.IndentBackColor = Color.FromArgb(50, 50, 70);
			ColoredTextForm.ServiceLinesColor = ColoredTextForm.IndentBackColor;
			ColoredTextForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			this.EndscriptEditor.Controls.Add(ColoredTextForm);
			this.DataSet_Split2.Panel2.Controls.Add(this.EndscriptEditor);
			this.DataSet_Split2.Panel1.Controls.Add(this.BinaryDataView);
			this.DataSet_Split1.Panel2.Controls.Add(this.DataSet_Split2);
			this.DataSet_Split1.Panel1.Controls.Add(this.BinaryTree);
			Process.MessageShow = true;
		}

		public Carbon()
		{
			InitializeComponent();
			InstantiateControls();
		}

		public Carbon(bool forceload)
		{
			InitializeComponent();
			InstantiateControls();
			DataSet_MenuStrip.Renderer = new MyRenderer();
			if (forceload)
				this.LoadDBCarbon(Process.GlobalDir, false);
		}

		private class MyRenderer : ToolStripProfessionalRenderer
		{
			public MyRenderer() : base(new MyColors()) { }
		}

		private class MyColors : ProfessionalColorTable
		{
			public override Color MenuItemSelected
			{
				get { return Color.FromArgb(40, 23, 60); }
			}
			public override Color MenuItemSelectedGradientBegin
			{
				get { return Color.FromArgb(40, 23, 60); }
			}
			public override Color MenuItemSelectedGradientEnd
			{
				get { return Color.FromArgb(40, 23, 60); }
			}
		}

		private void EnableButtons()
		{
			DataSet_ReloadFile.Enabled = true;
			DataSet_SaveFile.Enabled = true;
			DataSet_ImportFile.Enabled = true;
			DataSet_ProcessCommand.Enabled = true;
			DataSet_GenerateCommand.Enabled = true;
			DataSet_RestoreBackups.Enabled = true;
			DataSet_CreateBackups.Enabled = true;
			DataSet_UnlockFiles.Enabled = true;
			DataSet_RunGame.Enabled = true;
			DataSet_ExportAllTextures.Enabled = true;
			DataSet_DBInfo.Enabled = true;
			DataSet_BoundsList.Enabled = true;
			EndscriptToolStripMenuItemI.Enabled = true;
		}

		private void DisableButtons()
		{
			DataSet_ReloadFile.Enabled = false;
			DataSet_SaveFile.Enabled = false;
			DataSet_ImportFile.Enabled = false;
			DataSet_ProcessCommand.Enabled = false;
			DataSet_GenerateCommand.Enabled = false;
			DataSet_RestoreBackups.Enabled = false;
			DataSet_CreateBackups.Enabled = false;
			DataSet_UnlockFiles.Enabled = false;
			DataSet_RunGame.Enabled = false;
			DataSet_ExportAllTextures.Enabled = false;
			DataSet_DBInfo.Enabled = false;
			DataSet_BoundsList.Enabled = false;
			EndscriptToolStripMenuItemI.Enabled = false;
		}

		private void CreateBackupFiles(bool force)
		{
			try
			{
				string a1 = Process.GlobalDir + @"\GLOBAL\GlobalA.bun";
				string a2 = Process.GlobalDir + @"\GLOBAL\GlobalA.bun.bacc";
				string b1 = Process.GlobalDir + @"\GLOBAL\GlobalB.lzc";
				string b2 = Process.GlobalDir + @"\GLOBAL\GlobalB.lzc.bacc";
				string c1 = Process.GlobalDir + @"\LANGUAGES\English_Global.bin";
				string c2 = Process.GlobalDir + @"\LANGUAGES\English_Global.bin.bacc";
				string d1 = Process.GlobalDir + @"\LANGUAGES\Labels_Global.bin";
				string d2 = Process.GlobalDir + @"\LANGUAGES\Labels_Global.bin.bacc";
				if (!force)
				{
					if (!File.Exists(a2)) File.Copy(a1, a2);
					if (!File.Exists(b2)) File.Copy(b1, b2);
					if (!File.Exists(c2)) File.Copy(c1, c2);
					if (!File.Exists(d2)) File.Copy(d1, d2);
				}
				else
				{
					if (File.Exists(a2)) File.Delete(a2);
					if (File.Exists(b2)) File.Delete(b2);
					if (File.Exists(c2)) File.Delete(c2);
					if (File.Exists(d2)) File.Delete(d2);
					File.Copy(a1, a2); File.Copy(b1, b2);
					File.Copy(c1, c2); File.Copy(d1, d2);
				}
			}
			catch (Exception e)
			{
				while (e.InnerException != null) e = e.InnerException;
				MessageBox.Show($"Error occured: {e.Message}", "Failure");
			}
		}

		private void TerminateLoad()
		{
			this.dbC = null;
			DataSet_Status.Text = $"Failed to load data | {DateTime.Now.ToString()}";
			BinaryTree.Nodes.Clear();
			BinaryDataView.Columns.Clear();
			this.DisableButtons();
		}

		private void LoadDBCarbon(string foldername, bool showerror)
		{
			var GlobalA = File.Exists(foldername + @"\Global\GlobalA.bun");
			var GlobalB = File.Exists(foldername + @"\Global\GlobalB.lzc");
			var LangGen = File.Exists(foldername + @"\Languages\English_Global.bin");
			var LangLab = File.Exists(foldername + @"\Languages\Labels_Global.bin");
			var Stream = File.Exists(foldername + @"\Tracks\StreamL5RA.bun");
			var NFSC = File.Exists(foldername + @"\nfsc.exe");
			var Load = GlobalA && GlobalB && LangGen && LangLab && Stream && NFSC;
			if (!Load)
			{
				if (showerror)
					MessageBox.Show("Folder is not game's directory." + Environment.NewLine + "Please select the correct folder.", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			Process.GlobalDir = foldername;
			dbC = null;
			dbC = new GlobalLib.Database.Carbon();
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			bool valid = Process.LoadData(dbC);
			watch.Stop();
			if (!valid) { this.TerminateLoad(); return; }
			LoadBinaryTree();
			DataSet_Status.Text = $"Finished loading data in {watch.ElapsedMilliseconds}ms | {DateTime.Now.ToString()}";
			EnableButtons();
			if (Properties.Settings.Default.EnableAutobackup)
				this.CreateBackupFiles(false);
		}

		private TreeNode AppendTreeNode(string name, List<VirtualNode> subnodes)
		{
			var treenode = new TreeNode(name);
			foreach (var subnode in subnodes)
				treenode.Nodes.Add(this.AppendTreeNode(subnode.NodeName, subnode.SubNodes));
			return treenode;
		}

		private void LoadBinaryTree()
		{
			this.BinaryTree.Nodes.Clear();
			this.BinaryDataView.Columns.Clear();
			this.BinaryTree.Nodes.Add(this.AppendTreeNode(this.dbC.CarTypeInfos.ThisName, this.dbC.CarTypeInfos.GetAllNodes()));
			this.BinaryTree.Nodes.Add(this.AppendTreeNode(this.dbC.Materials.ThisName, this.dbC.Materials.GetAllNodes()));
			this.BinaryTree.Nodes.Add(this.AppendTreeNode(this.dbC.PresetRides.ThisName, this.dbC.PresetRides.GetAllNodes()));
			this.BinaryTree.Nodes.Add(this.AppendTreeNode(this.dbC.PresetSkins.ThisName, this.dbC.PresetSkins.GetAllNodes()));
		}

		private void BinaryDataViewColumnInit()
		{
			var column_descr = new DataGridViewTextBoxColumn();
			var column_value = new DataGridViewTextBoxColumn();

			column_descr.Name = "Attribute";
			column_descr.HeaderText = "Attribute";
			column_descr.ReadOnly = true;
			column_descr.Resizable = DataGridViewTriState.False;

			column_value.Name = "Value";
			column_value.HeaderText = "Value";
			column_value.Resizable = DataGridViewTriState.False;

			column_descr.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			column_value.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			column_descr.SortMode = DataGridViewColumnSortMode.NotSortable;
			column_value.SortMode = DataGridViewColumnSortMode.NotSortable;
			BinaryDataView.MultiSelect = false;

			BinaryDataView.Columns.Add(column_descr);
			BinaryDataView.Columns.Add(column_value);
			BinaryDataView.RowHeadersWidth = 30;
		}

		private void DataSet_OpenFile_Click(object sender, EventArgs e)
		{
			this.BrowseGameDirDialog.Description = "Select the NFS: Carbon directory that you want to work on.";

			if (BrowseGameDirDialog.ShowDialog() == DialogResult.OK)
			{
				this.LoadDBCarbon(BrowseGameDirDialog.SelectedPath, true);
			}
		}

		private void DataSet_SaveFile_Click(object sender, EventArgs e)
		{
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			Process.SaveData(dbC, Properties.Settings.Default.EnableCompression);
			watch.Stop();
			DataSet_Status.Text = $"Finished saving data in {watch.ElapsedMilliseconds}ms | {DateTime.Now.ToString()}";
		}

		private void BinaryTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BinaryDataView.Columns.Clear();

			if (BinaryTree.SelectedNode == null || BinaryTree.SelectedNode.Parent == null) return;
			var obj = dbC.GetPrimitive(Utils.Path.SplitPath(BinaryTree.SelectedNode.FullPath));
			if (obj == null) return;
			var list = obj.GetAccessibles(GlobalLib.Database.Collection.eGetInfoType.PROPERTY_NAMES);

			this.BinaryDataViewColumnInit();

			var accessibles = new List<string>(list.Length);
			for (int a1 = 0; a1 < list.Length; ++a1)
				accessibles.Add(list[a1].ToString());
			accessibles.Sort();

			foreach (var access in accessibles)
			{
				string field = access.ToString();
				if (obj.OfEnumerableType(field))
				{
					var attribcell = new DataGridViewTextBoxCell();
					var valuecell = new DataGridViewComboBoxCell();
					attribcell.Value = field;
					valuecell.Items.AddRange(obj.GetPropertyEnumerableTypes(field));
					valuecell.Value = obj.GetValue(field);
					valuecell.FlatStyle = FlatStyle.Flat;
					valuecell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
					valuecell.Style.BackColor = Color.FromArgb(30, 30, 45);
					var row = new DataGridViewRow();
					row.Cells.AddRange(attribcell, valuecell);
					this.BinaryDataView.Rows.Add(row);
				}
				else
				{
					var attribcell = new DataGridViewTextBoxCell();
					var valuecell = new DataGridViewTextBoxCell();
					attribcell.Value = field;
					valuecell.Value = obj.GetValue(field);
					var row = new DataGridViewRow();
					row.Cells.AddRange(attribcell, valuecell);
					this.BinaryDataView.Rows.Add(row);
				}
			}
		}

		private void DataSet_ReloadFile_Click(object sender, EventArgs e)
		{
			this.LoadDBCarbon(Process.GlobalDir, true);
		}

		private void DataSet_Hasher_Click(object sender, EventArgs e)
		{
			var HasherWindow = new Tools.Hasher();
			HasherWindow.Show();
		}

		private void DataSet_Raider_Click(object sender, EventArgs e)
		{
			var RaiderWindow = new Tools.Raider();
			RaiderWindow.Show();
		}

		private void DataSet_Color_Click(object sender, EventArgs e)
		{
			var ColorWindow = new Tools.ColorPicker();
			ColorWindow.Show();
		}

		private void DataSet_Swatch_Click(object sender, EventArgs e)
		{
			var SwatchWindow = new Tools.SwatchPicker();
			SwatchWindow.Show();
		}

		private void DataSet_Exit_Click(object sender, EventArgs e)
		{
			this.Carbon_FormClosing(this, null);
			this.Close();
		}

		private void Carbon_FormClosing(object sender, FormClosingEventArgs e)
		{
			var list = Application.OpenForms.Cast<Form>().ToList();
			for (int a1 = list.Count - 1; a1 >= 0; --a1)
			{
				if (list[a1].Name != "Main" && list[a1].Name != this.Name)
					list[a1].Close();
			}
		}

		private void Carbon_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.dbC = null;
			GC.Collect(0, GCCollectionMode.Forced);
			GC.Collect(1, GCCollectionMode.Forced);
			GC.Collect(2, GCCollectionMode.Forced);
		}
	}
}
