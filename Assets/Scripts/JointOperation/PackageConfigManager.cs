using UnityEngine;

public class PackageConfigManager:SimpleSingleton<PackageConfigManager>
{

    private IWiOSPackageConfig _iwiOSConfig;
    private NormalPackageConfig _normalConfig;

    //注意：editor文件夹下的脚本不可调用此方法,否则获得的结果无效
    public BasePackageConfig CurPackageConfig
    {
        get
        {
            BasePackageConfig result = null;
#if USE_iOS_IW
            result = _iwiOSConfig ?? new IWiOSPackageConfig();
#else
            result = _normalConfig ?? new NormalPackageConfig();
#endif
            return result;
        }
    }

    public BasePackageConfig GetiOSBuildConfig(bool isIwBuild)
    {
        BasePackageConfig result;

        if (isIwBuild)
            result = _iwiOSConfig ?? new IWiOSPackageConfig();
        else
            result = _normalConfig ?? new NormalPackageConfig();

        return result;
    }


}
