namespace RdcMan
{
	/// <summary>
	/// ��¼��֤��Ϣ�ӿ�
	/// </summary>
	public interface ILogonCredentials {
		string ProfileName { get; }

		ProfileScope ProfileScope { get; }

		string UserName { get; }

		PasswordSetting Password { get; }

		string Domain { get; }
	}
}
