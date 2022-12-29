using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


    public readonly partial struct MoveAspect : global::Unity.Entities.IAspect, global::Unity.Entities.IAspectCreate<MoveAspect>
    {
        MoveAspect(global::Unity.Entities.Entity entity,global::Unity.Transforms.TransformAspect transformaspect,global::Unity.Entities.RefRO<global::SpeedComponent> speed,global::Unity.Entities.RefRO<global::SizeComponent> size,global::Unity.Entities.RefRW<global::TargetPositionComponent> targetposition)
        {
            this.transformAspect = transformaspect;
            this.speed = speed;
            this.size = size;
            this.targetPosition = targetposition;

            this.entity = entity;

        }
        public MoveAspect CreateAspect(global::Unity.Entities.Entity entity, ref global::Unity.Entities.SystemState systemState, bool isReadOnly)
        {
            var lookup = new Lookup(ref systemState, isReadOnly);
            return lookup[entity];
        }

        public static global::Unity.Entities.ComponentType[] ExcludeComponents => global::System.Array.Empty<Unity.Entities.ComponentType>();
        static global::Unity.Entities.ComponentType[] s_RequiredComponents => global::Unity.Entities.ComponentType.Combine(new [] {  global::Unity.Entities.ComponentType.ReadOnly<global::SpeedComponent>(), global::Unity.Entities.ComponentType.ReadOnly<global::SizeComponent>(), global::Unity.Entities.ComponentType.ReadWrite<global::TargetPositionComponent>() },  Unity.Transforms.TransformAspect.RequiredComponents);
        static global::Unity.Entities.ComponentType[] s_RequiredComponentsRO => global::Unity.Entities.ComponentType.Combine(new [] {  global::Unity.Entities.ComponentType.ReadOnly<global::SpeedComponent>(), global::Unity.Entities.ComponentType.ReadOnly<global::SizeComponent>(), global::Unity.Entities.ComponentType.ReadOnly<global::TargetPositionComponent>() },  Unity.Transforms.TransformAspect.RequiredComponentsRO);
        public static global::Unity.Entities.ComponentType[] RequiredComponents => s_RequiredComponents;
        public static global::Unity.Entities.ComponentType[] RequiredComponentsRO => s_RequiredComponentsRO;
        public struct Lookup
        {
            bool _IsReadOnly
            {
                get { return __IsReadOnly == 1; }
                set { __IsReadOnly = value ? (byte) 1 : (byte) 0; }
            }
            private byte __IsReadOnly;

            [global::Unity.Collections.ReadOnly]
            global::Unity.Entities.ComponentLookup<global::SpeedComponent> speedComponentLookup;
            [global::Unity.Collections.ReadOnly]
            global::Unity.Entities.ComponentLookup<global::SizeComponent> sizeComponentLookup;
            global::Unity.Entities.ComponentLookup<global::TargetPositionComponent> targetPositionComponentLookup;


            global::Unity.Transforms.TransformAspect.Lookup transformAspect;
            public Lookup(ref global::Unity.Entities.SystemState state, bool isReadOnly)
            {
                __IsReadOnly = isReadOnly ? (byte) 1u : (byte) 0u;
                this.speedComponentLookup = state.GetComponentLookup<global::SpeedComponent>(true);
                this.sizeComponentLookup = state.GetComponentLookup<global::SizeComponent>(true);
                this.targetPositionComponentLookup = state.GetComponentLookup<global::TargetPositionComponent>(isReadOnly);


                this.transformAspect = new global::Unity.Transforms.TransformAspect.Lookup(ref state, isReadOnly);
            }
            public void Update(ref global::Unity.Entities.SystemState state)
            {
                this.speedComponentLookup.Update(ref state);
                this.sizeComponentLookup.Update(ref state);
                this.targetPositionComponentLookup.Update(ref state);

                this.transformAspect.Update(ref state);
            }
            public MoveAspect this[global::Unity.Entities.Entity entity]
            {
                get
                {
                    return new MoveAspect(entity,this.transformAspect[entity],this.speedComponentLookup.GetRefRO(entity),this.sizeComponentLookup.GetRefRO(entity),this.targetPositionComponentLookup.GetRefRW(entity, _IsReadOnly));
                }
            }
        }
        public struct ResolvedChunk
        {
            internal global::Unity.Collections.NativeArray<global::Unity.Entities.Entity> m_Entities;
            internal global::Unity.Collections.NativeArray<global::SpeedComponent> speed;
            internal global::Unity.Collections.NativeArray<global::SizeComponent> size;
            internal global::Unity.Collections.NativeArray<global::TargetPositionComponent> targetPosition;

            internal global::Unity.Transforms.TransformAspect.ResolvedChunk transformAspect;
            public MoveAspect this[int index]
            {
                get
                {
                    return new MoveAspect(m_Entities[index],
this.transformAspect[index],
                        new global::Unity.Entities.RefRO<SpeedComponent>(this.speed, index),
                        new global::Unity.Entities.RefRO<SizeComponent>(this.size, index),
                        new global::Unity.Entities.RefRW<TargetPositionComponent>(this.targetPosition, index));
                }
            }
            public int Length;
        }
        public struct TypeHandle
        {
            [global::Unity.Collections.ReadOnly]
            global::Unity.Entities.ComponentTypeHandle<global::SpeedComponent> speedCth;
            [global::Unity.Collections.ReadOnly]
            global::Unity.Entities.ComponentTypeHandle<global::SizeComponent> sizeCth;
            global::Unity.Entities.ComponentTypeHandle<global::TargetPositionComponent> targetPositionCth;

            global::Unity.Entities.EntityTypeHandle m_Entities;

            global::Unity.Transforms.TransformAspect.TypeHandle transformAspect;
            public TypeHandle(ref global::Unity.Entities.SystemState state, bool isReadOnly)
            {
                this.speedCth = state.GetComponentTypeHandle<global::SpeedComponent>(true);
                this.sizeCth = state.GetComponentTypeHandle<global::SizeComponent>(true);
                this.targetPositionCth = state.GetComponentTypeHandle<global::TargetPositionComponent>(isReadOnly);

                this.m_Entities = state.GetEntityTypeHandle();

                this.transformAspect = new global::Unity.Transforms.TransformAspect.TypeHandle(ref state, isReadOnly);
            }
            public void Update(ref global::Unity.Entities.SystemState state)
            {
                this.speedCth.Update(ref state);
                this.sizeCth.Update(ref state);
                this.targetPositionCth.Update(ref state);

                this.m_Entities.Update(ref state);
                this.transformAspect.Update(ref state);
            }
            public ResolvedChunk Resolve(global::Unity.Entities.ArchetypeChunk chunk)
            {
                ResolvedChunk resolved;
                resolved.m_Entities = chunk.GetNativeArray(this.m_Entities);
                resolved.transformAspect = this.transformAspect.Resolve(chunk);
                resolved.speed = chunk.GetNativeArray(this.speedCth);
                resolved.size = chunk.GetNativeArray(this.sizeCth);
                resolved.targetPosition = chunk.GetNativeArray(this.targetPositionCth);

                resolved.Length = chunk.Count;
                return resolved;
            }
        }
        public static Enumerator Query(global::Unity.Entities.EntityQuery query, TypeHandle typeHandle) { return new Enumerator(query, typeHandle); }
        public struct Enumerator : global::System.Collections.Generic.IEnumerator<MoveAspect>, global::System.Collections.Generic.IEnumerable<MoveAspect>
        {
            ResolvedChunk                                _Resolved;
            global::Unity.Entities.EntityQueryEnumerator _QueryEnumerator;
            TypeHandle                                   _Handle;
            internal Enumerator(global::Unity.Entities.EntityQuery query, TypeHandle typeHandle)
            {
                _QueryEnumerator = new global::Unity.Entities.EntityQueryEnumerator(query);
                _Handle = typeHandle;
                _Resolved = default;
            }
            public void Dispose() { _QueryEnumerator.Dispose(); }
            public bool MoveNext()
            {
                if (_QueryEnumerator.MoveNextHotLoop())
                    return true;
                return MoveNextCold();
            }
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            bool MoveNextCold()
            {
                var didMove = _QueryEnumerator.MoveNextColdLoop(out var chunk);
                if (didMove)
                    _Resolved = _Handle.Resolve(chunk);
                return didMove;
            }
            public MoveAspect Current {
                get {
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
                        _QueryEnumerator.CheckDisposed();
                    #endif
                        return _Resolved[_QueryEnumerator.IndexInChunk];
                    }
            }
            public Enumerator GetEnumerator()  { return this; }
            void global::System.Collections.IEnumerator.Reset() => throw new global::System.NotImplementedException();
            object global::System.Collections.IEnumerator.Current => throw new global::System.NotImplementedException();
            global::System.Collections.Generic.IEnumerator<MoveAspect> global::System.Collections.Generic.IEnumerable<MoveAspect>.GetEnumerator() => throw new global::System.NotImplementedException();
            global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()=> throw new global::System.NotImplementedException();
        }

        /// <summary>
        /// Completes the dependency chain required for this aspect to have read access.
        /// So it completes all write dependencies of the components, buffers, etc. to allow for reading.
        /// </summary>
        /// <param name="state">The <see cref="SystemState"/> containing an <see cref="EntityManager"/> storing all dependencies.</param>
        public static void CompleteDependencyBeforeRO(ref global::Unity.Entities.SystemState state){
           Unity.Transforms.TransformAspect.CompleteDependencyBeforeRW(ref state);
           state.EntityManager.CompleteDependencyBeforeRO<global::SpeedComponent>();
           state.EntityManager.CompleteDependencyBeforeRO<global::SizeComponent>();
           state.EntityManager.CompleteDependencyBeforeRO<global::TargetPositionComponent>();
        }

        /// <summary>
        /// Completes the dependency chain required for this component to have read and write access.
        /// So it completes all write dependencies of the components, buffers, etc. to allow for reading,
        /// and it completes all read dependencies, so we can write to it.
        /// </summary>
        /// <param name="state">The <see cref="SystemState"/> containing an <see cref="EntityManager"/> storing all dependencies.</param>
        public static void CompleteDependencyBeforeRW(ref global::Unity.Entities.SystemState state){
           Unity.Transforms.TransformAspect.CompleteDependencyBeforeRO(ref state);
           state.EntityManager.CompleteDependencyBeforeRO<global::SpeedComponent>();
           state.EntityManager.CompleteDependencyBeforeRO<global::SizeComponent>();
           state.EntityManager.CompleteDependencyBeforeRW<global::TargetPositionComponent>();
        }
    }
