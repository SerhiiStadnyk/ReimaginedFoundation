using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReimaginedFoundation
{
    public class ScrollArea
    {
        private Vector2 _scrollPosition;

        private const float SCROLLBAR_WIDTH = 16f;

        /*        public static void DrawScrollArea(Rect rect, ref Vector2 scrollPosition, int itemCount, float itemHeight, float itemWidth, float itemSpacing, Action<Rect> drawElementAction)
                {
                    float contentHeight = (itemHeight + itemSpacing) * itemCount;

                    // Virtual view height (to enable scrolling if needed)
                    Rect scrollViewRect = new Rect(0, 0, rect.width - SCROLLBAR_WIDTH, contentHeight);

                    // Begin scrolling view
                    GUI.BeginScrollView(rect.AtZero(), scrollPosition, scrollViewRect);
                    //Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, scrollViewRect);

                    // Draw the content within the scrolling view
                    Rect itemRect = new Rect(0, 0, itemWidth, itemHeight);
                    float step = itemHeight + itemSpacing;
                    for (int i = 0; i < itemCount; i++)
                    {
                        drawElementAction?.Invoke(itemRect);
                        itemRect.y += step;
                    }

                    //Widgets.EndScrollView();
                    GUI.EndGroup();
                }*/

        private Vector2 GetPivotOffset(Vector2 pivot, float remainingSpaceX, float remainingSpaceY)
        {
            Vector2 pivotOffset = Vector2.zero;

            // X-axis alignment
            if (pivot.x == 0f) // Left-aligned
            {
                pivotOffset.x = 0f;
            }
            else if (pivot.x == 0.5f) // Centered
            {
                pivotOffset.x = remainingSpaceX / 2;
            }
            else if (pivot.x == 1f) // Right-aligned
            {
                pivotOffset.x = remainingSpaceX;
            }
            else // Default: Centered if invalid
            {
                pivotOffset.x = remainingSpaceX / 2;
            }

            // Y-axis alignment
            if (pivot.y == 0f) // Top-aligned
            {
                pivotOffset.y = 0f;
            }
            else if (pivot.y == 0.5f) // Centered
            {
                pivotOffset.y = remainingSpaceY / 2;
            }
            else if (pivot.y == 1f) // Bottom-aligned
            {
                pivotOffset.y = remainingSpaceY;
            }
            else // Default: Centered if invalid
            {
                pivotOffset.y = remainingSpaceY / 2;
            }

            return pivotOffset;
        }

        public void DrawGrid(Rect rect, List<IDraweable> items, float spacing, Vector2 pivot = default)
        {
            if (items == null || items.Count == 0)
            {
                Log.Warning("ScrollArea: items list is null or empty. Nothing will be drawn.");
                return;
            }

            // Default pivot = Vector2.zero (top-left)
            pivot = pivot == default ? Vector2.zero : pivot;

            // Get item rect dimensions
            Rect itemRect = items[0].Rect;

            // Draw background
            Widgets.DrawRectFast(rect, Widgets.MenuSectionBGFillColor);

            // Calculate the available width for placing items
            float availableWidth = rect.width - SCROLLBAR_WIDTH;

            // Calculate the maximum number of items per row
            int itemsPerRow = Mathf.FloorToInt((availableWidth + spacing) / (itemRect.width + spacing));
            itemsPerRow = Mathf.Max(1, itemsPerRow); // Ensure at least 1 item per row

            // Recalculate actual spacing to avoid overshooting
            float totalItemsWidth = itemsPerRow * itemRect.width + (itemsPerRow - 1) * spacing;
            float remainingSpace = availableWidth - totalItemsWidth;

            // Adjust horizontal offset based on pivot.x
            Vector2 pivotOffset = GetPivotOffset(pivot, remainingSpace, 0);

            // Calculate content height
            int rowCount = Mathf.CeilToInt((float)items.Count / itemsPerRow);
            float contentHeight = rowCount * (itemRect.height + spacing) - spacing;

            // Rect representing scrollable content
            Rect scrollViewRect = new Rect(0, 0, rect.width - SCROLLBAR_WIDTH, contentHeight);

            // Begin scrolling view
            Widgets.BeginScrollView(rect, ref _scrollPosition, scrollViewRect);

            // Draw each item at its calculated grid position
            float xOffset;
            float yOffset;
            for (int i = 0; i < items.Count; i++)
            {
                // Calculate position in grid
                xOffset = (i % itemsPerRow) * (itemRect.width + spacing) + pivotOffset.x;
                yOffset = (i / itemsPerRow) * (itemRect.height + spacing);

                // Position the item rect
                itemRect.position = new Vector2(xOffset, yOffset);

                // Draw the item
                items[i].Draw(itemRect);
            }

            Widgets.EndScrollView();
        }
    }
}
