using System;
using System.Numerics;
using Avalonia.Collections.Pooled;
using Avalonia.Platform;
using Avalonia.Rendering.Composition.Drawing;
using Avalonia.Rendering.Composition.Transport;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Utilities;

namespace Avalonia.Rendering.Composition.Server;

internal class ServerCompositionDrawListVisual : ServerCompositionContainerVisual
{
#if DEBUG
    public readonly Visual UiVisual;
#endif
    private CompositionDrawList? _renderCommands;
    
    public ServerCompositionDrawListVisual(ServerCompositor compositor, Visual v) : base(compositor)
    {
#if DEBUG
        UiVisual = v;
#endif
    }

    Rect? _contentBounds;

    public override Rect ContentBounds
    {
        get
        {
            if (_contentBounds == null)
            {
                var rect = Rect.Empty;
                if(_renderCommands!=null)
                    foreach (var cmd in _renderCommands)
                        rect = rect.Union(cmd.Item.Bounds);
                _contentBounds = rect;
            }

            return _contentBounds.Value;
        }
    }

    protected override void DeserializeChangesCore(BatchStreamReader reader, TimeSpan commitedAt)
    {
        if (reader.Read<byte>() == 1)
        {
            _renderCommands?.Dispose();
            _renderCommands = reader.ReadObject<CompositionDrawList?>();
            _contentBounds = null;
        }
        base.DeserializeChangesCore(reader, commitedAt);
    }

    protected override void RenderCore(CompositorDrawingContextProxy canvas)
    {
        if (_renderCommands != null)
        {
            _renderCommands.Render(canvas);
        }
        base.RenderCore(canvas);
    }
    
#if DEBUG
    public override string ToString()
    {
        return UiVisual.GetType().ToString();
    }
#endif
}