using System;
using SlopperEditor.UI;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Interaction;
using SlopperEngine.UI.Layout;
using SlopperEngine.UI.Text;

namespace SlopperEditor.Inspector;

/// <summary>
/// A window that can inspect a certain object.
/// </summary>
public class InspectorWindow : UIElement
{
    public event Action? OnObjectChanged;

    Editor _editor;

    public InspectorWindow(object toInspect, Editor editor) : base(new(0.4f, 0.3f, 0.6f, 0.7f))
    {
        Layout.Value = new LinearArrangedLayout
        {
            Padding = default,
            IsLayoutHorizontal = false,
            StartAtMax = true,
        };

        _editor = editor;
        editor.UndoQueue!.OnQueueChanged += OnObjectChanged;

        FloatingWindowHeader header = new(this, "Inspector - " + toInspect.GetType().Name);
        UIChildren.Add(header);
        ScrollableArea content = new();
        UIChildren.Add(content);
        content.Layout.Value = DefaultLayouts.DefaultVertical;

        foreach (var mems in ReflectionCache.GetPublicMembers(toInspect.GetType()))
        {
            var span = mems.Span;
            if (span.Length < 1)
                continue;

            content.UIChildren.Add(new TextBox(span[0].DeclaringType.Name, Style.Tint, default)
            {
                LocalShape = new(0.5f, 0.5f, 0.5f, 0.5f),
                Scale = 1
            });

            var list = new Spacer
            {
                LocalShape = new(0, 1, 1, 1),
                MinHeight = 16,
            };
            list.Layout.Value = DefaultLayouts.DefaultVertical;

            content.UIChildren.Add(list);

            foreach (var mem in span)
            {
                var value = ReflectionCache.GetMemberInspectorHandler(mem.MemberType).CreateInspectorElement(mem, toInspect, this, editor);
                UIElement valueName = new();
                
                valueName.Layout.Value = DefaultLayouts.DefaultHorizontal;
                list.UIChildren.Add(valueName);
                valueName.UIChildren.Add(new InspectorName(mem.Name));
                valueName.UIChildren.Add(value);
            }
        }
    }

    protected override void OnDestroyed()
    {
        _editor.UndoQueue!.OnQueueChanged -= OnObjectChanged;
    }

    protected override UIElementSize GetSizeConstraints() => new(Alignment.Middle, Alignment.Middle, 100, 100);
}