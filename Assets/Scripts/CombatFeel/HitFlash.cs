using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CombatFeel
{
    public sealed class HitFlash
    {
        private readonly GameObject ownerObject;
        private readonly List<Renderer> renderers = new();
        private readonly List<Color[]> originalColors = new();
        private int sequenceId;

        public HitFlash(GameObject ownerObject)
        {
            this.ownerObject = ownerObject;
            RefreshRenderers();
        }

        /// <summary>
        /// Hàm này giúp quét lại các Renderer. 
        /// Sau này nếu bạn đổi model hoặc bật/tắt các bộ phận của quái thì gọi hàm này.
        /// </summary>
        public void RefreshRenderers()
        {
            if (ownerObject == null) return;

            renderers.Clear();
            originalColors.Clear();

            // Quét tất cả MeshRenderer (Cube hiện tại) và SkinnedMeshRenderer (Model xịn sau này)
            ownerObject.GetComponentsInChildren<Renderer>(true, renderers);

            // Lưu lại màu gốc của từng Material trên từng Renderer
            foreach (var renderer in renderers)
            {
                Material[] mats = renderer.materials;
                Color[] colors = new Color[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i].HasProperty("_Color"))
                    {
                        colors[i] = mats[i].color;
                    }
                }
                originalColors.Add(colors);
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng nháy màu bất đồng bộ
        /// </summary>
        public async UniTaskVoid Play(Color flashColor, float duration)
        {
            if (renderers.Count == 0 || ownerObject == null) return;

            // Sử dụng sequenceId để chống dẫm chân nhau khi bị bắn liên tục
            int currentSequence = ++sequenceId;

            // Bước 1: Đổi toàn bộ sang màu Flash
            for (int r = 0; r < renderers.Count; r++)
            {
                if (renderers[r] == null) continue;
                Material[] mats = renderers[r].materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m].HasProperty("_Color"))
                    {
                        mats[m].color = flashColor;
                    }
                }
            }

            // Bước 2: Chờ một khoảng thời gian siêu ngắn (Ví dụ: 0.1 giây)
            await UniTask.Delay(TimeSpan.FromSeconds(duration), delayTiming: PlayerLoopTiming.Update);

            // Kiểm tra xem quái có bị hủy hoặc có phát bắn mới đè lên không
            if (ownerObject == null || currentSequence != sequenceId) return;

            // Bước 3: Trả lại màu gốc ban đầu
            for (int r = 0; r < renderers.Count; r++)
            {
                if (renderers[r] == null) continue;
                Material[] mats = renderers[r].materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m].HasProperty("_Color"))
                    {
                        mats[m].color = originalColors[r][m];
                    }
                }
            }
        }
    }
}