using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Projectile
{

    public class ProjectileMove : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 2f;
        private bool canMove = false;

        // --- Biến bổ sung cho Boomerang ---
        private bool isBoomerang = false;
        private bool isReturning = false;
        private Transform playerTransform;
        private System.Action onReturnStart; // Callback báo cho Controller biết đạn đang quay về
        private CancellationTokenSource disableCTS;
        public void ActiveSelf(bool isBoomerang, System.Action onReturnStart)
        {
            CancelInvoke(); // Xóa sạch các lệnh Invoke cũ nếu có
            this.isBoomerang = isBoomerang;
            this.onReturnStart = onReturnStart;
            this.isReturning = false;
            this.canMove = true;

            if (isBoomerang)
            {
                // Lấy vị trí Player thông qua Local Server GlobalEntities của bạn
                playerTransform = Global.GlobalEntities.Instance.PlayerStats.transform;

                // Thay vì Destroy ngay, sau một nửa thời gian nó sẽ tự kích hoạt chế độ bay về
                Invoke(nameof(StartReturnState), lifeTime * 0.5f);
            }
            else
            {
                // Đạn thường thì vẫn hủy sau thời gian lifeTime
                // Destroy(gameObject, lifeTime);
                ReturnAfterTime(disableCTS.Token).Forget();
            }
        }
        private async UniTask ReturnAfterTime(CancellationToken cancellationToken)
        {
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (isCanceled) return;

            if (gameObject.activeSelf && Core.ObjectPoolingManager.Instance != null)
            {
                Core.ObjectPoolingManager.Instance.Return(gameObject);
            }
        }

        public void StartReturnState()
        {
            if (isReturning || !isBoomerang) return;

            CancelInvoke(nameof(StartReturnState)); // Hủy lệnh invoke nếu được gọi ép buộc từ bên ngoài
            isReturning = true;

            // Kích hoạt callback để xóa list đã trúng độc bên Controller
            onReturnStart?.Invoke();
        }

        private void Update()
        {
            if (!canMove) return;

            if (isReturning && playerTransform != null)
            {
                // GIAI ĐOẠN 2: Hướng về phía Player như nam châm
                Vector3 direction = (playerTransform.position - transform.position).normalized;

                // Quay đầu viên đạn về phía Player nhìn cho đẹp mắt
                if (direction != Vector3.zero)
                {
                    transform.forward = direction;
                }

                transform.Translate(Vector3.forward * speed * 1.5f * Time.deltaTime);

                // Nếu quay về sát Player, tiến hành hủy đạn
                if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
                {
                    // Destroy(gameObject);
                    Core.ObjectPoolingManager.Instance.Return(gameObject);
                }
            }
            else
            {
                // GIAI ĐOẠN 1: Di chuyển về phía trước theo hướng mặc định
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }

        public void OnSpawnedFromPool()
        {
            // Đảm bảo CTS luôn luôn được tạo TRƯỚC khi bất kỳ logic nào khác chạy
            disableCTS = new CancellationTokenSource();
        }

        public void OnReturnedToPool()
        {
            CancelInvoke();
            canMove = false;
            isReturning = false;

            // Dọn dẹp tiến trình ngầm an toàn
            if (disableCTS != null)
            {
                disableCTS.Cancel();
                disableCTS.Dispose();
                disableCTS = null;
            }
        }
    }

}