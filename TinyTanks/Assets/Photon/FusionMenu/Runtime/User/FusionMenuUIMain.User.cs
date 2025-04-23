namespace Fusion.Menu
{
	using Fusion.Photon.Realtime;

	public partial class FusionMenuUIMain
	{
		partial void InitUser()
		{
			ConnectionArgs.AppVersion = PhotonAppSettings.Global.AppSettings.AppVersion;
		}
	}
}
