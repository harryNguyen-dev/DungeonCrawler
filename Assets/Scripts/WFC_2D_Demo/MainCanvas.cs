using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WFC;

namespace WFC_2D_Demo
{
    public class MainCanvas : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text generateInfoText;
        [SerializeField] private TMP_InputField seedInputField;
        [SerializeField] private Button WFC_PureBtn;
        [SerializeField] private Button WFC_CustomBtn;
        [SerializeField] private Button ResetBtn;
        [SerializeField] private RectTransform gridParent;

        [Header("WFC — cùng component WFCGeneration như Battle Scene")]
        [SerializeField] private WFCGeneration wfcGeneration;
        [SerializeField] private int maxRetryAttempts = 5;
        [SerializeField] private bool spawn3DForComparison = true;

        [Header("Grid Render")]
        [SerializeField] private Color emptyColor = Color.black;
        [SerializeField] private Color roomFallbackColor = new Color(0.30f, 0.70f, 1f, 1f);
        [SerializeField] private Color corridorFallbackColor = new Color(0.35f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color primLineColor = new Color(1f, 0.35f, 0.2f, 0.95f);
        [SerializeField] private Color primActiveLineColor = new Color(1f, 0.92f, 0.2f, 1f);
        [SerializeField] private float primLineThickness = 4f;
        [SerializeField] private float cellPixelSize = 64f;
        [SerializeField] private bool autoSizeCellsToViewport = true;

        private WFCCustom customGenerator;
        private WFCPure pureGenerator;
        private bool isGenerating;

        private void Awake()
        {
            if (wfcGeneration == null)
                wfcGeneration = GetComponent<WFCGeneration>();

            if (wfcGeneration != null)
            {
                customGenerator = new WFCCustom(
                    wfcGeneration,
                    gridParent,
                    emptyColor,
                    roomFallbackColor,
                    corridorFallbackColor,
                    primLineColor,
                    primActiveLineColor,
                    primLineThickness,
                    cellPixelSize,
                    autoSizeCellsToViewport);

                pureGenerator = new WFCPure(
                    wfcGeneration,
                    gridParent,
                    emptyColor,
                    roomFallbackColor,
                    corridorFallbackColor,
                    primLineColor,
                    primActiveLineColor,
                    primLineThickness,
                    cellPixelSize,
                    autoSizeCellsToViewport);
            }
        }

        private void OnEnable()
        {
            if (WFC_CustomBtn != null)
                WFC_CustomBtn.onClick.AddListener(OnCustomGenerateClicked);
            if (WFC_PureBtn != null)
                WFC_PureBtn.onClick.AddListener(OnPureGenerateClicked);
            if (ResetBtn != null)
                ResetBtn.onClick.AddListener(OnResetClicked);
        }

        private void OnDisable()
        {
            if (WFC_CustomBtn != null)
                WFC_CustomBtn.onClick.RemoveListener(OnCustomGenerateClicked);
            if (WFC_PureBtn != null)
                WFC_PureBtn.onClick.RemoveListener(OnPureGenerateClicked);
            if (ResetBtn != null)
                ResetBtn.onClick.RemoveListener(OnResetClicked);
        }

        private void OnCustomGenerateClicked()
        {
            if (isGenerating) return;
            RunCustomGenerate().Forget();
        }

        private void OnPureGenerateClicked()
        {
            if (isGenerating) return;
            RunPureGenerate().Forget();
        }

        private void OnResetClicked()
        {
            customGenerator?.Clear();
            pureGenerator?.Clear();
            if (generateInfoText != null)
                generateInfoText.text = string.Empty;
            if (seedInputField != null)
                seedInputField.text = string.Empty;
        }

        private async UniTask RunCustomGenerate()
        {
            if (customGenerator == null || wfcGeneration == null)
            {
                SetInfoText("Thiếu WFCGeneration — gán component giống Battle Scene.");
                return;
            }

            isGenerating = true;
            SetButtonsInteractable(false);

            try
            {
                int seed = ResolveSeed();
                if (seedInputField != null && string.IsNullOrWhiteSpace(seedInputField.text))
                    seedInputField.SetTextWithoutNotify(seed.ToString());

                WFCCustomResult result = await customGenerator.GenerateAndRender(
                    seed,
                    maxRetryAttempts,
                    spawnPrefabs: spawn3DForComparison);

                SetInfoText(FormatCustomInfo(result, spawn3DForComparison));
            }
            finally
            {
                isGenerating = false;
                SetButtonsInteractable(true);
            }
        }

        private async UniTask RunPureGenerate()
        {
            if (pureGenerator == null || wfcGeneration == null)
            {
                SetInfoText("Thiếu WFCGeneration — gán component giống Battle Scene.");
                return;
            }

            if (wfcGeneration.AllTiles == null || wfcGeneration.AllTiles.Length == 0)
            {
                SetInfoText("Thiếu allTiles — gán WFCData trong WFCGeneration.");
                return;
            }

            isGenerating = true;
            SetButtonsInteractable(false);

            try
            {
                int seed = ResolveSeed();
                if (seedInputField != null && string.IsNullOrWhiteSpace(seedInputField.text))
                    seedInputField.SetTextWithoutNotify(seed.ToString());

                WFCPureResult result = await pureGenerator.GenerateAndRender(seed, maxRetryAttempts);
                SetInfoText(FormatPureInfo(result));
            }
            finally
            {
                isGenerating = false;
                SetButtonsInteractable(true);
            }
        }

        private int ResolveSeed()
        {
            if (seedInputField == null || string.IsNullOrWhiteSpace(seedInputField.text))
                return Random.Range(0, int.MaxValue);

            string text = seedInputField.text.Trim();
            if (int.TryParse(text, out int parsed))
                return parsed;

            return Random.Range(0, int.MaxValue);
        }

        private static string FormatPureInfo(WFCPureResult result)
        {
            GenerationStats s = result.Stats;
            string status = result.Success ? "OK" : "FAIL";
            return
                $"WFC Pure [{status}]\n" +
                $"Seed: {s.seed} | Retries: {result.Attempts}\n" +
                $"WFC iter: {s.wfc_iterations} | Contradictions: {s.contradictions}\n" +
                $"Rooms: {s.room_cells_count} | Corridors: {s.corridor_cells_count} | Empty: {s.empty_cells_count}\n" +
                $"Density: {s.dungeon_density:P1} | Dead ends: {s.dead_end_count} | Branches: {s.branch_count}\n" +
                $"Time: {s.ms_total:F1}ms (WFC: {s.ms_wfc_fill:F1})";
        }

        private static string FormatCustomInfo(WFCCustomResult result, bool spawn3DForComparison)
        {
            GenerationStats s = result.Stats;
            string status = result.Success ? "OK" : "FAIL";
            string mode = spawn3DForComparison ? "3D+2D" : "2D only";
            return
                $"WFC Custom [{status}]\n" +
                $"Mode: {mode}\n" +
                $"Seed: {s.seed} | Retries: {result.Attempts}\n" +
                $"Rooms: {s.rooms_placed}/{s.rooms_target}\n" +
                $"MST: {s.mst_edges_success}/{s.mst_edges_total} | Extra: {s.extra_edges_success}/{s.extra_edges_total}\n" +
                $"A* path avg: {s.astar_path_avg_length:F1} (min {s.astar_path_min_length}, max {s.astar_path_max_length})\n" +
                $"WFC iter: {s.wfc_iterations} | Contradictions: {s.contradictions}\n" +
                $"Density: {s.dungeon_density:P1} | Dead ends: {s.dead_end_count} | Branches: {s.branch_count}\n" +
                $"Time: {s.ms_total:F1}ms (R:{s.ms_place_rooms:F1} C:{s.ms_connect_corridors:F1} W:{s.ms_wfc_fill:F1})";
        }

        private void SetInfoText(string text)
        {
            if (generateInfoText != null)
                generateInfoText.text = text;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (WFC_CustomBtn != null) WFC_CustomBtn.interactable = interactable;
            if (WFC_PureBtn != null) WFC_PureBtn.interactable = interactable;
            if (ResetBtn != null) ResetBtn.interactable = interactable;
        }
    }
}
