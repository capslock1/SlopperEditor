using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Text;

namespace SlopperEditor.Inspector;

/// <summary>
/// The name of a member in the inspector window.
/// </summary>
public class InspectorName : UIElement
{
    readonly UIElement _box;
    public InspectorName(string name) : base(new(0,0,0.5f,1))
    {
        UIChildren.Add(_box = new TextBox(name, Style.Tint)
        {
            Horizontal = Alignment.Max,
            Vertical = Alignment.Min,
            Scale = 1
        });
    }

    protected override Box2 GetScissorRegion() => new(0,0,1,1);

    protected override UIElementSize GetSizeConstraints()
    {
        var constr = _box.LastSizeConstraints;
        constr.MinimumSizeX = 0;
        constr.MaximumSizeX = int.MaxValue;
        return constr;
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        if(_box.LastGlobalShape.Size.X > LastGlobalShape.Size.X)
            Children.Add(new InspectorNameScroller(this, _box));
        base.HandleEvent(ref e);
    }

    class InspectorNameScroller(UIElement owner, UIElement boxToMove) : SceneObject
    {
        float _waitTimer = 0;
        MoveState _moveState;

        [OnInputUpdate]
        void Input(InputUpdateArgs args)
        {
            if(!owner.LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
                Destroy();
        }

        [OnFrameUpdate]
        void Frame(FrameUpdateArgs args)
        {
            const float waitDelaySeconds = 1;
            const float speedMult = -0.07f;

            if(_moveState == MoveState.WaitMin || _moveState == MoveState.WaitMax)
            {
                _waitTimer += args.DeltaTime;
                if(_waitTimer < waitDelaySeconds) 
                    return;
                _waitTimer = 0;
            }

            float deltaX = args.DeltaTime / owner.LastGlobalShape.Size.X * speedMult;
            var currentCenter = boxToMove.LocalShape.Center;
            switch(_moveState)
            {
                default: // wait min
                _moveState = MoveState.GoToMax;
                return;
                case MoveState.WaitMax:
                _moveState = MoveState.GoToMin;
                return;

                case MoveState.GoToMax:
                currentCenter.X += deltaX;
                if(boxToMove.LastGlobalShape.Max.X < owner.LastGlobalShape.Max.X)
                    _moveState = MoveState.WaitMax;
                break;
                case MoveState.GoToMin:
                currentCenter.X -= deltaX;
                if(boxToMove.LastGlobalShape.Min.X > owner.LastGlobalShape.Min.X)
                    _moveState = MoveState.WaitMin;
                break;
            }
            boxToMove.LocalShape.Center = currentCenter;
        }

        protected override void OnDestroyed()
        {
            boxToMove.LocalShape = new(0,0,1,1);
            base.OnDestroyed();
        }

        enum MoveState
        {
            WaitMin = 0,
            WaitMax,
            GoToMax, 
            GoToMin, 
        }
    }
}