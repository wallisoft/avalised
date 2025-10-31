using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace Avalised;

/// <summary>
/// Provides smooth, GPU-accelerated resize functionality for canvas controls
/// 8 resize handles: 4 corners + 4 edges
/// Uses RenderTransform during drag, commits to Width/Height on release
/// </summary>
public class ResizeBehavior
{
    private readonly Control _control;
    private readonly Canvas _canvas;
    private readonly double _minWidth;
    private readonly double _minHeight;
    private readonly double _handleSize = 8; // Hit test zone size in pixels
    
    private ResizeHandle _activeHandle = ResizeHandle.None;
    private Point _resizeStartPoint;
    private double _startWidth;
    private double _startHeight;
    private double _startLeft;
    private double _startTop;
    private ScaleTransform? _resizeTransform;
    private TranslateTransform? _positionTransform;
    private ITransform? _originalTransform;
    
    // Event fired during resize to update selection border (width, height, left, top)
    public Action<double, double, double, double>? OnResizing;
    public Action? OnResizeComplete;
    
    public enum ResizeHandle
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }
    
    public ResizeBehavior(Control control, Canvas canvas, double minWidth = 20, double minHeight = 20)
    {
        _control = control;
        _canvas = canvas;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }
    
    // NOTE: ResizeBehavior does NOT attach event handlers!
    // DesignerLayout routes events to HandlePointerXxx methods manually
    
    public void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_activeHandle != ResizeHandle.None)
        {
            // Currently resizing - update transform
            PerformResize(e);
            e.Handled = true;
            return;
        }
        
        // Not resizing - update cursor based on position
        var pos = e.GetPosition(_control);
        var handle = GetHandleAtPosition(pos);
        UpdateCursor(handle);
    }
    
    public bool HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(_control);
        _activeHandle = GetHandleAtPosition(pos);
        
        Console.WriteLine($"🔍 ResizeBehavior: pos=({pos.X:F0},{pos.Y:F0}), handle={_activeHandle}, bounds={_control.Bounds.Width:F0}x{_control.Bounds.Height:F0}");
        
        if (_activeHandle != ResizeHandle.None)
        {
            // Starting resize - return true to indicate we're handling it
            // Start resizing
            _resizeStartPoint = e.GetPosition(_canvas);
            _startWidth = _control.Bounds.Width;
            _startHeight = _control.Bounds.Height;
            _startLeft = Canvas.GetLeft(_control);
            _startTop = Canvas.GetTop(_control);
            
            // Setup transforms for smooth resizing
            _originalTransform = _control.RenderTransform;
            var transformGroup = new TransformGroup();
            _resizeTransform = new ScaleTransform(1, 1);
            _positionTransform = new TranslateTransform(0, 0);
            transformGroup.Children.Add(_resizeTransform);
            transformGroup.Children.Add(_positionTransform);
            _control.RenderTransform = transformGroup;
            _control.RenderTransformOrigin = GetTransformOrigin(_activeHandle);
            
            e.Handled = true;
            return true;  // We're handling resize
        }
        return false;  // Not in resize zone - don't handle
    }
    
    public void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_activeHandle != ResizeHandle.None && _resizeTransform != null)
        {
            // Commit the resize to actual Width/Height
            CommitResize();
            
            // Restore original transform
            _control.RenderTransform = _originalTransform;
            _control.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
            
            _activeHandle = ResizeHandle.None;
            _resizeTransform = null;
            _positionTransform = null;
            _originalTransform = null;
            
            // Update cursor
            var pos = e.GetPosition(_control);
            var handle = GetHandleAtPosition(pos);
            UpdateCursor(handle);
            
            e.Handled = true;
        }
    }
    
    public void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        if (_activeHandle == ResizeHandle.None)
        {
            _control.Cursor = Cursor.Default;
        }
    }
    
    /// <summary>
    /// Detect which resize handle (if any) is at the given position
    /// </summary>
    public ResizeHandle GetHandleAtPosition(Point pos)
    {
        var width = _control.Bounds.Width;
        var height = _control.Bounds.Height;
        
        var isLeft = pos.X <= _handleSize;
        var isRight = pos.X >= width - _handleSize;
        var isTop = pos.Y <= _handleSize;
        var isBottom = pos.Y >= height - _handleSize;
        
        // Priority: corners first, then edges
        if (isTop && isLeft) return ResizeHandle.TopLeft;
        if (isTop && isRight) return ResizeHandle.TopRight;
        if (isBottom && isLeft) return ResizeHandle.BottomLeft;
        if (isBottom && isRight) return ResizeHandle.BottomRight;
        if (isTop) return ResizeHandle.Top;
        if (isBottom) return ResizeHandle.Bottom;
        if (isLeft) return ResizeHandle.Left;
        if (isRight) return ResizeHandle.Right;
        
        return ResizeHandle.None;
    }
    
    /// <summary>
    /// Update cursor based on resize handle
    /// </summary>
    private void UpdateCursor(ResizeHandle handle)
    {
        _control.Cursor = handle switch
        {
            ResizeHandle.TopLeft => new Cursor(StandardCursorType.TopLeftCorner),
            ResizeHandle.Top => new Cursor(StandardCursorType.TopSide),
            ResizeHandle.TopRight => new Cursor(StandardCursorType.TopRightCorner),
            ResizeHandle.Right => new Cursor(StandardCursorType.RightSide),
            ResizeHandle.BottomRight => new Cursor(StandardCursorType.BottomRightCorner),
            ResizeHandle.Bottom => new Cursor(StandardCursorType.BottomSide),
            ResizeHandle.BottomLeft => new Cursor(StandardCursorType.BottomLeftCorner),
            ResizeHandle.Left => new Cursor(StandardCursorType.LeftSide),
            _ => Cursor.Default
        };
    }
    
    /// <summary>
    /// Get the transform origin point for the active resize handle
    /// Opposite corner stays fixed during resize
    /// </summary>
    private RelativePoint GetTransformOrigin(ResizeHandle handle)
    {
        return handle switch
        {
            ResizeHandle.TopLeft => new RelativePoint(1, 1, RelativeUnit.Relative),      // Bottom-right fixed
            ResizeHandle.Top => new RelativePoint(0.5, 1, RelativeUnit.Relative),        // Bottom center fixed
            ResizeHandle.TopRight => new RelativePoint(0, 1, RelativeUnit.Relative),     // Bottom-left fixed
            ResizeHandle.Right => new RelativePoint(0, 0.5, RelativeUnit.Relative),      // Left center fixed
            ResizeHandle.BottomRight => new RelativePoint(0, 0, RelativeUnit.Relative),  // Top-left fixed
            ResizeHandle.Bottom => new RelativePoint(0.5, 0, RelativeUnit.Relative),     // Top center fixed
            ResizeHandle.BottomLeft => new RelativePoint(1, 0, RelativeUnit.Relative),   // Top-right fixed
            ResizeHandle.Left => new RelativePoint(1, 0.5, RelativeUnit.Relative),       // Right center fixed
            _ => new RelativePoint(0, 0, RelativeUnit.Relative)
        };
    }
    
    /// <summary>
    /// Perform resize using GPU-accelerated transforms
    /// </summary>
    private void PerformResize(PointerEventArgs e)
    {
        if (_resizeTransform == null || _positionTransform == null) return;
        
        var currentPos = e.GetPosition(_canvas);
        var deltaX = currentPos.X - _resizeStartPoint.X;
        var deltaY = currentPos.Y - _resizeStartPoint.Y;
        
        double scaleX = 1.0;
        double scaleY = 1.0;
        double translateX = 0;
        double translateY = 0;
        
        switch (_activeHandle)
        {
            case ResizeHandle.Right:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth + deltaX) / _startWidth);
                break;
                
            case ResizeHandle.Bottom:
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight + deltaY) / _startHeight);
                break;
                
            case ResizeHandle.Left:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth - deltaX) / _startWidth);
                if (scaleX > _minWidth / _startWidth)
                {
                    translateX = deltaX;
                    Canvas.SetLeft(_control, _startLeft + deltaX);
                }
                break;
                
            case ResizeHandle.Top:
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight - deltaY) / _startHeight);
                if (scaleY > _minHeight / _startHeight)
                {
                    translateY = deltaY;
                    Canvas.SetTop(_control, _startTop + deltaY);
                }
                break;
                
            case ResizeHandle.TopLeft:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth - deltaX) / _startWidth);
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight - deltaY) / _startHeight);
                if (scaleX > _minWidth / _startWidth)
                {
                    translateX = deltaX;
                    Canvas.SetLeft(_control, _startLeft + deltaX);
                }
                if (scaleY > _minHeight / _startHeight)
                {
                    translateY = deltaY;
                    Canvas.SetTop(_control, _startTop + deltaY);
                }
                break;
                
            case ResizeHandle.TopRight:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth + deltaX) / _startWidth);
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight - deltaY) / _startHeight);
                if (scaleY > _minHeight / _startHeight)
                {
                    translateY = deltaY;
                    Canvas.SetTop(_control, _startTop + deltaY);
                }
                break;
                
            case ResizeHandle.BottomLeft:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth - deltaX) / _startWidth);
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight + deltaY) / _startHeight);
                if (scaleX > _minWidth / _startWidth)
                {
                    translateX = deltaX;
                    Canvas.SetLeft(_control, _startLeft + deltaX);
                }
                break;
                
            case ResizeHandle.BottomRight:
                scaleX = Math.Max(_minWidth / _startWidth, (_startWidth + deltaX) / _startWidth);
                scaleY = Math.Max(_minHeight / _startHeight, (_startHeight + deltaY) / _startHeight);
                break;
        }
        
        // Apply transforms - GPU accelerated!
        _resizeTransform.ScaleX = scaleX;
        _resizeTransform.ScaleY = scaleY;
        _positionTransform.X = translateX;
        _positionTransform.Y = translateY;
        
        // Notify about resize (for updating selection border)
        var currentLeft = Canvas.GetLeft(_control);
        var currentTop = Canvas.GetTop(_control);
        OnResizing?.Invoke(_startWidth * scaleX, _startHeight * scaleY, currentLeft, currentTop);
    }
    
    /// <summary>
    /// Commit the resize to actual Width/Height properties
    /// </summary>
    private void CommitResize()
    {
        if (_resizeTransform == null) return;
        
        // Calculate final dimensions
        var finalWidth = Math.Max(_minWidth, _startWidth * _resizeTransform.ScaleX);
        var finalHeight = Math.Max(_minHeight, _startHeight * _resizeTransform.ScaleY);
        
        // Apply to control
        _control.Width = finalWidth;
        _control.Height = finalHeight;
        
        // Position already set during resize for left/top handles
        Console.WriteLine($"✅ Resized {_control.Name}: {finalWidth:F0}×{finalHeight:F0}");
        
        // Notify resize complete
        OnResizeComplete?.Invoke();
    }
}
