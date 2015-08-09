using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;


class ListItem
{
	public readonly int CodePage;
	public readonly string Text;

	public ListItem(int codePage, string text)
	{
		CodePage = codePage;
		Text = text;
	}

	public override string ToString()
	{
		return Text;
	}
}


class ReCode : Form
{
	ListBox list = new ListBox();
	ComboBox oldCode = new ComboBox();
	ComboBox newCode = new ComboBox();
	Button start = new Button();
	Button info = new Button();
	ToolTip toolTip = new ToolTip();

	ReCode()
	{
		Text = "ReCode";
		TopMost = true;
		ClientSize = new Size(320, 240);
		list.IntegralHeight = false;
		list.AllowDrop = true;
		list.SelectionMode = SelectionMode.MultiExtended;
		oldCode.DropDownStyle = newCode.DropDownStyle = ComboBoxStyle.DropDownList;
		ListItem[] listItems = new ListItem[] { new ListItem(1251, "Windows"), new ListItem(866, "Dos"), new ListItem(20866, "Unix") };
		oldCode.Items.AddRange(listItems);
		newCode.Items.AddRange(listItems);
		oldCode.SelectedIndex = newCode.SelectedIndex = 0;
		start.Text = "Старт";
		info.Text = "Справка...";
		start.Height = info.Height = oldCode.Height;
		list.Parent = this;
		oldCode.Parent = this;
		newCode.Parent = this;
		start.Parent = this;
		info.Parent = this;
		toolTip.SetToolTip(list, "Список файлов");
		toolTip.SetToolTip(oldCode, "Исходная кодировка");
		toolTip.SetToolTip(newCode, "Требуемая кодировка");
		toolTip.SetToolTip(start, "Изменить кодировку");
		toolTip.SetToolTip(info, "Информация...");
		list.DragOver += new DragEventHandler(list_DragOver);
		list.DragDrop += new DragEventHandler(list_DragDrop);
		list.KeyDown += new KeyEventHandler(list_KeyDown);
		start.Click += new EventHandler(start_Click);
		info.Click += new EventHandler(info_Click);
	}

	protected override void OnResize(EventArgs e)
	{
		list.Height = ClientSize.Height - oldCode.Height;
		list.Width = ClientSize.Width;
		oldCode.Top = newCode.Top = start.Top = info.Top = ClientSize.Height - oldCode.Height;
		oldCode.Width = newCode.Width = start.Width = ClientSize.Width / 4;
		info.Width = ClientSize.Width - 3 * oldCode.Width;
		newCode.Left = oldCode.Right;
		start.Left = newCode.Right;
		info.Left = start.Right;
	}

	[STAThread]
	static void Main() 
	{
		Application.Run(new ReCode());
	}

	static bool IsFileDrop(IDataObject d)
	{
		foreach (string str in d.GetFormats())
		{
			if (str == DataFormats.FileDrop)
				return true;
		}
		return false;
	}

	void list_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = IsFileDrop(e.Data) ? DragDropEffects.All : DragDropEffects.None;
	}

	void info_Click(object sender, EventArgs e)
	{
		MessageBox.Show
		(
			"Утилита для изменения кодировки текста\n" +
			"Порядок действий:\n" +
			"1) Перетащите необходимые файлы (папки) в список\n" +
			"2) Выберите исходную кодировку\n" +
			"3) Выберите требуемую кодировку\n" +
			"4) Нажмите кнопку \"Старт\"\n\n" +
			"Примечание:\n" +
			"Клавишей Delete можно удалить лишние файлы из списка",
			"ReCode"
		);
	}

	void ReCodeFile(string path)
	{
		try
		{
			int oldCodePage = ((ListItem)oldCode.SelectedItem).CodePage;
			int newCodePage = ((ListItem)newCode.SelectedItem).CodePage;
			File.Copy(path, path + ".bak", true);
			string text = File.ReadAllText(path, Encoding.GetEncoding(oldCodePage));
			File.WriteAllText(path, text, Encoding.GetEncoding(newCodePage));
		}
		catch
		{
			MessageBox.Show("Ошибка при перекодировании файла \"" + path + "\".");
		}
	}

	void start_Click(object sender, EventArgs e)
	{
		foreach (string path in list.Items)
			ReCodeFile(path);
		list.Items.Clear();
	}

	void AddDir(string dir)
	{
		list.Items.AddRange(Directory.GetFiles(dir));
		string[] subDirs = Directory.GetDirectories(dir);
		foreach (string subDir in subDirs)
			AddDir(subDir);
	}

	void list_DragDrop(object sender, DragEventArgs e)
	{
		string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
		foreach (string path in paths)
		{
			if (File.Exists(path))
				list.Items.Add(path);
			else if(Directory.Exists(path))
				AddDir(path);
		}
	}

	void list_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Delete)
		{
			object[] selectedItems = new object[list.SelectedItems.Count];
			list.SelectedItems.CopyTo(selectedItems, 0);
			foreach (object obj in selectedItems)
				list.Items.Remove(obj);
		}
	}
}
