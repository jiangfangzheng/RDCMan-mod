using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SaveCredentialsDialog : RdcDialog
	{
		private RdcTextBox _profileNameTextBox;

		private ValueComboBox<ProfileScope> _locationComboBox;

		private FileGroup _file;

		public string ProfileName => _profileNameTextBox.Text;

		public ProfileScope ProfileScope => _locationComboBox.SelectedValue;

		public SaveCredentialsDialog(FileGroup file, string name)
			: base("��������: " + name, "����")
		{
			_file = file;
			InitializeComponent(name);
		}

		private void InitializeComponent(string name)
		{
			base.Size = new System.Drawing.Size(512, 150);
			int rowIndex = 0;
			int tabIndex = 0;
			_profileNameTextBox = RdcMan.FormTools.AddLabeledTextBox(this, "�����ļ���", ref rowIndex, ref tabIndex);
			_profileNameTextBox.Enabled = true;
			_profileNameTextBox.Text = name;
			_profileNameTextBox.Validate = new System.Func<string>(ValidateProfileName);
			_locationComboBox = RdcMan.FormTools.AddLabeledValueDropDown<RdcMan.ProfileScope>(this, "λ��", ref rowIndex, ref tabIndex, null, null);
			_locationComboBox.AddItem("Global", RdcMan.ProfileScope.Global);
			_locationComboBox.SelectedIndex = 0;
			if (_file != null)
			{
				_locationComboBox.AddItem(_file.Text, RdcMan.ProfileScope.File);
				_locationComboBox.SelectedIndex = 1;
			}
			InitButtons();
			this.ScaleAndLayout();
		}

		private string ValidateProfileName()
		{
			_profileNameTextBox.Text = _profileNameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(_profileNameTextBox.Text))
			{
				return "�����������ļ���";
			}
			if (LogonCredentials.IsCustomProfile(ProfileName))
			{
				return "'{0}' �Ǳ����������ļ�����".InvariantFormat("Custom");
			}
			CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
			string text = "Global";
			if (ProfileScope == ProfileScope.File)
			{
				credentialsProfiles = _file.CredentialsProfiles;
				text = _file.Text;
			}
			if (credentialsProfiles.Contains(ProfileName))
			{
				DialogResult dialogResult = FormTools.YesNoDialog(ProfileName + " �Ѿ����� " + text +"��"+ Environment.NewLine + "�Ƿ����?", MessageBoxDefaultButton.Button2);
				if (dialogResult != DialogResult.Yes)
				{
					return "�����ļ��Ѵ���";
				}
			}
			return null;
		}
	}
}
