using Mirror;
using UnityEngine;

namespace ProjectMER.Features.ClientSideToys;

public sealed class ClientSideCullingParent : ClientSideAdminToy
{
    public Vector3 Size { get; set; }
    public Vector3 BoundsPosition { get; set; }
    protected override uint AssetID => PrefabManager.CullingParent.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        writer.WriteVector3(BoundsPosition);
        writer.WriteVector3(Size);
    }
}