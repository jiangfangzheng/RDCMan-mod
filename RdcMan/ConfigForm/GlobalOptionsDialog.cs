using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public class GlobalOptionsDialog : TabbedSettingsDialog
	{
		private class BandwidthItem
		{
			public string Text;

			public int Flags;

			public BandwidthItem(string text, int flags)
			{
				Text = text;
				Flags = flags;
			}
		}

		private GroupBox _virtualGroupsGroup;

		private ValueComboBox<ControlVisibility> _treeVisibilityCombo;

		private ValueComboBox<DockStyle> _treeLocationCombo;

		private CheckBox _connectionBarEnabledCheckBox;

		private CheckBox _connectionBarAutoHiddenCheckBox;

		private RdcCheckBox _enablePanningCheckBox;

		private RdcNumericUpDown _panningAccelerationUpDown;

		private GroupBox _casSizeGroup;

		private RadioButton _casCustomRadio;

		private RadioButton _thumbnailPixelsRadio;

		private RadioButton _thumbnailPercentageRadio;

		private TextBox _thumbnailPercentageTextBox;

		private Button _casCustomButton;

		private Button _thumbnailPixelsButton;

		private ValueComboBox<BandwidthItem> _bandwidthComboBox;

		private CheckBox _desktopBackgroundCheckBox;

		private CheckBox _fontSmoothingCheckBox;

		private CheckBox _desktopCompositionCheckBox;

		private CheckBox _windowDragCheckBox;

		private CheckBox _menuAnimationCheckBox;

		private CheckBox _themesCheckBox;

		private bool _inHandler;

		private readonly BandwidthItem[] _bandwidthItems;

		protected GlobalOptionsDialog(Form parentForm)
			: base("ѡ��", "OK", parentForm)
		{
			_bandwidthItems = new BandwidthItem[5]
			{
				new BandwidthItem("Modem (28.8 Kbps)", 15),
				new BandwidthItem("Modem (56 Kbps)", 7),
				new BandwidthItem("Broadband (128 Kpbs - 1.5 Mbps)", 257),
				new BandwidthItem("LAN (10 Mbps or higher)", 384),
				new BandwidthItem("�Զ���", 0)
			};
			InitializeComponent();
		}

		public static GlobalOptionsDialog New()
		{
			GlobalOptionsDialog globalOptionsDialog = new GlobalOptionsDialog(Program.TheForm);
			globalOptionsDialog.InitializeControlsFromPreferences();
			return globalOptionsDialog;
		}

		private void InitializeControlsFromPreferences()
		{
			MainForm theForm = Program.TheForm;
			foreach (CheckBox control in _virtualGroupsGroup.Controls)
			{
				control.Checked = (control.Tag as IBuiltInVirtualGroup).IsInTree;
			}
			_treeLocationCombo.SelectedValue = Program.TheForm.ServerTreeLocation;
			_treeVisibilityCombo.SelectedValue = Program.TheForm.ServerTreeVisibility;
			_connectionBarEnabledCheckBox.Checked = (Program.Preferences.ConnectionBarState != RdpClient.ConnectionBarState.Off);
			_connectionBarAutoHiddenCheckBox.Checked = (Program.Preferences.ConnectionBarState == RdpClient.ConnectionBarState.AutoHide);
			_connectionBarAutoHiddenCheckBox.Enabled = _connectionBarEnabledCheckBox.Enabled;
			if (RdpClient.SupportsPanning)
			{
				_panningAccelerationUpDown.Enabled = Program.Preferences.EnablePanning;
			}
			Size clientSize = theForm.GetClientSize();
			RadioButton radioButton = (from r in _casSizeGroup.Controls.OfType<RadioButton>()
				where (Size?)r.Tag == clientSize
				select r).FirstOrDefault();
			if (radioButton != null)
			{
				radioButton.Checked = true;
			}
			else
			{
				_casCustomRadio.Checked = true;
			}
			_casCustomButton.Text = clientSize.ToFormattedString();
			_thumbnailPixelsButton.Text = Program.Preferences.ThumbnailSize.ToFormattedString();
			_thumbnailPercentageTextBox.Text = Program.Preferences.ThumbnailPercentage.ToString();
			if (Program.Preferences.ThumbnailSizeIsInPixels)
			{
				_thumbnailPixelsRadio.Checked = true;
			}
			else
			{
				_thumbnailPercentageRadio.Checked = true;
			}
			SetBandwidthCheckBoxes(Program.Preferences.PerformanceFlags);
		}

		public void UpdatePreferences()
		{
			UpdateSettings();
			MainForm theForm = Program.TheForm;
			foreach (CheckBox control in _virtualGroupsGroup.Controls)
			{
				(control.Tag as IBuiltInVirtualGroup).IsInTree = control.Checked;
			}
			Program.TheForm.ServerTreeLocation = _treeLocationCombo.SelectedValue;
			Program.TheForm.ServerTreeVisibility = _treeVisibilityCombo.SelectedValue;
			RdpClient.ConnectionBarState connectionBarState = _connectionBarEnabledCheckBox.Checked ? ((!_connectionBarAutoHiddenCheckBox.Checked) ? RdpClient.ConnectionBarState.Pinned : RdpClient.ConnectionBarState.AutoHide) : RdpClient.ConnectionBarState.Off;
			Program.Preferences.ConnectionBarState = connectionBarState;
			Program.Preferences.PerformanceFlags = ComputeFlagsFromCheckBoxes();
			string text = _casCustomButton.Text;
			if (!_casCustomRadio.Checked)
			{
				text = (from r in _casSizeGroup.Controls.OfType<RadioButton>()
					where r.Checked
					select r).First().Text;
			}
			Size size = SizeHelper.Parse(text);
			Size clientSize = theForm.GetClientSize();
			if (clientSize != size)
			{
				theForm.SetClientSize(size);
			}
			size = SizeHelper.Parse(_thumbnailPixelsButton.Text);
			Program.Preferences.ThumbnailSize = size;
			Program.Preferences.ThumbnailSizeIsInPixels = _thumbnailPixelsRadio.Checked;
			int thumbnailPercentage = int.Parse(_thumbnailPercentageTextBox.Text);
			Program.Preferences.ThumbnailPercentage = thumbnailPercentage;
		}

		private void InitializeComponent()
		{
			CreateGeneralPage();
			CreateServerTreePage();
			CreateClientAreaPage();
			CreateHotKeysPage();
			CreateExperiencePage();
			CreateFullScreenPage();
			InitButtons();
			this.ScaleAndLayout();
		}

		private TabPage NewTabPage(string name)
		{
			SettingsTabPage settingsTabPage = new SettingsTabPage();
			settingsTabPage.Location = FormTools.TopLeftLocation();
			settingsTabPage.Size = new Size(512, 334);
			settingsTabPage.Text = name;
			TabPage tabPage = settingsTabPage;
			AddTabPage(tabPage);
			return tabPage;
		}

		private TabPage CreateFullScreenPage()
		{
			int rowIndex = 0;
			int tabIndex = 0;
			TabPage tabPage = NewTabPage("ȫ��");
			_connectionBarEnabledCheckBox = FormTools.NewCheckBox("��ʾȫ��������", 0, rowIndex++, tabIndex++);
			_connectionBarEnabledCheckBox.CheckedChanged += ConnectionBarEnabledCheckedChanged;
			_connectionBarAutoHiddenCheckBox = FormTools.NewCheckBox("�Զ����ع�����", 0, rowIndex++, tabIndex++);
			_connectionBarAutoHiddenCheckBox.Location = new Point(_connectionBarEnabledCheckBox.Left + 24, _connectionBarAutoHiddenCheckBox.Top);
			FormTools.AddCheckBox(tabPage, "ȫ������ʼ��λ�ڶ���", Program.Preferences.Settings.FullScreenWindowIsTopMost, 0, ref rowIndex, ref tabIndex);
			if (RdpClient.SupportsMonitorSpanning)
			{
				FormTools.AddCheckBox(tabPage, "��Ҫʱʹ�ö����ʾ��", Program.Preferences.Settings.UseMultipleMonitors, 0, ref rowIndex, ref tabIndex);
			}
			if (RdpClient.SupportsPanning)
			{
				_enablePanningCheckBox = FormTools.NewCheckBox("ʹ��ƽ�ƴ��������", 0, rowIndex++, tabIndex++);
				_enablePanningCheckBox.Setting = Program.Preferences.Settings.EnablePanning;
				_enablePanningCheckBox.CheckedChanged += EnablePanningCheckedChanged;
				Label label = FormTools.NewLabel("ƽ���ٶ�", 0, rowIndex);
				label.Size = new Size(116, 20);
				label.Location = new Point(_enablePanningCheckBox.Left + 24, label.Top);
				_panningAccelerationUpDown = new RdcNumericUpDown();
				_panningAccelerationUpDown.Location = FormTools.NewLocation(1, rowIndex++);
				_panningAccelerationUpDown.Minimum = 1m;
				_panningAccelerationUpDown.Maximum = 9m;
				_panningAccelerationUpDown.Size = new Size(40, 20);
				_panningAccelerationUpDown.TabIndex = tabIndex++;
				_panningAccelerationUpDown.Setting = Program.Preferences.Settings.PanningAcceleration;
				tabPage.Controls.Add(_enablePanningCheckBox, label, _panningAccelerationUpDown);
			}
			tabPage.Controls.Add(_connectionBarEnabledCheckBox, _connectionBarAutoHiddenCheckBox);
			return tabPage;
		}

		private TabPage CreateExperiencePage()
		{
			TabPage tabPage = NewTabPage("����");
			int rowIndex = 0;
			int tabIndex = 0;
			_bandwidthComboBox = FormTools.AddLabeledValueDropDown(tabPage, "&S�����ٶ�", ref rowIndex, ref tabIndex, (BandwidthItem v) => v.Text, _bandwidthItems);
			_bandwidthComboBox.SelectedIndexChanged += BandwidthCombo_ControlChanged;
			Label label = FormTools.NewLabel("��������:", 0, rowIndex);
			_desktopBackgroundCheckBox = FormTools.NewCheckBox("���汳��", 1, rowIndex++, tabIndex++);
			_desktopBackgroundCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_fontSmoothingCheckBox = FormTools.NewCheckBox("����ƽ��", 1, rowIndex++, tabIndex++);
			_fontSmoothingCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_desktopCompositionCheckBox = FormTools.NewCheckBox("���沼��", 1, rowIndex++, tabIndex++);
			_desktopCompositionCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_windowDragCheckBox = FormTools.NewCheckBox("�϶�ʱ��ʾ��������", 1, rowIndex++, tabIndex++);
			_windowDragCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_menuAnimationCheckBox = FormTools.NewCheckBox("�˵��ʹ��ڶ���", 1, rowIndex++, tabIndex++);
			_menuAnimationCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_themesCheckBox = FormTools.NewCheckBox("����", 1, rowIndex++, tabIndex++);
			_themesCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			tabPage.Controls.Add(label, _desktopBackgroundCheckBox, _fontSmoothingCheckBox, _desktopCompositionCheckBox, _windowDragCheckBox, _menuAnimationCheckBox, _themesCheckBox);
			return tabPage;
		}

		private TabPage CreateHotKeysPage()
		{
			GlobalSettings settings = Program.Preferences.Settings;
			TabPage tabPage = NewTabPage("�ȼ�");
			GroupBox groupBox = new GroupBox();
			groupBox.Text = "ALT�ȼ�������δ�ض���Windows��ϼ�ʱ��Ч��";
			GroupBox groupBox2 = groupBox;
			int rowIndex = 0;
			int tabIndex = 0;
			AddHotKeyBox(groupBox2, "ALT+TAB", "ALT+", settings.HotKeyAltTab, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox2, "ALT+SHIFT+TAB", "ALT+", settings.HotKeyAltShiftTab, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox2, "ALT+ESC", "ALT+", settings.HotKeyAltEsc, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox2, "ALT+SPACE", "ALT+", settings.HotKeyAltSpace, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox2, "CTRL+ESC", "ALT+", settings.HotKeyCtrlEsc, ref rowIndex, ref tabIndex);
			groupBox2.SizeAndLocate(null);
			GroupBox groupBox3 = new GroupBox();
			groupBox3.Text = "CTRL+ALT �ȼ���ʼ����Ч��";
			GroupBox groupBox4 = groupBox3;
			rowIndex = 0;
			tabIndex = 0;
			AddHotKeyBox(groupBox4, "CTRL+ALT+DEL", "CTRL+ALT+", settings.HotKeyCtrlAltDel, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox4, "ȫ��", "CTRL+ALT+", settings.HotKeyFullScreen, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox4, "��һ���Ự", "CTRL+ALT+", settings.HotKeyFocusReleaseLeft, ref rowIndex, ref tabIndex);
			AddHotKeyBox(groupBox4, "ѡ��Ự", "CTRL+ALT+", settings.HotKeyFocusReleaseRight, ref rowIndex, ref tabIndex);
			groupBox4.SizeAndLocate(groupBox2);
			tabPage.Controls.Add(groupBox2, groupBox4);
			return tabPage;
		}

		private void AddHotKeyBox(Control parent, string label, string prefix, EnumSetting<Keys> setting, ref int rowIndex, ref int tabIndex)
		{
			parent.Controls.Add(FormTools.NewLabel(label, 0, rowIndex));
			HotKeyBox hotKeyBox = new HotKeyBox();
			hotKeyBox.Prefix = prefix;
			hotKeyBox.Location = FormTools.NewLocation(1, rowIndex++);
			hotKeyBox.Size = new Size(340, 20);
			hotKeyBox.TabIndex = tabIndex++;
			hotKeyBox.Setting = setting;
			HotKeyBox value = hotKeyBox;
			parent.Controls.Add(value);
		}

		private TabPage CreateClientAreaPage()
		{
			_casCustomButton = new Button();
			_casCustomRadio = new RadioButton();
			_thumbnailPercentageRadio = new RadioButton();
			_thumbnailPixelsRadio = new RadioButton();
			_thumbnailPixelsButton = new Button();
			TabPage tabPage = NewTabPage("�ͻ���");
			_casSizeGroup = new GroupBox
			{
				Text = "�ͻ�����С"
			};
			_casSizeGroup.Controls.AddRange(FormTools.NewSizeRadios());
			_casCustomRadio.Size = new Size(72, 24);
			_casCustomRadio.Text = "�Զ���";
			_casSizeGroup.Controls.Add(_casCustomRadio);
			FormTools.LayoutGroupBox(_casSizeGroup, 2, null, 1, 1);
			RdcCheckBox rdcCheckBox = new RdcCheckBox();
			rdcCheckBox.Size = new Size(480, 24);
			rdcCheckBox.Text = "�������ڴ�С";
			rdcCheckBox.Location = FormTools.NewLocation(0, 0);
			rdcCheckBox.TabIndex = 0;
			rdcCheckBox.TabStop = true;
			rdcCheckBox.Setting = Program.Preferences.Settings.LockWindowSize;
			RdcCheckBox value = rdcCheckBox;
			_casSizeGroup.Controls.Add(value);
			_casCustomButton.Location = new Point(_casCustomRadio.Right + 10, _casCustomRadio.Location.Y);
			_casCustomButton.TabIndex = _casCustomRadio.TabIndex + 1;
			_casCustomButton.Click += CustomSizeClick;
			_casSizeGroup.Controls.Add(_casCustomButton);
			GroupBox groupBox = new GroupBox();
			groupBox.Size = new Size(512, 72);
			groupBox.Text = "����ͼ��λ��С";
			GroupBox groupBox2 = groupBox;
			groupBox2.Controls.Add(_thumbnailPixelsRadio, _thumbnailPercentageRadio);
			_thumbnailPixelsRadio.Size = new Size(80, 24);
			_thumbnailPixelsRadio.Text = "����";
			_thumbnailPercentageRadio.Size = new Size(88, 24);
			_thumbnailPercentageRadio.Text = "�ٷֱ�";
			_thumbnailPercentageRadio.CheckedChanged += ThumbnailPercentageRadioCheckedChanged;
			FormTools.LayoutGroupBox(groupBox2, 1, _casSizeGroup);
			int num = Math.Max(_thumbnailPixelsRadio.Right, _thumbnailPercentageRadio.Right);
			_thumbnailPixelsButton.Location = new Point(num + 10, _thumbnailPixelsRadio.Location.Y);
			_thumbnailPixelsButton.TabIndex = _thumbnailPercentageRadio.TabIndex + 1;
			_thumbnailPixelsButton.Click += CustomSizeClick;
			_thumbnailPercentageTextBox = new NumericTextBox(1, 100, "�ٷֱȱ������1��100֮�䣨��1��100��");
			_thumbnailPercentageTextBox.Enabled = false;
			_thumbnailPercentageTextBox.Location = new Point(num + 11, _thumbnailPercentageRadio.Location.Y + 2);
			_thumbnailPercentageTextBox.Size = new Size(72, 20);
			_thumbnailPercentageTextBox.TabIndex = _thumbnailPercentageRadio.TabIndex + 1;
			groupBox2.Controls.Add(_thumbnailPixelsButton, _thumbnailPercentageTextBox);
			tabPage.Controls.Add(_casSizeGroup, groupBox2);
			return tabPage;
		}

		private TabPage CreateServerTreePage()
		{
			int rowIndex = 0;
			int tabIndex = 0;
			TabPage tabPage = NewTabPage("Ⱥ����");
			GroupBox groupBox = new GroupBox();
			groupBox.Text = "��������";
			GroupBox groupBox2 = groupBox;
			FormTools.AddCheckBox(groupBox2, "������ѡ�н������Ƶ�Զ�̿ͻ���", Program.Preferences.Settings.FocusOnClick, 0, ref rowIndex, ref tabIndex);
			FormTools.AddCheckBox(groupBox2, "���ؼ����ڷǻ״̬ʱʹ�ڵ�䰵", Program.Preferences.Settings.DimNodesWhenInactive, 0, ref rowIndex, ref tabIndex);
			_treeLocationCombo = FormTools.AddLabeledValueDropDown(groupBox2, "λ��", ref rowIndex, ref tabIndex, (DockStyle v) => v.ToString(), new DockStyle[2]
			{
				DockStyle.Left,
				DockStyle.Right
			});
			_treeVisibilityCombo = FormTools.AddLabeledValueDropDown(groupBox2, "�ɼ���ʽ", ref rowIndex, ref tabIndex, (ControlVisibility v) => v.ToString(), new ControlVisibility[3]
			{
				ControlVisibility.Dock,
				ControlVisibility.AutoHide,
				ControlVisibility.Hide
			});
			Label label = FormTools.NewLabel("�����ӳ�:", 0, rowIndex++);
			label.Left += 24;
			label.Size = new Size(80, label.Height);
			NumericTextBox serverTreeAutoHidePopUpDelay = new NumericTextBox(0, 1000, "�Զ����ص����ӳ�ʱ�����Ϊ0��1000����")
			{
				Enabled = false,
				Location = new Point(label.Right, label.Top),
				Size = new Size(40, 24),
				Setting = Program.Preferences.Settings.ServerTreeAutoHidePopUpDelay,
				TabStop = true,
				TabIndex = tabIndex++
			};
			_treeVisibilityCombo.SelectedIndexChanged += delegate
			{
				serverTreeAutoHidePopUpDelay.Enabled = (_treeVisibilityCombo.SelectedValue == ControlVisibility.AutoHide);
			};
			groupBox2.AddControlsAndSizeGroup(label);
			Label label2 = new Label();
			label2.Location = new Point(serverTreeAutoHidePopUpDelay.Right + 3, label.Top);
			label2.Size = new Size(80, 24);
			label2.Text = "����(s)";
			Label label3 = label2;
			groupBox2.Controls.Add(serverTreeAutoHidePopUpDelay, label3);
			groupBox2.SizeAndLocate(null);
			_virtualGroupsGroup = new GroupBox
			{
				Text = "����Ⱥ��"
			};
			foreach (IBuiltInVirtualGroup item in Program.BuiltInVirtualGroups.Where((IBuiltInVirtualGroup group) => group.IsVisibilityConfigurable))
			{
				_virtualGroupsGroup.Controls.Add(new CheckBox
				{
					Size = new Size(112, 24),
					Tag = item,
					Text = item.Text
				});
			}
			FormTools.LayoutGroupBox(_virtualGroupsGroup, 2, groupBox2);
			rowIndex = 0;
			tabIndex = 0;
			GroupBox groupBox3 = new GroupBox();
			FormTools.AddLabeledValueDropDown(groupBox3, "������ʽ", Program.Preferences.Settings.GroupSortOrder, ref rowIndex, ref tabIndex, Helpers.SortOrderToString, new SortOrder[2]
			{
				SortOrder.ByName,
				SortOrder.None
			});
			FormTools.AddLabeledEnumDropDown(groupBox3, "����������ʽ", Program.Preferences.Settings.ServerSortOrder, ref rowIndex, ref tabIndex, Helpers.SortOrderToString);
			groupBox3.Text = "����ʽ";
			FormTools.LayoutGroupBox(groupBox3, 2, _virtualGroupsGroup);
			tabPage.Controls.Add(groupBox2, groupBox3, _virtualGroupsGroup);
			return tabPage;
		}

		private TabPage CreateGeneralPage()
		{
			int rowIndex = 0;
			int tabIndex = 0;
			TabPage tabPage = NewTabPage("����");
			FormTools.AddCheckBox(tabPage, "�������˵���ֱ����ALT��", Program.Preferences.Settings.HideMainMenu, 0, ref rowIndex, ref tabIndex);
			RdcCheckBox autoSaveCheckBox = FormTools.AddCheckBox(tabPage, "�Զ�������:", Program.Preferences.Settings.AutoSaveFiles, 0, ref rowIndex, ref tabIndex);
			autoSaveCheckBox.Size = new Size(120, 24);
			NumericTextBox autoSaveInterval = new NumericTextBox(0, 60, "�Զ�����������Ϊ0��60����")
			{
				Location = new Point(autoSaveCheckBox.Right + 1, autoSaveCheckBox.Top + 2),
				Size = new Size(20, 24),
				TabIndex = tabIndex++,
				TabStop = true,
				Enabled = false
			};
			autoSaveInterval.Setting = Program.Preferences.Settings.AutoSaveInterval;
			autoSaveCheckBox.CheckedChanged += delegate
			{
				autoSaveInterval.Enabled = autoSaveCheckBox.Checked;
			};
			Label label = new Label();
			label.Location = new Point(autoSaveInterval.Right + 3, autoSaveCheckBox.Top + 4);
			label.Size = new Size(60, 24);
			label.Text = "����(s)";
			Label label2 = label;
			RdcCheckBox rdcCheckBox = FormTools.AddCheckBox(tabPage, "����ʱ��ʾ�����������ӵķ�����", Program.Preferences.Settings.ReconnectOnStartup, 0, ref rowIndex, ref tabIndex);
			Button button = new Button();
			button.Location = new Point(8, rdcCheckBox.Bottom + 8);
			button.TabIndex = tabIndex++;
			button.Text = "Ĭ��Ⱥ������...";
			button.Width = 140;
			Button button2 = button;
			button2.Click += delegate
			{
				DefaultSettingsGroup.Instance.DoPropertiesDialog();
			};
			tabPage.Controls.Add(autoSaveCheckBox, autoSaveInterval, label2, button2);
			return tabPage;
		}

		private void EnablePanningCheckedChanged(object sender, EventArgs e)
		{
			_panningAccelerationUpDown.Enabled = _enablePanningCheckBox.Checked;
		}

		private void ConnectionBarEnabledCheckedChanged(object sender, EventArgs e)
		{
			_connectionBarAutoHiddenCheckBox.Enabled = _connectionBarEnabledCheckBox.Checked;
		}

		private void CustomSizeClick(object sender, EventArgs e)
		{
			Button button = sender as Button;
			Size size = SizeHelper.Parse(button.Text);
			using (CustomSizeDialog customSizeDialog = new CustomSizeDialog(size))
			{
				if (customSizeDialog.ShowDialog() == DialogResult.OK)
				{
					button.Text = customSizeDialog.WidthText + SizeHelper.Separator + customSizeDialog.HeightText;
					_thumbnailPixelsRadio.Checked = true;
				}
			}
		}

		private void ThumbnailPercentageRadioCheckedChanged(object sender, EventArgs e)
		{
			_thumbnailPercentageTextBox.Enabled = (sender as RadioButton).Checked;
		}

		private void BandwidthCombo_ControlChanged(object sender, EventArgs e)
		{
			BandwidthSettingsChanged();
		}

		private void BandwidthSettingsChanged()
		{
			if (!_inHandler)
			{
				_inHandler = true;
				SetBandwidthCheckBoxes(_bandwidthComboBox.SelectedValue.Flags);
				_inHandler = false;
			}
		}

		private void SetBandwidthCheckBoxes(int flags)
		{
			_desktopBackgroundCheckBox.Checked = ((flags & 1) == 0);
			_fontSmoothingCheckBox.Checked = ((flags & 0x80) != 0);
			_desktopCompositionCheckBox.Checked = ((flags & 0x100) != 0);
			_windowDragCheckBox.Checked = ((flags & 2) == 0);
			_menuAnimationCheckBox.Checked = ((flags & 4) == 0);
			_themesCheckBox.Checked = ((flags & 8) == 0);
		}

		private void PerfCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!_inHandler)
			{
				_inHandler = true;
				int flags = ComputeFlagsFromCheckBoxes();
				BandwidthItem selectedValue = _bandwidthItems.Where((BandwidthItem i) => i.Flags == flags).FirstOrDefault() ?? _bandwidthItems.First((BandwidthItem i) => i.Text.Equals("�Զ���"));
				_bandwidthComboBox.SelectedValue = selectedValue;
				_inHandler = false;
			}
		}

		private int ComputeFlagsFromCheckBoxes()
		{
			int num = 0;
			if (!_desktopBackgroundCheckBox.Checked)
			{
				num |= 1;
			}
			if (_fontSmoothingCheckBox.Checked)
			{
				num |= 0x80;
			}
			if (_desktopCompositionCheckBox.Checked)
			{
				num |= 0x100;
			}
			if (!_windowDragCheckBox.Checked)
			{
				num |= 2;
			}
			if (!_menuAnimationCheckBox.Checked)
			{
				num |= 4;
			}
			if (!_themesCheckBox.Checked)
			{
				num |= 8;
			}
			return num;
		}
	}
}
