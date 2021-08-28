using HitScoreVisualizer.Services;
using HitScoreVisualizer.Settings;
using IPA.Logging;
using SiraUtil;
using Zenject;

namespace HitScoreVisualizer.Installers
{
	internal class HsvAppInstaller : Installer<Logger, HSVConfig, HsvAppInstaller>
	{
		private readonly Logger _logger;
		private readonly HSVConfig _hsvConfig;

		internal HsvAppInstaller(Logger logger, HSVConfig hsvConfig)
		{
			_logger = logger;
			_hsvConfig = hsvConfig;
		}

		public override void InstallBindings()
		{
			Container.BindLoggerAsSiraLogger(_logger);
			Container.BindInstance(_hsvConfig);
			Container.BindInterfacesAndSelfTo<ConfigProvider>().AsSingle();
			Container.BindInterfacesAndSelfTo<BloomFontProvider>().AsSingle();
		}
	}
}