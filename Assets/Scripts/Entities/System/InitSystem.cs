using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

namespace HitEngine.Entities
{
    [DisableAutoCreation]
    public class InitSystem : SystemBase
    {
        // 背景的长宽
        private const float m_BackgroundLength = 100f;
        // 一个plane默认的长宽
        private const float m_PlaneLength = 10f;

        protected override void OnCreate()
        {
            base.OnCreate();

            InitBackground();
            SpawnHitable(500);
        }

        protected override void OnUpdate()
        {
        }

        private void InitBackground()
        {
            var backgroundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            backgroundGo.name = "Background";
            backgroundGo.transform.position = new Vector3(0, 0, 0.1f);
            backgroundGo.transform.up = Vector3.back;
            backgroundGo.transform.localScale = new Vector3(m_BackgroundLength / m_PlaneLength, 1, m_BackgroundLength / m_PlaneLength);
            backgroundGo.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            var backgroundEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(backgroundGo, settings);

            EntityManager.AddComponentData(backgroundEntity, new BackgroundComponent
            {
                left = -m_BackgroundLength / 2,
                right = m_BackgroundLength / 2,
                top = m_BackgroundLength / 2,
                bottom = -m_BackgroundLength / 2
            });

            // NullReferenceException:
            //Camera.main.orthographicSize = m_BackgroundLength / 2;
        }

        private void SpawnHitable(int count)
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            var random = new Unity.Mathematics.Random(1);
            for (var i = 0; i < count; i++)
            {
                var entityGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(entityGO, settings);
                EntityManager.AddComponentData(entity, new CircleHitableComponent { radius = random.NextFloat(1, 3) });
                EntityManager.AddComponentData(entity, new IsInHitComponent { value = false });
                EntityManager.AddComponentData(entity, new InQuadtreeIdxComponent { value = -1 });
                EntityManager.AddComponentData(entity, new AABB());

                var randSpeed = new float2(random.NextFloat(-10, 10), random.NextFloat(-10, 10));
                EntityManager.AddComponentData(entity, new MoverBySpeedComponent { speed = randSpeed }); ;
            }
        }
    }
}