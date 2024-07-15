using OSY;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RestrictedColliderUI : MonoBehaviour
{
    public Entity colliderEntity;
    private async void Start()
    {
        colliderEntity = default;
        await Utils.WaitUntil(() => World.DefaultGameObjectInjectionWorld.IsCreated && World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<RestrictedColliderUIUpdateSystem>() != null, Utils.YieldCaches.UniTaskYield, destroyCancellationToken);
        await Utils.YieldCaches.UniTaskYield;
        UpdateColliderEntity();
        await Utils.YieldCaches.UniTaskYield;
        UpdateColliderEntity();
    }
    public void UpdateColliderEntity()
    {
        float size = GameManager.instance.rootCanvas.transform.localScale.x;
        Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta * size;
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<RestrictedColliderUIUpdateSystem>().UpdateColliderEntity(ref colliderEntity, new float3(transform.position.x, transform.position.y, 0), new float3(sizeDelta.x, sizeDelta.y, 1));
    }
    public void DestroySelf()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entityManager.Exists(colliderEntity))
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(colliderEntity);
        Destroy(gameObject);
    }
}
