using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;
using SlopperEngine.UI.Interaction;
using SlopperEngine.UI.Text;
using SlopperEditor.UI;

namespace SlopperEditor.Toolbar;

/// <summary>
/// Represents a tab on the toolbar. 
/// </summary>
public abstract class Tab : BaseButton
{
    protected readonly Editor editor;
    protected readonly Toolbar bar;
    readonly ColorRectangle _background;
    readonly TextBox _textRenderer;
    readonly Popup _options;
    bool _hideOptionsNextFrame;

    protected Tab(string text, Editor editor, Toolbar bar)
    {
        this.editor = editor;
        this.bar = bar;

        _options = new(true);
        Children.Add(_options);

        _background = new(new(0, 0, 1, 1), Style.ForegroundWeak);
        UIChildren.Add(_background);

        _textRenderer = new(text);
        _textRenderer.Horizontal = Alignment.Middle;
        _textRenderer.Vertical = Alignment.Middle;
        _textRenderer.LocalShape = new(0.5f, 0.5f, 0.5f, 0.5f);
        _textRenderer.Scale = 1;
        UIChildren.Add(_textRenderer);

        LocalShape = new(0.5f, 0.5f, 0.5f, 0.5f);

        SetupOptions(_options);
    }

    public void ShowOptions()
    {
        bar.CurrentlyOpen?.HideOptions();
        _options.Show(LastGlobalShape.Min);
        bar.CurrentlyOpen = this;
    }

    public void HideOptions()
    {
        if (bar.CurrentlyOpen == this)
            bar.CurrentlyOpen = null;
        _options.Hide();
        _hideOptionsNextFrame = false;
    }

    [OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if (!_options.Shown)
            return;

        if (LastRenderer == null)
            return;

        if (_hideOptionsNextFrame)
        {
            HideOptions();
            return;
        }

        Vector2 mousePos = args.NormalizedMousePosition * 2 - Vector2.One;
        bool hovered = _options.Hovered(mousePos);
        hovered |= LastGlobalShape.ContainsInclusive(mousePos) || LastGlobalShape.DistanceToNearestEdge(mousePos) * LastRenderer.GetScreenSize().EuclideanLength < 10;
        if (!hovered)
            _hideOptionsNextFrame = true;
    }

    protected abstract void SetupOptions(Popup options);

    protected sealed override void OnPressed(MouseButton button)
    {
        _background.Color = Style.Tint;
        _textRenderer.TextColor = Style.ForegroundStrong;
        if (button == MouseButton.Left)
            ShowOptions();
    }

    protected sealed override void OnAnyRelease(MouseButton button) { }

    protected sealed override void OnAllButtonsReleased()
    {
        _background.Color = Style.ForegroundStrong;
        _textRenderer.TextColor = Style.Tint;
    }

    protected sealed override void OnMouseEntry()
    {
        OnAllButtonsReleased();
        if (bar.CurrentlyOpen != null && bar.CurrentlyOpen != this)
            ShowOptions();
    }

    protected sealed override void OnMouseExit()
    {
        _background.Color = Style.ForegroundWeak;
        _textRenderer.TextColor = Style.Tint;
    }

    protected sealed override void OnEnable()
    {
        OnMouseExit();
    }

    protected sealed override void OnDisable()
    {
        _background.Color = Style.BackgroundWeak;
        _textRenderer.TextColor = Style.ForegroundStrong;
    }

    protected override UIElementSize GetSizeConstraints()
    {
        var constr = _textRenderer.LastSizeConstraints;
        constr.GrowX = Alignment.Max;
        return constr;
    }
}