using _Project.Scripts.Abilities;
using _Project.Scripts.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Editor-only: MOBA > Create Player HUD builds a working HUD in the open scene
// (4 ability squares with the health bar and resource bar below them, bottom centre).
// Running it again replaces the HUD it made before. Restyle it freely afterwards.
namespace _Project.Scripts.EditorTools
{
    public static class PlayerHUDBuilder
    {
        private const float SlotSize = 80f;
        private const float SlotSpacing = 10f;
        private const float BarSpacing = 6f;
        private const float HealthBarHeight = 24f;
        private const float ResourceBarHeight = 16f;
        private const float BottomMargin = 24f;

        private static readonly AbilitySlot[] Slots =
        {
            AbilitySlot.Ability1, AbilitySlot.Ability2, AbilitySlot.Ability3, AbilitySlot.Ultimate
        };

        private static readonly Color PanelColor = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        private static readonly Color CooldownColor = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color HealthColor = new Color(0.25f, 0.8f, 0.3f);
        private static readonly Color ResourceColor = new Color(0.25f, 0.55f, 1f); // replaced by the resource's own colour at runtime
        private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.2f);

        [MenuItem("MOBA/Create Player HUD")]
        private static void Create()
        {
            // TextMeshPro needs its default font, which comes with the "Essential Resources"
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog("Player HUD",
                    "TextMeshPro's essential resources are not in the project yet.\n\n" +
                    "Import them with Window > TextMeshPro > Import TMP Essential Resources, then run this again.",
                    "OK");
                return;
            }

            // Replace an older HUD instead of stacking a second one on top
            PlayerHUD existing = Object.FindAnyObjectByType<PlayerHUD>(FindObjectsInactive.Include);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Player HUD",
                        $"The scene already has a HUD (\"{existing.name}\"). Replace it?", "Replace", "Cancel"))
                {
                    return;
                }

                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            // The built-in rounded sprite Unity's own UI menu uses. Filled images need a sprite to sweep.
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // Canvas
            var canvasObject = new GameObject("PlayerHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasObject.AddComponent<PlayerHUD>();

            // Bottom-centre column: squares, then health, then resource. It grows upwards from the
            // bottom edge, and a hidden resource bar takes no space (the layout skips inactive children).
            float width = Slots.Length * SlotSize + (Slots.Length - 1) * SlotSpacing;

            RectTransform group = CreateRect("Bottom", canvasObject.transform);
            group.anchorMin = group.anchorMax = new Vector2(0.5f, 0f);
            group.pivot = new Vector2(0.5f, 0f);
            group.sizeDelta = new Vector2(width, 0f);
            group.anchoredPosition = new Vector2(0f, BottomMargin);

            var column = group.gameObject.AddComponent<VerticalLayoutGroup>();
            column.spacing = BarSpacing;
            column.childAlignment = TextAnchor.LowerCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            var fitter = group.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Row of ability squares
            RectTransform row = CreateRect("Abilities", group);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = SlotSize;

            var slotViews = new AbilitySlotUI[Slots.Length];
            for (int i = 0; i < Slots.Length; i++)
            {
                slotViews[i] = CreateSlot(row, Slots[i], i, uiSprite);
            }

            // Bars underneath
            HealthBarUI healthBar = CreateHealthBar(group, uiSprite);
            ResourceBarUI resourceBar = CreateResourceBar(group, uiSprite);

            // Hook everything up on the PlayerHUD (its fields are private, so through SerializedObject)
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("healthBar").objectReferenceValue = healthBar;
            serializedHud.FindProperty("resourceBar").objectReferenceValue = resourceBar;

            SerializedProperty slotsProperty = serializedHud.FindProperty("slots");
            slotsProperty.arraySize = Slots.Length;
            for (int i = 0; i < Slots.Length; i++)
            {
                SerializedProperty element = slotsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("slot").enumValueIndex = (int)Slots[i];
                element.FindPropertyRelative("view").objectReferenceValue = slotViews[i];
            }

            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Player HUD");
            Selection.activeGameObject = canvasObject;
        }

        private static AbilitySlotUI CreateSlot(RectTransform parent, AbilitySlot slot, int index, Sprite uiSprite)
        {
            RectTransform root = CreateRect(slot.ToString(), parent);
            root.anchorMin = root.anchorMax = root.pivot = Vector2.zero;
            root.sizeDelta = new Vector2(SlotSize, SlotSize);
            root.anchoredPosition = new Vector2(index * (SlotSize + SlotSpacing), 0f);

            // Square (no sprite) so the cooldown sweep lines up with its corners
            Image background = root.gameObject.AddComponent<Image>();
            background.color = PanelColor;

            // Icon, inset a little so the background shows as a frame
            Image icon = CreateImage("Icon", root, Color.white, null);
            Stretch(icon.rectTransform, 4f);
            icon.preserveAspect = true;

            // Cooldown sweep over the whole square, edge to edge. No sprite on purpose: AbilitySlotUI gives it
            // a plain square at runtime (Unity's default UI sprite has rounded, see-through corners).
            Image cooldown = CreateImage("Cooldown", root, CooldownColor, null);
            Stretch(cooldown.rectTransform, 0f);
            cooldown.enabled = false; // without a sprite it would show as a full dark square in the editor
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Radial360;
            cooldown.fillOrigin = (int)Image.Origin360.Top;
            cooldown.fillClockwise = false;
            cooldown.fillAmount = 0f;

            // Border shown while readied / toggled on
            GameObject highlight = CreateBorder(root, HighlightColor, 3f);
            highlight.SetActive(false);

            TMP_Text cooldownText = CreateText("CooldownText", root, 32f, TextAlignmentOptions.Center);
            cooldownText.fontStyle = FontStyles.Bold;
            Stretch(cooldownText.rectTransform, 0f);

            TMP_Text keyText = CreateText("KeyText", root, 16f, TextAlignmentOptions.BottomRight);
            keyText.fontStyle = FontStyles.Bold;
            Stretch(keyText.rectTransform, 6f);
            keyText.text = string.Empty;

            TMP_Text costText = CreateText("CostText", root, 16f, TextAlignmentOptions.TopLeft);
            costText.fontStyle = FontStyles.Bold;
            Stretch(costText.rectTransform, 6f);

            var view = root.gameObject.AddComponent<AbilitySlotUI>();
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("icon").objectReferenceValue = icon;
            serializedView.FindProperty("cooldownOverlay").objectReferenceValue = cooldown;
            serializedView.FindProperty("cooldownText").objectReferenceValue = cooldownText;
            serializedView.FindProperty("keyText").objectReferenceValue = keyText;
            serializedView.FindProperty("costText").objectReferenceValue = costText;
            serializedView.FindProperty("activeHighlight").objectReferenceValue = highlight;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static HealthBarUI CreateHealthBar(RectTransform parent, Sprite uiSprite)
        {
            RectTransform root = CreateBar("HealthBar", parent, HealthBarHeight, HealthColor, 16f, uiSprite,
                out Image fill, out TMP_Text label);

            var bar = root.gameObject.AddComponent<HealthBarUI>();
            var serializedBar = new SerializedObject(bar);
            serializedBar.FindProperty("fill").objectReferenceValue = fill.rectTransform;
            serializedBar.FindProperty("label").objectReferenceValue = label;
            serializedBar.ApplyModifiedPropertiesWithoutUndo();

            return bar;
        }

        private static ResourceBarUI CreateResourceBar(RectTransform parent, Sprite uiSprite)
        {
            RectTransform root = CreateBar("ResourceBar", parent, ResourceBarHeight, ResourceColor, 12f, uiSprite,
                out Image fill, out TMP_Text label);

            // The script goes on the always-active column, because it hides the bar itself
            var bar = parent.gameObject.AddComponent<ResourceBarUI>();
            var serializedBar = new SerializedObject(bar);
            serializedBar.FindProperty("bar").objectReferenceValue = root.gameObject;
            serializedBar.FindProperty("fill").objectReferenceValue = fill.rectTransform;
            serializedBar.FindProperty("fillGraphic").objectReferenceValue = fill;
            serializedBar.FindProperty("label").objectReferenceValue = label;
            serializedBar.ApplyModifiedPropertiesWithoutUndo();

            return bar;
        }

        // A dark background with a coloured fill and a centred label, sized by the column layout
        private static RectTransform CreateBar(string name, RectTransform parent, float height, Color fillColor,
            float fontSize, Sprite uiSprite, out Image fill, out TMP_Text label)
        {
            RectTransform root = CreateRect(name, parent);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = height;

            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = uiSprite;
            background.type = Image.Type.Sliced;
            background.color = PanelColor;
            background.raycastTarget = false;

            // The fill sits in an inset area so it keeps a small frame around it
            RectTransform fillArea = CreateRect("FillArea", root);
            Stretch(fillArea, 3f);

            fill = CreateImage("Fill", fillArea, fillColor, null);
            Stretch(fill.rectTransform, 0f);

            label = CreateText("Label", root, fontSize, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            Stretch(label.rectTransform, 0f);

            return root;
        }

        private static GameObject CreateBorder(RectTransform parent, Color color, float thickness)
        {
            RectTransform root = CreateRect("ActiveHighlight", parent);
            Stretch(root, 0f);

            // Four thin images: bottom, top, left, right
            CreateEdge(root, color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness));
            CreateEdge(root, color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness));
            CreateEdge(root, color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f));
            CreateEdge(root, color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f));

            return root.gameObject;
        }

        private static void CreateEdge(RectTransform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            Image edge = CreateImage("Edge", parent, color, null);
            edge.rectTransform.anchorMin = anchorMin;
            edge.rectTransform.anchorMax = anchorMax;
            edge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            edge.rectTransform.sizeDelta = size;
            edge.rectTransform.anchoredPosition = Vector2.zero;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Sprite sprite)
        {
            Image image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
        {
            var text = CreateRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.text = string.Empty;
            return text;
        }

        // Fill the parent, leaving `inset` pixels on every side
        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
