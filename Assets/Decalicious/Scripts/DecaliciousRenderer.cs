using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeEyedGames
{
    [ExecuteInEditMode]
    public class DecaliciousRenderer : MonoBehaviour
    {
#if UNITY_5_5_OR_NEWER
        [HideInInspector]
        public bool UseInstancing = true;
#endif

        protected CommandBuffer _bufferDeferred = null;
        protected CommandBuffer _bufferUnlit = null;
        protected CommandBuffer _bufferLimitTo = null;
        protected Dictionary<Material, HashSet<Decal>> _deferredDecals;
        protected Dictionary<Material, HashSet<Decal>> _unlitDecals;
        protected List<MeshRenderer> _limitToMeshRenderers;
        protected List<SkinnedMeshRenderer> _limitToSkinnedMeshRenderers;
        protected HashSet<GameObject> _limitToGameObjects;
        protected List<Decal> _decalComponent;
        protected List<MeshFilter> _meshFilterComponent;

        protected const string _bufferBaseName = "Decalicious - ";
        protected const string _bufferDeferredName = _bufferBaseName + "Deferred";
        protected const string _bufferUnlitName = _bufferBaseName + "Unlit";
        protected const string _bufferLimitToName = _bufferBaseName + "Limit To Game Objects";
        protected const CameraEvent _camEventDeferred = CameraEvent.BeforeReflections;
        protected const CameraEvent _camEventUnlit = CameraEvent.BeforeImageEffectsOpaque;
        protected const CameraEvent _camEventLimitTo = CameraEvent.AfterGBuffer;

        protected Camera _camera;
        protected bool _camLastKnownHDR;
        protected static Mesh _cubeMesh = null;

        protected Matrix4x4[] _matrices;
        protected float[] _fadeValues;
        protected float[] _limitToValues;
        protected MaterialPropertyBlock _block;
        protected RenderTargetIdentifier[] _albedoRenderTarget;
        protected RenderTargetIdentifier[] _normalRenderTarget;
        protected Material _materialLimitToGameObjects;

        //// TODO: Currently only this is only one mesh, find a good solution that produces
        //// no garbage (as in, neither GC allocs nor dead meshes)
        //private Mesh temporaryMesh = null;

        void OnEnable()
        {
            _deferredDecals = new Dictionary<Material, HashSet<Decal>>();
            _unlitDecals = new Dictionary<Material, HashSet<Decal>>();
            _limitToMeshRenderers = new List<MeshRenderer>();
            _limitToSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
            _limitToGameObjects = new HashSet<GameObject>();
            _decalComponent = new List<Decal>();
            _meshFilterComponent = new List<MeshFilter>();
            _matrices = new Matrix4x4[1023];
            _fadeValues = new float[1023];
            _limitToValues = new float[1023];
            _block = new MaterialPropertyBlock();
            _camera = GetComponent<Camera>();
            _cubeMesh = Resources.Load<Mesh>("DecalCube");
            _normalRenderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.GBuffer1, BuiltinRenderTextureType.GBuffer2 };
        }

        void OnDisable()
        {
            if (_bufferDeferred != null)
            {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventDeferred, _bufferDeferred);
                _bufferDeferred = null;
            }

            if (_bufferUnlit != null)
            {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventUnlit, _bufferUnlit);
                _bufferUnlit = null;
            }

            if (_bufferLimitTo != null)
            {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventLimitTo, _bufferLimitTo);
                _bufferLimitTo = null;
            }
        }

        void OnPreRender()
        {
#if UNITY_5_5_OR_NEWER
            if (!SystemInfo.supportsInstancing)
                UseInstancing = false;
#endif

            if (_albedoRenderTarget == null || _camera.hdr != _camLastKnownHDR)
            {
                _camLastKnownHDR = _camera.hdr;
                _albedoRenderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.GBuffer0,
                    _camLastKnownHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3 };
            }

            // Make sure that command buffers are created
            CreateBuffer(ref _bufferDeferred, _camera, _bufferDeferredName, _camEventDeferred);
            CreateBuffer(ref _bufferUnlit, _camera, _bufferUnlitName, _camEventUnlit);
            CreateBuffer(ref _bufferLimitTo, _camera, _bufferLimitToName, _camEventLimitTo);

            // Render Game Objects that are special decal targets
            _bufferLimitTo.Clear();
            DrawLimitToGameObjects(_camera);

            // Prepare command buffer for deferred decals
            _bufferDeferred.Clear();
            DrawDeferredDecals_Albedo(_camera);
            DrawDeferredDecals_NormSpecSmooth(_camera);

            // Prepare command buffer for unlit decals
            _bufferUnlit.Clear();
            DrawUnlitDecals(_camera);

            // TODO: Materials that are no longer used will never be removed from the dictionary -
            //   which should not be a big thing, but anyway.

            // Clear deferred decal list for next frame
            var decalEnum =  _deferredDecals.GetEnumerator();
            while (decalEnum.MoveNext())
                decalEnum.Current.Value.Clear();

            // Clear unlit decal list for next frame
            decalEnum = _unlitDecals.GetEnumerator();
            while (decalEnum.MoveNext())
                decalEnum.Current.Value.Clear();

            // Clear limit to targets for next frame
            _limitToGameObjects.Clear();
        }

        private void DrawLimitToGameObjects(Camera cam)
        {
            if (_limitToGameObjects.Count == 0)
                return;

            if (_materialLimitToGameObjects == null)
                _materialLimitToGameObjects = new Material(Shader.Find("Hidden/Decalicious Game Object ID"));

            var limitToId = Shader.PropertyToID("_DecaliciousLimitToGameObject");
            _bufferLimitTo.GetTemporaryRT(limitToId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RInt);
            _bufferLimitTo.SetRenderTarget(limitToId, BuiltinRenderTextureType.CameraTarget);
            _bufferLimitTo.ClearRenderTarget(false, true, Color.black);

            //if (temporaryMesh == null)
            //    temporaryMesh = new Mesh();

            // Loop over all game objects used for limiting decals
            foreach (GameObject go in _limitToGameObjects)
            {
                _bufferLimitTo.SetGlobalFloat("_ID", go.GetInstanceID());

                // Draw all mesh renderers...
                _limitToMeshRenderers.Clear();
                go.GetComponentsInChildren(_limitToMeshRenderers);
                foreach (MeshRenderer mr in _limitToMeshRenderers)
                {
                    // ...if they are not decals themselves...
                    // NOTE: We're using this trick because GetComponent() does some GC allocs
                    //       when the component is null (for some warning string or whatever).
                    _decalComponent.Clear();
                    mr.GetComponents(_decalComponent);
                    if (_decalComponent.Count == 0)
                    {
                        // ...and have a mesh filter
                        _meshFilterComponent.Clear();
                        mr.GetComponents(_meshFilterComponent);
                        if (_meshFilterComponent.Count == 1)
                        {
                            MeshFilter mf = _meshFilterComponent[0];
                            _bufferLimitTo.DrawMesh(mf.sharedMesh, mr.transform.localToWorldMatrix, _materialLimitToGameObjects);
                        }
                    }
                }

                _limitToSkinnedMeshRenderers.Clear();
                go.GetComponentsInChildren(_limitToSkinnedMeshRenderers);
                foreach (SkinnedMeshRenderer mr in _limitToSkinnedMeshRenderers)
                {
                    // TODO: Allow limiting to skinned meshes
                    //mr.BakeMesh(temporaryMesh);
                    //_bufferLimitTo.DrawMesh(temporaryMesh, mr.transform.localToWorldMatrix, _materialLimitToGameObjects);
                }
            }
        }

        private void DrawDeferredDecals_Albedo(Camera cam)
        {
            if (_deferredDecals.Count == 0)
                return;

            // Render first pass: albedo
            _bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

            var allDecalEnum = _deferredDecals.GetEnumerator();
            while (allDecalEnum.MoveNext())
            {
                Material material = allDecalEnum.Current.Key;
                HashSet<Decal> decals = allDecalEnum.Current.Value;
                int decalCount = decals.Count;
                int n = 0;

                var decalListEnum = decals.GetEnumerator();
                while (decalListEnum.MoveNext())
                {
                    Decal decal = decalListEnum.Current;
                    if (decal != null && decal.DrawAlbedo)
                    {
#if UNITY_5_5_OR_NEWER
                        if (UseInstancing)
                        {
                            _matrices[n] = decal.transform.localToWorldMatrix;
                            _fadeValues[n] = decal.Fade;
                            _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN;
                            ++n;

                            if (n == 1023)
                            {
                                _block.Clear();
                                _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                                _block.SetFloatArray("_LimitTo", _limitToValues);
                                _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _block);
                                n = 0;
                            }
                        }
                        else
#endif
                        {
                            // Fall back to non-instanced rendering
                            _block.Clear();
                            _block.SetFloat("_MaskMultiplier", decal.Fade);
                            _block.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN);
                            _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 0, _block);
                        }
                    }
                }

#if UNITY_5_5_OR_NEWER
                if (UseInstancing && n > 0)
                {
                    _block.Clear();
                    _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                    _block.SetFloatArray("_LimitTo", _limitToValues);
                    _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _block);
                }
#endif
            }
        }

        private void DrawDeferredDecals_NormSpecSmooth(Camera cam)
        {
            if (_deferredDecals.Count == 0)
                return;

            var copy1id = Shader.PropertyToID("_CameraGBufferTexture1Copy");
            _bufferDeferred.GetTemporaryRT(copy1id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            var copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");
            _bufferDeferred.GetTemporaryRT(copy2id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            // Render second pass: specular / smoothness and normals
            var allDecalEnum = _deferredDecals.GetEnumerator();
            while (allDecalEnum.MoveNext())
            {
                Material material = allDecalEnum.Current.Key;
                HashSet<Decal> decals = allDecalEnum.Current.Value;
                int n = 0;

                var decalListEnum = decals.GetEnumerator();
                while (decalListEnum.MoveNext())
                {
                    Decal decal = decalListEnum.Current;
                    if (decal != null && decal.DrawNormalAndGloss)
                    {
                        if (decal.HighQualityBlending)
                        {
                            // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                            _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
                            _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

                            _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);

                            _block.Clear();
                            _block.SetFloat("_MaskMultiplier", decal.Fade);
                            _block.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN);
                            _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 1, _block);
                        }
                        else
                        {
#if UNITY_5_5_OR_NEWER
                            if (UseInstancing)
                            {
                                // Instanced drawing
                                _matrices[n] = decal.transform.localToWorldMatrix;
                                _fadeValues[n] = decal.Fade;
                                _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN;
                                ++n;

                                if (n == 1023)
                                {
                                    // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
                                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

                                    _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
                                    _block.Clear();
                                    _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                                    _block.SetFloatArray("_LimitTo", _limitToValues);
                                    _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _block);
                                    n = 0;
                                }
                            }
                            else
#endif
                            {
                                if (n == 0)
                                {
                                    // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
                                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);
                                }

                                _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
                                _block.Clear();
                                _block.SetFloat("_MaskMultiplier", decal.Fade);
                                _block.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN);
                                _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 1, _block);
                                ++n;
                            }
                        }
                    }
                }

#if UNITY_5_5_OR_NEWER
                if (UseInstancing && n > 0)
                {
                    // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
                    _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

                    _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);

                    _block.Clear();
                    _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                    _block.SetFloatArray("_LimitTo", _limitToValues);
                    _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _block);
                }
#endif
            }
        }

        private void DrawUnlitDecals(Camera cam)
        {
            if (_unlitDecals.Count == 0)
                return;

            // Render third pass: unlit decals
            _bufferUnlit.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            var allDecalEnum = _unlitDecals.GetEnumerator();
            while (allDecalEnum.MoveNext())
            {
                Material material = allDecalEnum.Current.Key;
                HashSet<Decal> decals = allDecalEnum.Current.Value;
                int n = 0;

                var decalListEnum = decals.GetEnumerator();
                while (decalListEnum.MoveNext())
                {
                    Decal decal = decalListEnum.Current;
                    if (decal != null)
                    {
#if UNITY_5_5_OR_NEWER
                        if (UseInstancing)
                        {
                            _matrices[n] = decal.transform.localToWorldMatrix;
                            _fadeValues[n] = decal.Fade;
                            _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN;
                            ++n;

                            if (n == 1023)
                            {
                                _block.Clear();
                                _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                                _block.SetFloatArray("_LimitTo", _limitToValues);
                                _bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _block);
                                n = 0;
                            }
                        }
                        else
#endif
                        {
                            _block.Clear();
                            _block.SetFloat("_MaskMultiplier", decal.Fade);
                            _block.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN);
                            _bufferUnlit.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 0, _block);
                        }
                    }
                }

#if UNITY_5_5_OR_NEWER
                if (UseInstancing && n > 0)
                {
                    _block.Clear();
                    _block.SetFloatArray("_MaskMultiplier", _fadeValues);
                    _block.SetFloatArray("_LimitTo", _limitToValues);
                    _bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _block);
                }
#endif
            }
        }

        private static void CreateBuffer(ref CommandBuffer buffer, Camera cam, string name, CameraEvent evt)
        {
            if (buffer == null)
            {
                // See if the camera already has a command buffer to avoid duplicates
                foreach (CommandBuffer existingCommandBuffer in cam.GetCommandBuffers(evt))
                {
                    if (existingCommandBuffer.name == name)
                    {
                        buffer = existingCommandBuffer;
                        break;
                    }
                }

                // Not found? Create a new command buffer
                if (buffer == null)
                {
                    buffer = new CommandBuffer();
                    buffer.name = name;
                    cam.AddCommandBuffer(evt, buffer);
                }
            }
        }

        public void Add(Decal decal, GameObject limitTo)
        {
            if (limitTo)
            {
                _limitToGameObjects.Add(limitTo);
            }

            switch (decal.RenderMode)
            {
                case Decal.DecalRenderMode.Deferred:
                    AddDeferred(decal);
                    break;
                case Decal.DecalRenderMode.Unlit:
                    AddUnlit(decal);
                    break;
                default:
                    break;
            }
        }

        protected void AddDeferred(Decal decal)
        {
            if (!_deferredDecals.ContainsKey(decal.Material))
            {
                _deferredDecals.Add(decal.Material, new HashSet<Decal>() { decal });
            }
            else
            {
                _deferredDecals[decal.Material].Add(decal);
            }
        }

        protected void AddUnlit(Decal decal)
        {
            if (!_unlitDecals.ContainsKey(decal.Material))
            {
                _unlitDecals.Add(decal.Material, new HashSet<Decal>() { decal });
            }
            else
            {
                _unlitDecals[decal.Material].Add(decal);
            }
        }
    }
}
