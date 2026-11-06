using Mirror;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Features.ClientSideToys;

public sealed class ClientSideToyAnchor : MonoBehaviour
{
    public readonly uint NetId = NetworkIdentity.GetNextNetworkId();
    public SchematicObject SchematicObject { get; private set; }
    public int ObjectId { get; private set; }

    public void Init(int objectId, SchematicObject schematicObject)
    {
        if (SchematicObject != null || schematicObject == null)
            return;
        
        SchematicObject = schematicObject;
        ObjectId = objectId;
    }
    
    private void OnDestroy()
    {
        SchematicObject.DestroyClientSide(ObjectId);
        SchematicObject.ClientSideAdminToys.Remove(ObjectId);
    }
}
