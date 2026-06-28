using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CombatFeel
{
    public sealed class HitFlash
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly Color FrozenTint = new(0.55f, 0.85f, 1f, 1f);
        private static readonly Color FrozenEmission = new(0.15f, 0.45f, 0.95f, 1f);

        private readonly struct MaterialColors
        {
            public readonly bool HasBaseColor;
            public readonly Color BaseColor;
            public readonly bool HasColor;
            public readonly Color Color;
            public readonly bool HasEmission;
            public readonly Color Emission;

            public MaterialColors(Material mat)
            {
                HasBaseColor = mat.HasProperty(BaseColorId);
                BaseColor = HasBaseColor ? mat.GetColor(BaseColorId) : default;

                HasColor = mat.HasProperty(ColorId);
                Color = HasColor ? mat.GetColor(ColorId) : default;

                HasEmission = mat.HasProperty(EmissionColorId);
                Emission = HasEmission ? mat.GetColor(EmissionColorId) : default;
            }

            public void ApplyTo(Material mat)
            {
                if (HasBaseColor)
                    mat.SetColor(BaseColorId, BaseColor);
                if (HasColor)
                    mat.SetColor(ColorId, Color);
                if (HasEmission)
                    mat.SetColor(EmissionColorId, Emission);
            }

            public MaterialColors WithFrozenTint()
            {
                return new MaterialColors(
                    HasBaseColor, MultiplyColors(BaseColor, FrozenTint),
                    HasColor, MultiplyColors(Color, FrozenTint),
                    HasEmission, FrozenEmission);
            }

            private MaterialColors(
                bool hasBaseColor, Color baseColor,
                bool hasColor, Color color,
                bool hasEmission, Color emission)
            {
                HasBaseColor = hasBaseColor;
                BaseColor = baseColor;
                HasColor = hasColor;
                Color = color;
                HasEmission = hasEmission;
                Emission = emission;
            }
        }

        private readonly GameObject ownerObject;
        private readonly List<Renderer> renderers = new();
        private readonly List<MaterialColors[]> originalColors = new();

        private int flashSequenceId;
        private int freezeSequenceId;
        private bool isFrozen;

        public HitFlash(GameObject ownerObject)
        {
            this.ownerObject = ownerObject;
            RefreshRenderers();
        }

        /// <summary>
        /// Kích hoạt hiệu ứng đóng băng (tint xanh dương trong một khoảng thời gian)
        /// </summary>
        public async UniTaskVoid HitFrozen(float duration)
        {
            if (renderers.Count == 0 || ownerObject == null) return;

            int currentFreeze = ++freezeSequenceId;
            isFrozen = true;
            ApplyFrozenTint();

            await UniTask.Delay(TimeSpan.FromSeconds(duration), delayTiming: PlayerLoopTiming.Update);

            if (ownerObject == null || currentFreeze != freezeSequenceId) return;

            isFrozen = false;
            ResetToOriginalColors();
        }

        /// <summary>
        /// Kích hoạt hiệu ứng nháy màu bất đồng bộ
        /// </summary>
        public async UniTaskVoid Play(Color flashColor, float duration)
        {
            if (renderers.Count == 0 || ownerObject == null) return;

            int currentFlash = ++flashSequenceId;
            ApplyAbsoluteColor(flashColor);

            await UniTask.Delay(TimeSpan.FromSeconds(duration), delayTiming: PlayerLoopTiming.Update);

            if (ownerObject == null || currentFlash != flashSequenceId) return;

            if (isFrozen)
                ApplyFrozenTint();
            else
                ResetToOriginalColors();
        }

        /// <summary>
        /// Đưa màu sắc về lại màu gốc và hủy trạng thái đóng băng.
        /// </summary>
        public void ResetToOriginalColors()
        {
            isFrozen = false;
            freezeSequenceId++;

            for (int r = 0; r < renderers.Count; r++)
            {
                if (renderers[r] == null) continue;
                Material[] mats = renderers[r].materials;
                for (int m = 0; m < mats.Length; m++)
                    originalColors[r][m].ApplyTo(mats[m]);
            }
        }

        /// <summary>
        /// Quét lại các Renderer sau khi spawn từ pool hoặc đổi model.
        /// </summary>
        public void RefreshRenderers()
        {
            if (ownerObject == null) return;

            renderers.Clear();
            originalColors.Clear();

            ownerObject.GetComponentsInChildren<Renderer>(true, renderers);

            foreach (var renderer in renderers)
            {
                Material[] mats = renderer.materials;
                var colors = new MaterialColors[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                    colors[i] = new MaterialColors(mats[i]);
                originalColors.Add(colors);
            }
        }

        private void ApplyFrozenTint()
        {
            for (int r = 0; r < renderers.Count; r++)
            {
                if (renderers[r] == null) continue;
                Material[] mats = renderers[r].materials;
                for (int m = 0; m < mats.Length; m++)
                    originalColors[r][m].WithFrozenTint().ApplyTo(mats[m]);
            }
        }

        private void ApplyAbsoluteColor(Color color)
        {
            for (int r = 0; r < renderers.Count; r++)
            {
                if (renderers[r] == null) continue;
                Material[] mats = renderers[r].materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m].HasProperty(BaseColorId))
                        mats[m].SetColor(BaseColorId, color);
                    else if (mats[m].HasProperty(ColorId))
                        mats[m].SetColor(ColorId, color);
                }
            }
        }

        private static Color MultiplyColors(Color a, Color b) =>
            new(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }
}
