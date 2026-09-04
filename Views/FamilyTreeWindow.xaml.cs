using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using WpfPrismFrameworkTemplate.ViewModels;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Shapes;
using WpfPrismFrameworkTemplate.Model;
using System.Linq;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// Interaction logic for FamilyTreeWindow
    /// </summary>
    public partial class FamilyTreeWindow : UserControl
    {
        private bool _isDragging;
        private Point _startPoint;
        private TextBlock _draggedElement;
        private bool _isCanvasInternalDrag;
        private Point _originalPosition;

        // 连线相关变量
        private TextBlock _sourceElement;
        private bool _isConnecting;
        private Path _previewLine;
        // 将原有的Dictionary<string, List<Line>>替换为更完善的结构
        private Dictionary<string, List<ConnectionLineInfo>> _connectionLinesInfo = new Dictionary<string, List<ConnectionLineInfo>>();

        // 添加成员变量跟踪当前缩放级别
        private double _currentScale = 1.0;
        private Point _lastPanPoint;
        private bool _isPanning = false;

        private bool _isMoving = false;
        private TextBlock _currentMovingElement = null;
        private Point _dragOffset;

        public FamilyTreeWindow()
        {
            InitializeComponent();
            _connectionLinesInfo = new Dictionary<string, List<ConnectionLineInfo>>();

            // 添加平移功能的鼠标事件
            DestinationCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            DestinationCanvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            DestinationCanvas.MouseMove += Canvas_MouseMove;
        }

        private void TextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentPosition = e.GetPosition(null);
            currentPosition = ApplyInverseTransform(currentPosition);
            Vector diff = _startPoint - currentPosition;

            // 如果移动超过了拖拽阈值，开始拖拽操作
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                // 获取ViewModel
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                if (viewModel == null) return;

                // 创建拖拽数据
                DataObject dragData = new DataObject();
                dragData.SetData("DraggableTextContent", viewModel.DraggableText);

                // 开始拖拽操作
                DragDrop.DoDragDrop(textBlock, dragData, DragDropEffects.Copy);

                _isDragging = false;
                textBlock.ReleaseMouseCapture();
            }
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = true;

            // 获取焦点以确保接收后续事件
            ((TextBlock)sender).CaptureMouse();
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((TextBlock)sender).ReleaseMouseCapture();
        }

        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                // 获取拖拽的文本内容
                string droppedText = e.Data.GetData("DraggableTextContent") as string;

                // 创建新的TextBlock和外层Border (用于右键菜单和可视化标识)
                TextBlock newTextBlock = new TextBlock
                {
                    Text = droppedText,
                    Background = Brushes.LightGreen,
                    Padding = new Thickness(10),
                    Tag = Guid.NewGuid().ToString() // 用于标识唯一控件
                };

                // 为TextBlock添加右键菜单
                newTextBlock.ContextMenu = new ContextMenu();
                MenuItem connectMenuItem = new MenuItem { Header = "开始连线" };
                connectMenuItem.Click += (s, args) => StartConnection(newTextBlock);
                newTextBlock.ContextMenu.Items.Add(connectMenuItem);

                // 为新创建的TextBlock添加鼠标事件处理，使其能在Canvas内拖动
                newTextBlock.MouseLeftButtonDown += CanvasElement_MouseLeftButtonDown;
                newTextBlock.MouseMove += CanvasElement_MouseMove;
                newTextBlock.MouseLeftButtonUp += CanvasElement_MouseLeftButtonUp;

                // 当连线模式激活时，单击事件用于完成连线
                newTextBlock.MouseLeftButtonDown += (s, args) => {
                    if (_isConnecting && _sourceElement != null && _sourceElement != newTextBlock)
                    {
                        CompleteConnection(newTextBlock);
                        args.Handled = true;
                    }
                };

                // 获取放置的坐标位置
                Point dropPosition = e.GetPosition(DestinationCanvas);

                // 应用逆变换来调整位置
                dropPosition = ApplyInverseTransform(dropPosition);

                // 将新的TextBlock添加到Canvas中
                DestinationCanvas.Children.Add(newTextBlock);

                // 设置Canvas的附加属性，确定位置
                Canvas.SetLeft(newTextBlock, dropPosition.X);
                Canvas.SetTop(newTextBlock, dropPosition.Y);

                // 初始化连线字典
                // 初始化连接线信息字典
                _connectionLinesInfo[newTextBlock.Tag.ToString()] = new List<ConnectionLineInfo>();

                // 通知ViewModel
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                viewModel?.NotifyTextBlockDropped(droppedText, dropPosition, newTextBlock.Tag.ToString());

                e.Handled = true;
            }
        }

        private void CanvasElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果正在连线模式，则跳过拖拽
            if (_isConnecting) return;

            _draggedElement = sender as TextBlock;
            if (_draggedElement != null)
            {
                _isCanvasInternalDrag = true;
                _isDragging = true;
                _startPoint = e.GetPosition(DestinationCanvas);

                // 记录原始位置
                _originalPosition = new Point(
                    Canvas.GetLeft(_draggedElement),
                    Canvas.GetTop(_draggedElement));

                // 捕获鼠标
                _draggedElement.CaptureMouse();
                e.Handled = true;
            }
        }

        private void CanvasElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isCanvasInternalDrag && _isDragging && _draggedElement != null)
            {
                Point currentPosition = e.GetPosition(DestinationCanvas);

                // 计算位移
                double offsetX = currentPosition.X - _startPoint.X;
                double offsetY = currentPosition.Y - _startPoint.Y;

                // 更新元素位置
                double newLeft = _originalPosition.X + offsetX;
                double newTop = _originalPosition.Y + offsetY;

                Canvas.SetLeft(_draggedElement, newLeft);
                Canvas.SetTop(_draggedElement, newTop);

                // 更新相关的连线
                UpdateConnectionLines(_draggedElement);

                e.Handled = true;
            }
        }

        private void CanvasElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isCanvasInternalDrag && _draggedElement != null)
            {
                // 释放鼠标捕获
                _draggedElement.ReleaseMouseCapture();

                // 更新ViewModel中的位置信息
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                viewModel?.UpdateTextBlockPosition(
                    _draggedElement.Text,
                    Canvas.GetLeft(_draggedElement),
                    Canvas.GetTop(_draggedElement));

                // 重置状态
                _isDragging = false;
                _isCanvasInternalDrag = false;
                _draggedElement = null;

                e.Handled = true;
            }
        }

        // 开始连线过程
        private void StartConnection(TextBlock source)
        {
            _sourceElement = source;
            _isConnecting = true;

            // 创建预览线
            Point sourceCenter = GetElementCenter(source);
            _previewLine = CreateConnectionLine(sourceCenter, sourceCenter, Brushes.Gray, 1);

            // 添加到Canvas
            DestinationCanvas.Children.Add(_previewLine);

            // 设置鼠标事件处理
            DestinationCanvas.MouseMove += Canvas_MouseMoveForConnection;
            DestinationCanvas.MouseRightButtonDown += Canvas_MouseRightButtonDownForConnection;

            // 更改光标以指示连接模式
            this.Cursor = Cursors.Cross;
        }

        // 鼠标移动时更新预览线
        private void Canvas_MouseMoveForConnection(object sender, MouseEventArgs e)
        {
            if (_isConnecting && _sourceElement != null && _previewLine != null)
            {
                Point sourceCenter = GetElementCenter(_sourceElement);
                Point currentPosition = e.GetPosition(DestinationCanvas);

                // 重新创建带箭头的预览线
                DestinationCanvas.Children.Remove(_previewLine);
                _previewLine = CreateConnectionLine(sourceCenter, currentPosition, Brushes.Gray, 1);
                DestinationCanvas.Children.Add(_previewLine);
            }
        }

        // 右键点击取消连线
        private void Canvas_MouseRightButtonDownForConnection(object sender, MouseButtonEventArgs e)
        {
            if (_isConnecting)
            {
                CancelConnection();
                e.Handled = true;
            }
        }

        // 取消连线
        private void CancelConnection()
        {
            if (_previewLine != null)
            {
                DestinationCanvas.Children.Remove(_previewLine);
                _previewLine = null;
            }

            DestinationCanvas.MouseMove -= Canvas_MouseMoveForConnection;
            DestinationCanvas.MouseRightButtonDown -= Canvas_MouseRightButtonDownForConnection;
            _isConnecting = false;
            _sourceElement = null;
            this.Cursor = Cursors.Arrow;
        }

        // 完成连线
        private void CompleteConnection(TextBlock target)
        {
            if (_sourceElement == null || _previewLine == null) return;

            // 删除预览线
            DestinationCanvas.Children.Remove(_previewLine);

            // 设置线的起点和终点
            Point sourceCenter = GetElementCenter(_sourceElement);
            Point targetCenter = GetElementCenter(target);

            // 创建实际的连接线（带箭头）
            Path connectionPath = CreateConnectionLine(sourceCenter, targetCenter, Brushes.Red, 1);

            // 将连接线添加到Canvas
            DestinationCanvas.Children.Add(connectionPath);

            // 存储连接线信息
            string sourceId = _sourceElement.Tag.ToString();
            string targetId = target.Tag.ToString();

            // 创建连接线信息对象
            ConnectionLineInfo lineInfo = new ConnectionLineInfo
            {
                LinePath = connectionPath,
                SourceId = sourceId,
                TargetId = targetId
            };

            // 初始化字典条目（如果需要）
            if (!_connectionLinesInfo.ContainsKey(sourceId))
            {
                _connectionLinesInfo[sourceId] = new List<ConnectionLineInfo>();
            }
            if (!_connectionLinesInfo.ContainsKey(targetId))
            {
                _connectionLinesInfo[targetId] = new List<ConnectionLineInfo>();
            }

            // 在源和目标的字典中都添加此连接信息
            _connectionLinesInfo[sourceId].Add(lineInfo);
            _connectionLinesInfo[targetId].Add(lineInfo);

            // 将连接信息添加到ViewModel
            FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
            viewModel?.AddConnection(sourceId, targetId);

            // 重置状态
            _previewLine = null;
            DestinationCanvas.MouseMove -= Canvas_MouseMoveForConnection;
            DestinationCanvas.MouseRightButtonDown -= Canvas_MouseRightButtonDownForConnection;
            _isConnecting = false;
            _sourceElement = null;
            this.Cursor = Cursors.Arrow;
        }

        // 获取元素的中心点
        private Point GetElementCenter(TextBlock element)
        {
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);

            
            double width = element.ActualWidth;
            double height = element.ActualHeight;

            //return new Point(left + width / 2, top + height / 2);
            // 计算中心点
            return new Point(
                left + element.ActualWidth / 2,
                top + element.ActualHeight / 2
            );
        }

        // 更新与特定元素相关的所有连接线
        private void UpdateConnectionLines(TextBlock element)
        {
            if (element == null || element.Tag == null) return;

            string elementId = element.Tag.ToString();
            if (!_connectionLinesInfo.ContainsKey(elementId)) return;

            Point elementCenter = GetElementCenter(element);

            foreach (var lineInfo in _connectionLinesInfo[elementId])
            {
                // 确定此元素是源还是目标
                bool isSource = lineInfo.SourceId == elementId;

                // 查找连接的另一个元素
                TextBlock otherElement = null;
                foreach (UIElement child in DestinationCanvas.Children)
                {
                    if (child is TextBlock tb && tb.Tag != null)
                    {
                        string otherId = tb.Tag.ToString();
                        if ((isSource && otherId == lineInfo.TargetId) ||
                            (!isSource && otherId == lineInfo.SourceId))
                        {
                            otherElement = tb;
                            break;
                        }
                    }
                }

                if (otherElement != null)
                {
                    Point otherCenter = GetElementCenter(otherElement);

                    // 更新连接线端点
                    Path oldPath = lineInfo.LinePath;
                    DestinationCanvas.Children.Remove(oldPath);

                    Path newPath;
                    if (isSource)
                    {
                        // 当前元素是源，箭头指向目标
                        newPath = CreateConnectionLine(elementCenter, otherCenter, Brushes.Red, 1);
                    }
                    else
                    {
                        // 当前元素是目标，箭头来自源
                        newPath = CreateConnectionLine(otherCenter, elementCenter, Brushes.Red, 1);
                    }

                    // 将新路径添加到Canvas
                    DestinationCanvas.Children.Add(newPath);

                    // 更新连接信息
                    lineInfo.LinePath = newPath;
                }
            }
        }

        private Path CreateConnectionLine(Point start, Point end, Brush strokeColor, double strokeThickness, string text = "测试")
        {
            // 创建几何图形路径
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            // 设置线的起点
            pathFigure.StartPoint = start;
            // 添加直线段到终点
            LineSegment lineSegment = new LineSegment(end, true);
            pathFigure.Segments.Add(lineSegment);
            // 将路径图形添加到几何图形中
            pathGeometry.Figures.Add(pathFigure);

            // 计算线的中点位置（将箭头放在中间）
            Point midPoint = new Point(
                (start.X + end.X) / 2,
                (start.Y + end.Y) / 2
            );

            // 计算箭头方向的向量
            Vector direction = Point.Subtract(end, start);
            direction.Normalize();

            // 箭头的两个侧翼点
            const double arrowSize = 10;
            Vector leftVector = new Vector(-direction.X * arrowSize + direction.Y * arrowSize / 2,
                                          -direction.Y * arrowSize - direction.X * arrowSize / 2);
            Vector rightVector = new Vector(-direction.X * arrowSize - direction.Y * arrowSize / 2,
                                           -direction.Y * arrowSize + direction.X * arrowSize / 2);

            // 使用中点作为箭头尖端
            Point arrowTip = new Point(
                midPoint.X + direction.X * arrowSize / 2,
                midPoint.Y + direction.Y * arrowSize / 2
            );
            Point arrowLeft = Point.Add(arrowTip, leftVector);
            Point arrowRight = Point.Add(arrowTip, rightVector);

            // 创建箭头的路径图形
            PathFigure arrowFigure = new PathFigure();
            arrowFigure.StartPoint = arrowTip;
            arrowFigure.Segments.Add(new LineSegment(arrowLeft, true));
            arrowFigure.Segments.Add(new LineSegment(arrowRight, true));
            arrowFigure.Segments.Add(new LineSegment(arrowTip, true));
            arrowFigure.IsClosed = true;

            // 将箭头路径添加到几何图形
            pathGeometry.Figures.Add(arrowFigure);

            // 创建Path对象并设置其属性
            Path path = new Path
            {
                Data = pathGeometry,
                Stroke = strokeColor,
                StrokeThickness = strokeThickness,
                Fill = strokeColor // 填充箭头
            };

            // 添加文字标签（如果提供了文本）
            if (!string.IsNullOrEmpty(text))
            {
                // 计算文字旋转角度，使其与线平行
                double angle = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180 / Math.PI;

                // 确保文字始终正向显示（不倒置）
                if (angle > 90 || angle < -90)
                {
                    // 如果角度会导致文字倒置，则旋转180度
                    angle += 180;
                    if (angle > 180) angle -= 360; // 保持角度在 -180 到 180 之间
                }

                // 创建文本路径
                FormattedText formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    20,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

                // 计算文本的宽度和高度
                double textWidth = formattedText.Width;
                double textHeight = formattedText.Height;

                // 计算垂直于线的向量（用于将文本放在箭头上方或下方）
                Vector perpendicular = new Vector(-direction.Y, direction.X);
                perpendicular.Normalize();

                // 文本偏移距离（调整为适当的值使文本位于箭头上方或下方）
                double textOffset = 15; // 根据需要调整

                // 将文字放在箭头的正上方或正下方
                Point textCenterPoint = new Point(
                    midPoint.X + perpendicular.X * textOffset,
                    midPoint.Y + perpendicular.Y * textOffset
                );

                // 调整文本位置，考虑到旋转和居中
                Point textPosition = new Point(
                    textCenterPoint.X - textWidth / 2,
                    textCenterPoint.Y - textHeight / 2
                );

                // 创建文本的几何形状
                Geometry textGeometry = formattedText.BuildGeometry(textPosition);

                // 创建旋转变换
                RotateTransform rotateTransform = new RotateTransform(angle, textCenterPoint.X, textCenterPoint.Y);
                textGeometry = textGeometry.Clone();
                textGeometry.Transform = rotateTransform;

                // 将文本几何形状添加到路径中
                PathGeometry textPathGeometry = textGeometry as PathGeometry;

                // 如果文本几何形状不是PathGeometry类型，则进行转换
                if (textPathGeometry == null)
                {
                    textPathGeometry = textGeometry.GetFlattenedPathGeometry();
                }

                foreach (PathFigure figure in textPathGeometry.Figures)
                {
                    pathGeometry.Figures.Add(figure);
                }

                // 重新设置Path数据
                path.Data = pathGeometry;
            }

            // 设置Z索引使其低于TextBlock
            Panel.SetZIndex(path, -1);

            return path;
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomCanvas(1.1);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomCanvas(0.9);
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            // 重置缩放级别
            _currentScale = 1.0;
            CanvasScaleTransform.ScaleX = 1.0;
            CanvasScaleTransform.ScaleY = 1.0;

            // 重置平移位置
            CanvasTranslateTransform.X = 0;
            CanvasTranslateTransform.Y = 0;
        }

        // 缩放方法
        private void ZoomCanvas(double zoomFactor)
        {
            // 计算新的缩放值
            double newScale = _currentScale * zoomFactor;

            // 限制最大和最小缩放级别
            if (newScale < 0.1) newScale = 0.1;
            if (newScale > 5.0) newScale = 5.0;

            // 更新缩放级别
            _currentScale = newScale;

            // 应用新的缩放
            CanvasScaleTransform.ScaleX = _currentScale;
            CanvasScaleTransform.ScaleY = _currentScale;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 计算新的缩放值
            double zoom = e.Delta > 0 ? 1.1 : 0.9;

            // 限制最大和最小缩放级别
            double newScale = _currentScale * zoom;
            if (newScale < 0.1) newScale = 0.1;
            if (newScale > 5.0) newScale = 5.0;

            // 更新缩放级别
            _currentScale = newScale;

            // 应用新的缩放
            CanvasScaleTransform.ScaleX = _currentScale;
            CanvasScaleTransform.ScaleY = _currentScale;

            // 防止事件继续传播
            e.Handled = true;
        }

        // 处理平移操作开始
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果按下了Ctrl键，则启动平移模式
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _isPanning = true;
                _lastPanPoint = e.GetPosition(DestinationCanvas);
                DestinationCanvas.CaptureMouse();
                e.Handled = true;
            }
        }

        // 处理平移操作结束
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                DestinationCanvas.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        // 处理平移时的鼠标移动
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 处理平移操作
            if (_isPanning)
            {
                Point currentPoint = e.GetPosition(DestinationCanvas);
                Vector delta = Point.Subtract(currentPoint, _lastPanPoint);

                // 根据缩放级别调整平移速度
                delta.X /= _currentScale;
                delta.Y /= _currentScale;

                // 更新平移变换
                CanvasTranslateTransform.X += delta.X;
                CanvasTranslateTransform.Y += delta.Y;

                _lastPanPoint = currentPoint;
                e.Handled = true;
            }

            // 如果已有连接相关的鼠标移动事件，确保与之不冲突
            if (_isConnecting && !_isPanning)
            {
                // 这里是原有的连接预览逻辑
                // Canvas_MouseMoveForConnection 应该在这里处理
            }
        }

        // 添加辅助方法，用于在缩放时获取准确的位置
        private Point ApplyInverseTransform(Point point)
        {
            // 应用逆变换来调整坐标
            return new Point(
                (point.X - CanvasTranslateTransform.X) / _currentScale,
                (point.Y - CanvasTranslateTransform.Y) / _currentScale
            );
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果已经在连接模式，右键点击取消连接操作
            if (_isConnecting)
            {
                CancelConnection();
                return;
            }

            // 检查点击位置是否在某个TextBlock上
            Point clickPoint = e.GetPosition(DestinationCanvas);

            // 应用逆变换来调整位置（考虑缩放和平移）
            clickPoint = ApplyInverseTransform(clickPoint);

            TextBlock clickedElement = null;

            // 遍历Canvas中的所有元素
            foreach (UIElement element in DestinationCanvas.Children)
            {
                if (element is TextBlock textBlock)
                {
                    double left = Canvas.GetLeft(textBlock);
                    double top = Canvas.GetTop(textBlock);

                    // 检查点击是否在TextBlock范围内
                    if (clickPoint.X >= left && clickPoint.X <= left + textBlock.ActualWidth &&
                        clickPoint.Y >= top && clickPoint.Y <= top + textBlock.ActualHeight)
                    {
                        clickedElement = textBlock;
                        break;
                    }
                }
            }

            // 如果点击了TextBlock，显示上下文菜单
            if (clickedElement != null)
            {
                ShowTextBlockContextMenu(clickedElement, e);
            }
            else
            {
                // 如果点击了空白区域，可以显示Canvas的上下文菜单
                ShowCanvasContextMenu(e);
            }

            // 标记事件已处理
            e.Handled = true;
        }

        // 显示TextBlock的上下文菜单
        private void ShowTextBlockContextMenu(TextBlock textBlock, MouseButtonEventArgs e)
        {
            // 创建上下文菜单
            ContextMenu contextMenu = new ContextMenu();

            // 添加"开始连接"菜单项
            MenuItem startConnectionItem = new MenuItem();
            startConnectionItem.Header = "开始连接";
            startConnectionItem.Click += (s, args) => StartConnection(textBlock);
            contextMenu.Items.Add(startConnectionItem);

            // 添加"编辑文本"菜单项
            MenuItem editTextItem = new MenuItem();
            editTextItem.Header = "编辑文本";
            editTextItem.Click += (s, args) => EditTextBlock(textBlock);
            contextMenu.Items.Add(editTextItem);

            // 添加"删除节点"菜单项
            MenuItem deleteItem = new MenuItem();
            deleteItem.Header = "删除节点";
            deleteItem.Click += (s, args) => DeleteTextBlock(textBlock);
            contextMenu.Items.Add(deleteItem);

            // 显示上下文菜单
            contextMenu.IsOpen = true;
        }

        // 显示Canvas的上下文菜单
        private void ShowCanvasContextMenu(MouseButtonEventArgs e)
        {
            // 创建上下文菜单
            ContextMenu contextMenu = new ContextMenu();

            // 添加"添加新节点"菜单项
            MenuItem addNodeItem = new MenuItem();
            addNodeItem.Header = "添加新节点";

            // 获取鼠标点击位置（考虑缩放和平移）
            Point clickPoint = e.GetPosition(DestinationCanvas);
            clickPoint = ApplyInverseTransform(clickPoint);

            addNodeItem.Click += (s, args) => AddNewTextBlock(clickPoint);
            contextMenu.Items.Add(addNodeItem);

            // 添加"重置视图"菜单项
            MenuItem resetViewItem = new MenuItem();
            resetViewItem.Header = "重置视图";
            resetViewItem.Click += (s, args) => ZoomReset_Click(s, args);
            contextMenu.Items.Add(resetViewItem);

            // 显示上下文菜单
            contextMenu.IsOpen = true;
        }

        // 编辑TextBlock文本
        private void EditTextBlock(TextBlock textBlock)
        {
            // 这里可以实现编辑文本的逻辑
            // 例如：弹出一个输入对话框

            // 简单实现：
            InputDialog dialog = new InputDialog("编辑节点文本", textBlock.Text);
            if (dialog.ShowDialog() == true)
            {
                textBlock.Text = dialog.Answer;
            }
        }

        // 删除TextBlock及其连接线
        private void DeleteTextBlock(TextBlock textBlock)
        {
            string elementId = textBlock.Tag.ToString();

            // 首先删除与此TextBlock相关的所有连接线
            if (_connectionLinesInfo.ContainsKey(elementId))
            {
                foreach (var lineInfo in _connectionLinesInfo[elementId].ToList())
                {
                    // 从画布中移除连接线
                    DestinationCanvas.Children.Remove(lineInfo.LinePath);

                    // 从另一端的连接信息中也移除此连接
                    string otherId = lineInfo.SourceId == elementId ? lineInfo.TargetId : lineInfo.SourceId;
                    if (_connectionLinesInfo.ContainsKey(otherId))
                    {
                        _connectionLinesInfo[otherId].Remove(lineInfo);
                    }

                    // 从ViewModel中移除连接
                    FamilyTreeWindowViewModel tmpviewModel = DataContext as FamilyTreeWindowViewModel;
                    tmpviewModel?.RemoveConnection(lineInfo.SourceId, lineInfo.TargetId);
                }

                // 清空此元素的连接信息
                _connectionLinesInfo.Remove(elementId);
            }

            // 从画布中移除TextBlock
            DestinationCanvas.Children.Remove(textBlock);
        }

        // 添加新的TextBlock
        private void AddNewTextBlock(Point position)
        {
            // 创建一个新的TextBlock
            TextBlock newTextBlock = new TextBlock
            {
                Text = "新节点",
                Background = Brushes.LightYellow,
                Padding = new Thickness(10),
                Tag = Guid.NewGuid().ToString() // 使用GUID作为唯一标识符
            };

            // 设置TextBlock的位置
            Canvas.SetLeft(newTextBlock, position.X);
            Canvas.SetTop(newTextBlock, position.Y);

            // 添加鼠标事件处理
            newTextBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
            newTextBlock.MouseMove += TextBlock_MouseMove;
            newTextBlock.MouseLeftButtonUp += TextBlock_MouseLeftButtonUp;

            // 初始化连接线信息字典
            _connectionLinesInfo[newTextBlock.Tag.ToString()] = new List<ConnectionLineInfo>();

            // 将TextBlock添加到Canvas
            DestinationCanvas.Children.Add(newTextBlock);

            // 添加到ViewModel
            FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
            viewModel?.NotifyTextBlockDropped(newTextBlock.Text, position ,newTextBlock.Tag.ToString());

            // 立即编辑新节点的文本
            EditTextBlock(newTextBlock);
        }

        // 添加TextBlock鼠标按下事件处理
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取触发事件的TextBlock
            TextBlock textBlock = sender as TextBlock;
            if (textBlock == null) return;

            // 如果按下了Ctrl键，则不做任何操作，因为Ctrl+左键是用于画布平移的
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }

            // 记录当前鼠标位置和偏移量
            _isMoving = true;
            _currentMovingElement = textBlock;

            // 获取鼠标在TextBlock内的相对位置（考虑缩放和平移）
            Point mousePosition = e.GetPosition(DestinationCanvas);
            mousePosition = ApplyInverseTransform(mousePosition);

            double left = Canvas.GetLeft(textBlock);
            double top = Canvas.GetTop(textBlock);

            _dragOffset = new Point(mousePosition.X - left, mousePosition.Y - top);

            // 捕获鼠标以跟踪移动
            textBlock.CaptureMouse();

            // 将TextBlock置于最前（提高Z-Index）
            Panel.SetZIndex(textBlock, 10);

            // 标记事件已处理
            e.Handled = true;
        }

        // 添加TextBlock鼠标释放事件处理
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock == null) return;

            // 如果处于移动状态，则结束移动操作
            if (_isMoving && _currentMovingElement == textBlock)
            {
                _isMoving = false;
                _currentMovingElement = null;
                textBlock.ReleaseMouseCapture();

                // 恢复默认Z-Index
                Panel.SetZIndex(textBlock, 1);

                // 更新ViewModel中的节点位置
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                if (viewModel != null && textBlock.Tag != null)
                {
                    double left = Canvas.GetLeft(textBlock);
                    double top = Canvas.GetTop(textBlock);
                    viewModel.UpdateTextBlockPosition(textBlock.Tag.ToString(), left, top);
                }
            }

            // 标记事件已处理
            e.Handled = true;
        }
    }
}
