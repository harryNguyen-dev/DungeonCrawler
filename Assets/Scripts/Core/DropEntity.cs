using DG.Tweening;
using UnityEngine;

namespace Core
{
    public enum DropType
    {
        Gold,
        Exp
    }

    public class DropEntity : MonoBehaviour, IPoolable
    {
        [Header("Motion")]
        [SerializeField] private float popHeight = 0.45f;
        [SerializeField] private float popDuration = 0.25f;
        [SerializeField] private float collectDelay = 0.35f;
        [SerializeField] private float collectDuration = 0.45f;
        [SerializeField] private float playerHeightOffset = 0.75f;

        private DropType _type;
        private int _value;
        private bool _isCollecting;
        private Sequence _sequence;
        private Tween _collectTween;
        private Vector3 _spawnPosition;

        public DropType Type => _type;
        public int Value => _value;

        public void Initialize(DropType type, int value, Vector3 worldPosition)
        {
            _type = type;
            _value = Mathf.Max(0, value);
            _spawnPosition = worldPosition;
            transform.position = worldPosition;
            BeginDropSequence();
        }

        public void OnSpawnedFromPool()
        {
            _isCollecting = false;
            KillTweens();
        }

        public void OnReturnedToPool()
        {
            _isCollecting = false;
            _value = 0;
            KillTweens();
        }

        private void OnDisable() => KillTweens();

        private void BeginDropSequence()
        {
            KillTweens();

            if (_value <= 0)
            {
                DropPool.Instance?.Return(this);
                return;
            }

            var popTarget = _spawnPosition + Vector3.up * popHeight;
            _sequence = DOTween.Sequence()
                .Append(transform.DOMove(popTarget, popDuration).SetEase(Ease.OutQuad))
                .Append(transform.DOMove(_spawnPosition, popDuration * 0.6f).SetEase(Ease.InQuad))
                .AppendInterval(collectDelay)
                .AppendCallback(StartCollectTween)
                .SetLink(gameObject);
        }

        private void StartCollectTween()
        {
            if (_isCollecting) return;
            _isCollecting = true;

            var start = transform.position;
            _collectTween = DOVirtual.Float(0f, 1f, collectDuration, t =>
                {
                    var player = GetPlayerTransform();
                    if (player == null) return;

                    var target = player.position + Vector3.up * playerHeightOffset;
                    transform.position = Vector3.Lerp(start, target, t);
                })
                .SetEase(Ease.InQuad)
                .OnComplete(OnCollected)
                .SetLink(gameObject);
        }

        private void OnCollected()
        {
            if (_value <= 0)
            {
                DropPool.Instance?.Return(this);
                return;
            }

            switch (_type)
            {
                case DropType.Gold:
                    GameManager.Instance?.CollectGold(_value);
                    break;
                case DropType.Exp:
                    Global.GlobalEntities.Instance?.PlayerStats?.CollectExp(_value);
                    break;
            }

            DropPool.Instance?.Return(this);
        }

        private static Transform GetPlayerTransform()
        {
            return Global.GlobalEntities.Instance?.PlayerInstance?.transform;
        }

        private void KillTweens()
        {
            _sequence?.Kill();
            _sequence = null;
            _collectTween?.Kill();
            _collectTween = null;
        }
    }
}
