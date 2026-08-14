using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class InteractionPointViewTests
{
    [UnityTest]
    public IEnumerator HoverAndFocusShareTooltipVisibility()
    {
        using var rig = new TestRig();
        InteractionDefinition definition = rig.CreateDefinition(
            "INT_TEST_FOCUS",
            "Test interaction",
            new Rect(0.4f, 0.4f, 0.2f, 0.2f));
        rig.View.Apply(definition);
        yield return null;

        Assert.That(rig.Tooltip.IsVisible, Is.False);

        var pointer = new PointerEventData(rig.EventSystem);
        ExecuteEvents.Execute(
            rig.View.gameObject,
            pointer,
            ExecuteEvents.pointerEnterHandler);
        Assert.That(rig.Tooltip.IsVisible, Is.True);
        Assert.That(rig.Tooltip.Text, Is.EqualTo(definition.DisplayName));

        rig.EventSystem.SetSelectedGameObject(rig.View.gameObject);
        ExecuteEvents.Execute(
            rig.View.gameObject,
            pointer,
            ExecuteEvents.pointerExitHandler);
        Assert.That(
            rig.Tooltip.IsVisible,
            Is.True,
            "Pointer exit must not hide a tooltip while the interaction keeps focus.");

        rig.EventSystem.SetSelectedGameObject(null);
        Assert.That(rig.Tooltip.IsVisible, Is.False);

        rig.EventSystem.SetSelectedGameObject(rig.View.gameObject);
        Assert.That(rig.Tooltip.IsVisible, Is.True);
        Assert.That(rig.Tooltip.Text, Is.EqualTo(definition.DisplayName));

        rig.EventSystem.SetSelectedGameObject(null);
        Assert.That(rig.Tooltip.IsVisible, Is.False);
    }

    [UnityTest]
    public IEnumerator ClickAndSubmitUseOneActivationContract()
    {
        using var rig = new TestRig();
        InteractionDefinition definition = rig.CreateDefinition(
            "INT_TEST_ACTIVATE",
            "Activate interaction",
            new Rect(0.4f, 0.4f, 0.2f, 0.2f));
        rig.View.Apply(definition);
        yield return null;

        int activations = 0;
        rig.View.Clicked += _ => activations++;

        rig.EventSystem.SetSelectedGameObject(rig.View.gameObject);
        Assert.That(rig.Tooltip.IsVisible, Is.True);
        ExecuteEvents.Execute(
            rig.View.gameObject,
            new BaseEventData(rig.EventSystem),
            ExecuteEvents.submitHandler);
        Assert.That(activations, Is.EqualTo(1));
        Assert.That(rig.Tooltip.IsVisible, Is.False);

        var pointer = new PointerEventData(rig.EventSystem)
        {
            button = PointerEventData.InputButton.Left,
        };
        ExecuteEvents.Execute(
            rig.View.gameObject,
            pointer,
            ExecuteEvents.pointerEnterHandler);
        Assert.That(rig.Tooltip.IsVisible, Is.True);
        ExecuteEvents.Execute(
            rig.View.gameObject,
            pointer,
            ExecuteEvents.pointerClickHandler);
        Assert.That(activations, Is.EqualTo(2));
        Assert.That(rig.Tooltip.IsVisible, Is.False);

        ExecuteEvents.Execute(
            rig.View.gameObject,
            pointer,
            ExecuteEvents.pointerExitHandler);
        Assert.That(
            rig.Tooltip.IsVisible,
            Is.False,
            "Activation must not let a retained focus re-open the tooltip on pointer exit.");
    }

    [Test]
    public void WorldApplyPositionsTooltipAwayFromScreenEdges()
    {
        using var rig = new TestRig();

        rig.View.Apply(rig.CreateDefinition(
            "INT_TEST_LEFT",
            "Left edge",
            new Rect(0f, 0.35f, 0.1f, 0.1f)));
        Assert.That(rig.TooltipRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(rig.TooltipRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(rig.TooltipRect.pivot, Is.EqualTo(new Vector2(0f, 0f)));
        Assert.That(rig.TooltipRect.anchoredPosition, Is.EqualTo(new Vector2(48f, 48f)));
        Assert.That(rig.MarkerRect.anchorMin, Is.EqualTo(new Vector2(1f, 0.5f)));
        Assert.That(rig.MarkerRect.anchorMax, Is.EqualTo(new Vector2(1f, 0.5f)));
        Assert.That(rig.MarkerRect.anchoredPosition, Is.EqualTo(new Vector2(-36f, 0f)));

        rig.View.Apply(rig.CreateDefinition(
            "INT_TEST_RIGHT_TOP",
            "Right top edge",
            new Rect(0.9f, 0.9f, 0.1f, 0.1f)));
        Assert.That(rig.TooltipRect.pivot, Is.EqualTo(new Vector2(1f, 1f)));
        Assert.That(
            rig.TooltipRect.anchoredPosition,
            Is.EqualTo(new Vector2(-48f, -48f)));
        Assert.That(rig.MarkerRect.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(rig.MarkerRect.anchorMax, Is.EqualTo(Vector2.zero));
        Assert.That(rig.MarkerRect.anchoredPosition, Is.EqualTo(new Vector2(36f, 36f)));

        rig.View.Apply(rig.CreateDefinition(
            "INT_TEST_CENTER_TOP",
            "Center top edge",
            new Rect(0.45f, 0.9f, 0.1f, 0.1f)));
        Assert.That(rig.TooltipRect.pivot, Is.EqualTo(new Vector2(0.5f, 1f)));
        Assert.That(rig.TooltipRect.anchoredPosition, Is.EqualTo(new Vector2(0f, -48f)));
        Assert.That(rig.MarkerRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0f)));
        Assert.That(rig.MarkerRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0f)));
        Assert.That(rig.MarkerRect.anchoredPosition, Is.EqualTo(new Vector2(0f, 36f)));
    }

    [Test]
    public void AnchoredApplyPreservesAuthoredTooltipLayout()
    {
        using var rig = new TestRig();
        var anchor = new Vector2(0.2f, 0.7f);
        var pivot = new Vector2(0.15f, 0.8f);
        var position = new Vector2(-123f, 45f);
        rig.TooltipRect.anchorMin = anchor;
        rig.TooltipRect.anchorMax = anchor;
        rig.TooltipRect.pivot = pivot;
        rig.TooltipRect.anchoredPosition = position;

        rig.View.ApplyAnchored(rig.CreateDefinition(
            "INT_TEST_ANCHORED",
            "Anchored interaction",
            new Rect(0f, 0f, 1f, 1f)));

        Assert.That(rig.TooltipRect.anchorMin, Is.EqualTo(anchor));
        Assert.That(rig.TooltipRect.anchorMax, Is.EqualTo(anchor));
        Assert.That(rig.TooltipRect.pivot, Is.EqualTo(pivot));
        Assert.That(rig.TooltipRect.anchoredPosition, Is.EqualTo(position));
    }

    private sealed class TestRig : IDisposable
    {
        private readonly GameObject canvasObject;
        private readonly GameObject eventSystemObject;
        private readonly List<InteractionDefinition> definitions = new();

        public TestRig()
        {
            canvasObject = new GameObject(
                "InteractionPointView Test Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            eventSystemObject = new GameObject(
                "InteractionPointView Test EventSystem",
                typeof(EventSystem));
            EventSystem = eventSystemObject.GetComponent<EventSystem>();

            GameObject viewObject = new(
                "InteractionPointView Test View",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            viewObject.SetActive(false);
            viewObject.transform.SetParent(canvasObject.transform, false);
            viewObject.GetComponent<Image>().color = Color.clear;
            View = viewObject.AddComponent<InteractionPointView>();

            GameObject markerObject = new("Marker", typeof(RectTransform));
            markerObject.transform.SetParent(viewObject.transform, false);
            MarkerRect = markerObject.GetComponent<RectTransform>();
            MarkerRect.anchorMin = MarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
            MarkerRect.sizeDelta = new Vector2(72f, 72f);

            GameObject tooltipObject = new("Tooltip", typeof(RectTransform));
            tooltipObject.SetActive(false);
            tooltipObject.transform.SetParent(viewObject.transform, false);
            TooltipRect = tooltipObject.GetComponent<RectTransform>();
            Tooltip = tooltipObject.AddComponent<TooltipView>();

            GameObject labelObject = new(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            labelObject.transform.SetParent(tooltipObject.transform, false);
            Text label = labelObject.GetComponent<Text>();
            SetPrivateField(Tooltip, "label", label);
            SetPrivateField(View, "tooltip", Tooltip);

            viewObject.SetActive(true);
        }

        public InteractionPointView View { get; }
        public TooltipView Tooltip { get; }
        public RectTransform TooltipRect { get; }
        public RectTransform MarkerRect { get; }
        public EventSystem EventSystem { get; }

        public InteractionDefinition CreateDefinition(
            string id,
            string displayName,
            Rect normalizedRect)
        {
            InteractionDefinition definition =
                ScriptableObject.CreateInstance<InteractionDefinition>();
            definition.name = id;
            SetPrivateField(definition, "id", id);
            SetPrivateField(definition, "displayName", displayName);
            SetPrivateField(definition, "normalizedRect", normalizedRect);
            definitions.Add(definition);
            return definition;
        }

        public void Dispose()
        {
            if (EventSystem != null && EventSystem.currentSelectedGameObject != null)
                EventSystem.SetSelectedGameObject(null);
            foreach (InteractionDefinition definition in definitions)
                if (definition != null)
                    UnityEngine.Object.DestroyImmediate(definition);
            if (eventSystemObject != null)
                UnityEngine.Object.DestroyImmediate(eventSystemObject);
            if (canvasObject != null)
                UnityEngine.Object.DestroyImmediate(canvasObject);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}.");
        field.SetValue(target, value);
    }
}
