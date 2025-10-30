using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace JobMountRoulette;

public sealed class MountInventory
{
    private readonly ITextureProvider mTextureProvider;
    private readonly List<Mount> mMounts;

    public MountInventory(IDataManager dataManager, ITextureProvider textureProvider)
    {
        mTextureProvider = textureProvider;
        mMounts = (from mount in dataManager.GameData.GetExcelSheet<Lumina.Excel.Sheets.Mount>()
                   where mount.UIPriority > 0 && mount.Icon != 0
                   orderby mount.UIPriority, mount.RowId
                   select new Mount(mount, mTextureProvider)).ToList();
    }

    public List<Mount> GetUnlockedMounts()
    {
        return mMounts.Where(x => x.isUnlocked()).ToList();
    }

    public List<Mount> GetAvailableMounts()
    {
        return mMounts;
    }
}
