using System;
using System.Drawing;

namespace RdcMan
{
	public class GroupDisplaySettingsTabPage : DisplaySettingsTabPage<GroupDisplaySettings>
	{
		private readonly RdcCheckBox _previewCheckBox;

		private readonly RdcCheckBox _interactionCheckBox;

		public GroupDisplaySettingsTabPage(TabbedSettingsDialog dialog, GroupDisplaySettings settings)
			: base(dialog, settings)
		{
			Create(out int rowIndex, out int tabIndex);
			_previewCheckBox = FormTools.AddCheckBox(this, "������ͼԤ���Ự", settings.SessionThumbnailPreview, 0, ref rowIndex, ref tabIndex);
			_interactionCheckBox = FormTools.AddCheckBox(this, "��������ͼ�Ự����", settings.AllowThumbnailSessionInteraction, 0, ref rowIndex, ref tabIndex);
			_interactionCheckBox.Location = new Point(_previewCheckBox.Left + 24, _interactionCheckBox.Top);
			RdcCheckBox previewCheckBox = _previewCheckBox;
			EventHandler value = delegate
			{
				PreviewCheckBoxChanged();
			};
			previewCheckBox.CheckedChanged += value;
			if (base.InheritanceControl != null)
			{
				base.InheritanceControl.EnabledChanged += delegate
				{
					PreviewCheckBoxChanged();
				};
			}
			FormTools.AddCheckBox(this, "��ʾ�Ͽ�������ͼ", settings.ShowDisconnectedThumbnails, 0, ref rowIndex, ref tabIndex);
		}

		private void PreviewCheckBoxChanged()
		{
			_interactionCheckBox.Enabled = (_previewCheckBox.Checked && _previewCheckBox.Enabled);
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			PreviewCheckBoxChanged();
		}
	}
}
