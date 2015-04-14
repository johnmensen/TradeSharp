using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    //public partial class ChartToolsSetupForm
    //{
    //    private TreeNode dragNode;
    //    private readonly ImageList imageListDrag = new ImageList();

    //    private void TreeButtonsItemDrag(object sender, ItemDragEventArgs e)
    //    {
    //        // Get drag node and select it
    //        dragNode = (TreeNode)e.Item;
    //        treeButtons.SelectedNode = dragNode;

    //        // Reset image list used for drag image
    //        imageListDrag.Images.Clear();
    //        imageListDrag.ImageSize =
    //              new Size(dragNode.Bounds.Size.Width
    //              + treeButtons.Indent, dragNode.Bounds.Height);

    //        // Create new bitmap
    //        // This bitmap will contain the tree node image to be dragged
    //        var bmp = new Bitmap(dragNode.Bounds.Width
    //            + treeButtons.Indent, dragNode.Bounds.Height);

    //        // Get graphics from bitmap
    //        var gfx = Graphics.FromImage(bmp);

    //        // Draw node icon into the bitmap
    //        gfx.DrawImage(lstIcons.Images[0], 0, 0);

    //        // Draw node label into bitmap
    //        gfx.DrawString(dragNode.Text,
    //            treeButtons.Font,
    //            new SolidBrush(treeButtons.ForeColor),
    //            treeButtons.Indent, 1.0f);

    //        // Add bitmap to imagelist
    //        imageListDrag.Images.Add(bmp);

    //        // Get mouse position in client coordinates
    //        var p = treeButtons.PointToClient(MousePosition);

    //        // Compute delta between mouse position and node bounds
    //        int dx = p.X + treeButtons.Indent - dragNode.Bounds.Left;
    //        int dy = p.Y - dragNode.Bounds.Top;

    //        // Begin dragging image
    //        if (DragHelper.ImageList_BeginDrag(imageListDrag.Handle, 0, dx, dy))
    //        {
    //            // Begin dragging
    //            treeButtons.DoDragDrop(bmp, DragDropEffects.Move);
    //            // End dragging image
    //            DragHelper.ImageList_EndDrag();
    //        }
    //    }

    //    private void TreeButtonsDragOver(object sender, DragEventArgs e)
    //    {
    //        var formP = PointToClient(new Point(e.X, e.Y));
    //        DragHelper.ImageList_DragMove(formP.X - treeButtons.Left,
    //                                      formP.Y - treeButtons.Top);
    //    }

    //    private void TreeButtonsGiveFeedback(object sender, GiveFeedbackEventArgs e)
    //    {
    //        if (e.Effect == DragDropEffects.Move)
    //        {
    //            // Show pointer cursor while dragging
    //            e.UseDefaultCursors = false;
    //            treeButtons.Cursor = Cursors.Default;
    //        }
    //        else e.UseDefaultCursors = true;
    //    }

    //    private void TreeButtonsDragDrop(object sender, DragEventArgs e)
    //    {
    //        // Unlock updates
    //        DragHelper.ImageList_DragLeave(treeButtons.Handle);
    //    }
    //}

    //public class DragHelper
    //{
    //    [DllImport("comctl32.dll")]
    //    public static extern bool InitCommonControls();

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern bool ImageList_BeginDrag(
    //        IntPtr himlTrack, // Handler of the image list containing the image to drag
    //        int iTrack,       // Index of the image to drag 
    //        int dxHotspot,    // x-delta between mouse position and drag image
    //        int dyHotspot     // y-delta between mouse position and drag image
    //    );

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern bool ImageList_DragMove(
    //        int x,   // X-coordinate (relative to the form,
    //                 // not the treeview) at which to display the drag image.
    //        int y   // Y-coordinate (relative to the form,
    //                 // not the treeview) at which to display the drag image.
    //    );

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern void ImageList_EndDrag();

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern bool ImageList_DragEnter(
    //        IntPtr hwndLock,  // Handle to the control that owns the drag image.
    //        int x,            // X-coordinate (relative to the treeview)
    //                          // at which to display the drag image. 
    //        int y             // Y-coordinate (relative to the treeview)
    //                          // at which to display the drag image. 
    //    );

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern bool ImageList_DragLeave(
    //        IntPtr hwndLock  // Handle to the control that owns the drag image.
    //    );

    //    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    //    public static extern bool ImageList_DragShowNolock(
    //        bool fShow       // False to hide, true to show the image
    //    );

    //    static DragHelper()
    //    {
    //        InitCommonControls();
    //    }
    //}

    public partial class ChartToolsSetupForm
    {
        private string nodeMap;
        private StringBuilder newNodeMap = new StringBuilder(512);
        
        private void TreeButtonsItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TreeButtonsDragOver(object sender, DragEventArgs e)
        {
            TreeNode nodeOver = treeButtons.GetNodeAt(treeButtons.PointToClient(Cursor.Position));
            var nodeMoving = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");


            // A bit long, but to summarize, process the following code only if the nodeover is null
            // and either the nodeover is not the same thing as nodemoving UNLESSS nodeover happens
            // to be the last node in the branch (so we can allow drag & drop below a parent branch)
            if (nodeOver != null && (nodeOver != nodeMoving || (nodeOver.Parent != null && 
                nodeOver.Index == (nodeOver.Parent.Nodes.Count - 1))))
            {
                int offsetY = treeButtons.PointToClient(Cursor.Position).Y - nodeOver.Bounds.Top;
                int nodeOverImageWidth = treeButtons.ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;
                var g = treeButtons.CreateGraphics();

                // Image index of 1 is the non-folder icon
                if (nodeOver.ImageIndex == 1)
                {
                    if (offsetY < (nodeOver.Bounds.Height / 2))
                    {
                        var tnParadox = nodeOver;
                        while (tnParadox.Parent != null)
                        {
                            if (tnParadox.Parent == nodeMoving)
                            {
                                nodeMap = "";
                                return;
                            }

                            tnParadox = tnParadox.Parent;
                        }
                                                
                        SetNewNodeMap(nodeOver, false);
                        if (SetMapsEqual())
                            return;
                        
                        Refresh();
                        DrawLeafTopPlaceholders(nodeOver);
                        
                    }
                    else
                    {
                        var tnParadox = nodeOver;
                        while (tnParadox.Parent != null)
                        {
                            if (tnParadox.Parent == nodeMoving)
                            {
                                nodeMap = "";
                                return;
                            }

                            tnParadox = tnParadox.Parent;
                        }
                        
                        TreeNode parentDragDrop = null;
                        // If the node the mouse is over is the last node of the branch we should allow
                        // the ability to drop the "nodemoving" node BELOW the parent node
                        if (nodeOver.Parent != null && nodeOver.Index == (nodeOver.Parent.Nodes.Count - 1))
                        {
                            int xPos = treeButtons.PointToClient(Cursor.Position).X;
                            if (xPos < nodeOver.Bounds.Left)
                            {
                                parentDragDrop = nodeOver.Parent;

                                if (xPos < (parentDragDrop.Bounds.Left - treeButtons.ImageList.Images[parentDragDrop.ImageIndex].Size.Width))
                                {
                                    if (parentDragDrop.Parent != null)
                                        parentDragDrop = parentDragDrop.Parent;
                                }
                            }
                        }
                        
                        // Since we are in a special case here, use the ParentDragDrop node as the current "nodeover"
                        SetNewNodeMap(parentDragDrop != null ? parentDragDrop : nodeOver, true);
                        if (SetMapsEqual()) return;
                        
                        Refresh();
                        DrawLeafBottomPlaceholders(nodeOver, parentDragDrop);                        
                    }
                    
                }
                else
                {
                    if (offsetY < (nodeOver.Bounds.Height / 3))
                    {
                        var tnParadox = nodeOver;
                        while (tnParadox.Parent != null)
                        {
                            if (tnParadox.Parent == nodeMoving)
                            {
                                nodeMap = "";
                                return;
                            }

                            tnParadox = tnParadox.Parent;
                        }
                        
                        SetNewNodeMap(nodeOver, false);
                        if (SetMapsEqual()) return;
                        
                        Refresh();
                        DrawFolderTopPlaceholders(nodeOver);
                        
                    }
                    else if ((nodeOver.Parent != null && nodeOver.Index == 0) && (offsetY > (nodeOver.Bounds.Height - (nodeOver.Bounds.Height / 3))))
                    {
                        var tnParadox = nodeOver;
                        while (tnParadox.Parent != null)
                        {
                            if (tnParadox.Parent == nodeMoving)
                            {
                                nodeMap = "";
                                return;
                            }

                            tnParadox = tnParadox.Parent;
                        }
                        
                        SetNewNodeMap(nodeOver, true);
                        if (SetMapsEqual()) return;
                        
                        Refresh();
                        DrawFolderTopPlaceholders(nodeOver);
                        
                    }
                    else
                    {
                        
                        if (nodeOver.Nodes.Count > 0) nodeOver.Expand();                                                
                        else
                        {
                            if (nodeMoving == nodeOver)
                                return;
                            
                            var tnParadox = nodeOver;
                            while (tnParadox.Parent != null)
                            {
                                if (tnParadox.Parent == nodeMoving)
                                {
                                    nodeMap = "";
                                    return;
                                }

                                tnParadox = tnParadox.Parent;
                            }
                            
                            SetNewNodeMap(nodeOver, false);
                            newNodeMap = newNodeMap.Insert(newNodeMap.Length, "|0");

                            if (SetMapsEqual())
                                return;
                            
                            Refresh();
                            DrawAddToFolderPlaceholder(nodeOver);                            
                        }
                    }
                }
            }
        }

        private void TreeButtonsGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                // Show pointer cursor while dragging
                e.UseDefaultCursors = false;
                treeButtons.Cursor = Cursors.Default;
            }
            else e.UseDefaultCursors = true;
        }

        private void TreeButtonsDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false) && 
                !string.IsNullOrEmpty(nodeMap))
            {
                var movingNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                var nodeIndexes = nodeMap.Split('|');
                var insertCollection = treeButtons.Nodes;
                TreeNode topNode = null;
                for (var i = 0; i < nodeIndexes.Length - 1; i++)
                {
                    topNode = insertCollection[nodeIndexes[i].ToInt()];
                    insertCollection = topNode.Nodes;
                }

                // проверить родительский узел, в который будет вставлена кнопка
                // нельзя вставить кнопку в кнопку
                if (topNode != null)
                    if (topNode.Tag is ChartToolButtonSettings)
                    {// отмена
                        return;
                    }

                // поменять группу у кнопки
                var btn = (ChartToolButtonSettings) movingNode.Tag;
                btn.Group = topNode == null ? null : (ChartToolButtonGroup)topNode.Tag;

                insertCollection.Insert(nodeIndexes[nodeIndexes.Length - 1].ToInt(), (TreeNode)movingNode.Clone());
                treeButtons.SelectedNode = insertCollection[Int32.Parse(nodeIndexes[nodeIndexes.Length - 1])];
                movingNode.Remove();
            }    
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void TreeButtonsDragEnter(object sender, DragEventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            e.Effect = DragDropEffects.Move;
        }

        private void DrawLeafTopPlaceholders(TreeNode nodeOver)
        {
            var g = treeButtons.CreateGraphics();
            int nodeOverImageWidth = treeButtons.ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;
            int leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            int rightPos = treeButtons.Width - 4;

            var leftTriangle = new []{
												   new Point(leftPos, nodeOver.Bounds.Top - 4),
												   new Point(leftPos, nodeOver.Bounds.Top + 4),
												   new Point(leftPos + 4, nodeOver.Bounds.Y),
												   new Point(leftPos + 4, nodeOver.Bounds.Top - 1),
												   new Point(leftPos, nodeOver.Bounds.Top - 5)};

            var rightTriangle = new []{
													new Point(rightPos, nodeOver.Bounds.Top - 4),
													new Point(rightPos, nodeOver.Bounds.Top + 4),
													new Point(rightPos - 4, nodeOver.Bounds.Y),
													new Point(rightPos - 4, nodeOver.Bounds.Top - 1),
													new Point(rightPos, nodeOver.Bounds.Top - 5)};


            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Top), 
                new Point(rightPos, nodeOver.Bounds.Top));

        }

        private void DrawLeafBottomPlaceholders(TreeNode nodeOver, TreeNode parentDragDrop)
        {
            var g = treeButtons.CreateGraphics();

            int nodeOverImageWidth = treeButtons.ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;
            // Once again, we are not dragging to node over, draw the placeholder using the ParentDragDrop bounds
            int leftPos;
            if (parentDragDrop != null)
                leftPos = parentDragDrop.Bounds.Left - (treeButtons.ImageList.Images[parentDragDrop.ImageIndex].Size.Width + 8);
            else
                leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            int rightPos = treeButtons.Width - 4;

            var leftTriangle = new []{
												   new Point(leftPos, nodeOver.Bounds.Bottom - 4),
												   new Point(leftPos, nodeOver.Bounds.Bottom + 4),
												   new Point(leftPos + 4, nodeOver.Bounds.Bottom),
												   new Point(leftPos + 4, nodeOver.Bounds.Bottom - 1),
												   new Point(leftPos, nodeOver.Bounds.Bottom - 5)};

            var rightTriangle = new []{
													new Point(rightPos, nodeOver.Bounds.Bottom - 4),
													new Point(rightPos, nodeOver.Bounds.Bottom + 4),
													new Point(rightPos - 4, nodeOver.Bounds.Bottom),
													new Point(rightPos - 4, nodeOver.Bounds.Bottom - 1),
													new Point(rightPos, nodeOver.Bounds.Bottom - 5)};


            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Bottom),
                new Point(rightPos, nodeOver.Bounds.Bottom));
        }

        private void DrawFolderTopPlaceholders(TreeNode nodeOver)
        {
            var g = treeButtons.CreateGraphics();
            int nodeOverImageWidth = treeButtons.ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;

            int leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            int rightPos = treeButtons.Width - 4;

            var leftTriangle = new []{
												   new Point(leftPos, nodeOver.Bounds.Top - 4),
												   new Point(leftPos, nodeOver.Bounds.Top + 4),
												   new Point(leftPos + 4, nodeOver.Bounds.Y),
												   new Point(leftPos + 4, nodeOver.Bounds.Top - 1),
												   new Point(leftPos, nodeOver.Bounds.Top - 5)};

            var rightTriangle = new []{
													new Point(rightPos, nodeOver.Bounds.Top - 4),
													new Point(rightPos, nodeOver.Bounds.Top + 4),
													new Point(rightPos - 4, nodeOver.Bounds.Y),
													new Point(rightPos - 4, nodeOver.Bounds.Top - 1),
													new Point(rightPos, nodeOver.Bounds.Top - 5)};


            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Top), 
                new Point(rightPos, nodeOver.Bounds.Top));

        }

        private void DrawAddToFolderPlaceholder(TreeNode nodeOver)
        {
            var g = treeButtons.CreateGraphics();
            int rightPos = nodeOver.Bounds.Right + 6;
            var rightTriangle = new []{
													new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height / 2) + 4),
													new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height / 2) + 4),
													new Point(rightPos - 4, nodeOver.Bounds.Y + (nodeOver.Bounds.Height / 2)),
													new Point(rightPos - 4, nodeOver.Bounds.Y + (nodeOver.Bounds.Height / 2) - 1),
													new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height / 2) - 5)};

            Refresh();
            g.FillPolygon(Brushes.Black, rightTriangle);
        }

        private void SetNewNodeMap(TreeNode tnNode, bool boolBelowNode)
        {
            newNodeMap.Length = 0;

            if (boolBelowNode)
                newNodeMap.Insert(0, tnNode.Index + 1);
            else
                newNodeMap.Insert(0, tnNode.Index);
            TreeNode tnCurNode = tnNode;

            while (tnCurNode.Parent != null)
            {
                tnCurNode = tnCurNode.Parent;

                if (newNodeMap.Length == 0 && boolBelowNode)
                {
                    newNodeMap.Insert(0, (tnCurNode.Index + 1) + "|");
                }
                else
                {
                    newNodeMap.Insert(0, tnCurNode.Index + "|");
                }
            }
        }
    
        private bool SetMapsEqual()
		{
			if(newNodeMap.ToString() == nodeMap)
				return true;
			nodeMap = newNodeMap.ToString();
			return false;
		}
    }
}
