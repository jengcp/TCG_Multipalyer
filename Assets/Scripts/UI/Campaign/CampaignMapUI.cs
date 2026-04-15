using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Campaign;
using TCG.Core;

namespace TCG.UI.Campaign
{
    /// <summary>
    /// Renders the campaign hex grid map for one chapter at a time.
    ///
    /// Hex grid math (flat-top orientation):
    ///   xSpacing = hexSize * 1.5
    ///   ySpacing = hexSize * sqrt(3)
    ///   Odd columns are offset +ySpacing/2 upward.
    ///
    /// Assign <see cref="campaignData"/>, <see cref="stageNodePrefab"/>, and the container
    /// transforms in the Inspector. Chapter navigation arrows allow switching chapters.
    /// </summary>
    public class CampaignMapUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CampaignData campaignData;

        [Header("Prefabs & Containers")]
        [SerializeField] private HexStageNodeUI stageNodePrefab;
        [SerializeField] private RectTransform  hexGridContent;   // ScrollRect content panel
        [SerializeField] private StageDetailPanelUI detailPanel;

        [Header("Hex Grid Settings")]
        [Tooltip("Circumradius of each hex node in canvas units (pixels for 1:1 canvas scale).")]
        [SerializeField] private float hexSize = 80f;

        [Header("Connection Lines")]
        [SerializeField] private RectTransform lineContainer;     // parent for connection line images
        [SerializeField] private Image         linePrefab;

        [Header("Chapter Navigation")]
        [SerializeField] private TMP_Text  chapterNameText;
        [SerializeField] private Button    prevChapterButton;
        [SerializeField] private Button    nextChapterButton;

        // ── Runtime ────────────────────────────────────────────────────────────────

        private int                         _currentChapterIndex;
        private readonly List<HexStageNodeUI> _nodePool = new();
        private readonly List<Image>           _linePool = new();

        // Pixel offsets derived from hexSize
        private float XSpacing => hexSize * 1.5f;
        private float YSpacing => hexSize * Mathf.Sqrt(3f);

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            prevChapterButton?.onClick.AddListener(PrevChapter);
            nextChapterButton?.onClick.AddListener(NextChapter);
        }

        private void OnEnable()
        {
            GameEvents.OnCampaignStageCompleted += OnStageCompleted;
            GameEvents.OnCampaignStageUnlocked  += OnStageUnlocked;
            RebuildMap();
        }

        private void OnDisable()
        {
            GameEvents.OnCampaignStageCompleted -= OnStageCompleted;
            GameEvents.OnCampaignStageUnlocked  -= OnStageUnlocked;
        }

        // ── Chapter navigation ─────────────────────────────────────────────────────

        private void PrevChapter()
        {
            if (campaignData == null || _currentChapterIndex <= 0) return;
            _currentChapterIndex--;
            RebuildMap();
        }

        private void NextChapter()
        {
            if (campaignData == null || _currentChapterIndex >= campaignData.chapters.Count - 1) return;
            _currentChapterIndex++;
            RebuildMap();
        }

        // ── Map building ───────────────────────────────────────────────────────────

        private void RebuildMap()
        {
            if (campaignData == null || campaignData.chapters.Count == 0) return;

            _currentChapterIndex = Mathf.Clamp(_currentChapterIndex, 0, campaignData.chapters.Count - 1);
            var chapter = campaignData.chapters[_currentChapterIndex];

            // Update chapter header
            if (chapterNameText != null)
                chapterNameText.text = chapter.chapterName;

            // Update nav button interactability
            if (prevChapterButton != null) prevChapterButton.interactable = _currentChapterIndex > 0;
            if (nextChapterButton != null) nextChapterButton.interactable = _currentChapterIndex < campaignData.chapters.Count - 1;

            // Set chapter background
            var bg = GetComponentInParent<Image>();
            if (bg != null && chapter.chapterBackground != null)
                bg.sprite = chapter.chapterBackground;

            HideAllNodes();
            HideAllLines();

            var stages = chapter.stages;

            // Map stageId → node for drawing connections
            var stageToNode = new Dictionary<string, HexStageNodeUI>();

            for (int i = 0; i < stages.Count; i++)
            {
                var node = GetOrCreateNode(i);
                var stage = stages[i];

                node.Bind(stage, detailPanel);
                node.gameObject.SetActive(true);

                // Position
                var pos = HexToPixel(stage.gridColumn, stage.gridRow);
                node.GetComponent<RectTransform>().anchoredPosition = pos;

                stageToNode[stage.stageId] = node;
            }

            // Draw connection lines from each stage to its prerequisites
            int lineIdx = 0;
            foreach (var stage in stages)
            {
                if (stage.prerequisites == null) continue;
                foreach (var prereq in stage.prerequisites)
                {
                    if (prereq == null) continue;
                    if (!stageToNode.TryGetValue(stage.stageId,  out var toNode))   continue;
                    if (!stageToNode.TryGetValue(prereq.stageId, out var fromNode)) continue;

                    var line = GetOrCreateLine(lineIdx++);
                    DrawLine(line, fromNode.GetComponent<RectTransform>().anchoredPosition,
                                   toNode.GetComponent<RectTransform>().anchoredPosition);
                    line.gameObject.SetActive(true);
                }
            }
        }

        // ── Hex math ───────────────────────────────────────────────────────────────

        private Vector2 HexToPixel(int col, int row)
        {
            float x = col * XSpacing;
            float y = row * YSpacing + (col % 2 == 1 ? YSpacing * 0.5f : 0f);
            return new Vector2(x, y);
        }

        // ── Line drawing ───────────────────────────────────────────────────────────

        private void DrawLine(Image line, Vector2 from, Vector2 to)
        {
            if (line == null) return;

            var rt = line.GetComponent<RectTransform>();
            Vector2 dir = to - from;
            float dist  = dir.magnitude;

            rt.anchoredPosition = from + dir * 0.5f;
            rt.sizeDelta        = new Vector2(dist, 6f);
            rt.localRotation    = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        // ── Pooling ────────────────────────────────────────────────────────────────

        private HexStageNodeUI GetOrCreateNode(int index)
        {
            while (_nodePool.Count <= index)
            {
                var n = Instantiate(stageNodePrefab, hexGridContent);
                n.gameObject.SetActive(false);
                _nodePool.Add(n);
            }
            return _nodePool[index];
        }

        private Image GetOrCreateLine(int index)
        {
            while (_linePool.Count <= index)
            {
                var l = Instantiate(linePrefab, lineContainer != null ? lineContainer : hexGridContent);
                l.gameObject.SetActive(false);
                _linePool.Add(l);
            }
            return _linePool[index];
        }

        private void HideAllNodes()  { foreach (var n in _nodePool) n.gameObject.SetActive(false); }
        private void HideAllLines()  { foreach (var l in _linePool) l.gameObject.SetActive(false); }

        // ── Event handlers ─────────────────────────────────────────────────────────

        private void OnStageCompleted(CampaignStageResult result)
        {
            foreach (var node in _nodePool)
                if (node.gameObject.activeSelf) node.Refresh();
        }

        private void OnStageUnlocked(string stageId)
        {
            foreach (var node in _nodePool)
                if (node.gameObject.activeSelf) node.Refresh();
        }
    }
}
