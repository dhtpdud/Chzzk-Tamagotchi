using OSY;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RestrictedColliderUI : MonoBehaviour
{
    public Entity colliderEntity;
    private void Start()
    {
        colliderEntity = default;
        UpdateColliderEntity();
    }
    public async void UpdateColliderEntity()
    {
        RestrictedColliderUIUpdateSystem ColliderUpdateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<RestrictedColliderUIUpdateSystem>();
        await Utils.WaitUntil(() => GameManager.instance?.rootCanvas != null &&ColliderUpdateSystem.isReady, Utils.YieldCaches.UniTaskYield, destroyCancellationToken);

        float size = GameManager.instance.rootCanvas.transform.localScale.x;
        Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta * size;
        if (!destroyCancellationToken.IsCancellationRequested)
            ColliderUpdateSystem.UpdateColliderEntity(ref colliderEntity, new float3(transform.position.x, transform.position.y, 0), new float3(sizeDelta.x, sizeDelta.y, 10));
    }
    public void DestroySelf()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entityManager.Exists(colliderEntity))
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(colliderEntity);
        Destroy(gameObject);
    }
}
